[CmdletBinding()]
param (
    [string]$Root,
    [string]$Output,
    [switch]$IncludePrivate,
    [switch]$IncludeGenerated,
    [switch]$IncludeAssemblyInfo,
    [string[]]$ExcludeDirectories = @('.git', '.vs', 'bin', 'obj', 'packages', 'node_modules', 'TestResults', '.dotnet-cli', '.dotnet-sdk', 'artifacts'),
    [string[]]$ExcludeFilePatterns = @('*.g.cs', '*.g.i.cs', '*.designer.cs')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-RepositoryRoot {
    param([string]$ExplicitRoot)

    if ($ExplicitRoot) {
        return (Resolve-Path -LiteralPath $ExplicitRoot).Path
    }

    $candidates = @()
    if ($PSScriptRoot) {
        $candidates += $PSScriptRoot
        $candidates += (Split-Path -Parent $PSScriptRoot)
    }
    $candidates += (Get-Location).Path

    foreach ($candidate in ($candidates | Where-Object { $_ } | Select-Object -Unique)) {
        $resolved = Resolve-Path -LiteralPath $candidate -ErrorAction SilentlyContinue
        if (-not $resolved) { continue }

        $dir = [System.IO.DirectoryInfo]$resolved.Path
        while ($dir) {
            $hasSolution = @(Get-ChildItem -LiteralPath $dir.FullName -Filter *.sln -File -ErrorAction SilentlyContinue).Count -gt 0
            $hasProjects = (Test-Path (Join-Path $dir.FullName 'DataVisualiser')) -or (Test-Path (Join-Path $dir.FullName 'DataFileReader'))
            if ($hasSolution -or $hasProjects) {
                return $dir.FullName
            }
            $dir = $dir.Parent
        }
    }

    throw 'Unable to resolve repository root. Pass -Root explicitly.'
}

function Resolve-OutputPath {
    param([string]$ResolvedRoot, [string]$ExplicitOutput, [string]$DefaultName)

    if ([string]::IsNullOrWhiteSpace($ExplicitOutput)) {
        return (Join-Path $ResolvedRoot $DefaultName)
    }

    if ([System.IO.Path]::IsPathRooted($ExplicitOutput)) {
        return $ExplicitOutput
    }

    return (Join-Path $ResolvedRoot $ExplicitOutput)
}

function Get-RelativePathCompat {
    param(
        [Parameter(Mandatory=$true)][string]$BasePath,
        [Parameter(Mandatory=$true)][string]$TargetPath
    )

    try {
        return [System.IO.Path]::GetRelativePath($BasePath, $TargetPath)
    }
    catch {
        $baseResolved = [System.IO.Path]::GetFullPath($BasePath)
        $targetResolved = [System.IO.Path]::GetFullPath($TargetPath)

        if (-not $baseResolved.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
            $baseResolved += [System.IO.Path]::DirectorySeparatorChar
        }

        $baseUri = New-Object System.Uri($baseResolved)
        $targetUri = New-Object System.Uri($targetResolved)
        $relativeUri = $baseUri.MakeRelativeUri($targetUri)
        $relative = [System.Uri]::UnescapeDataString($relativeUri.ToString())
        return ($relative -replace '/', '\')
    }
}

function Test-PathExcluded {
    param(
        [string]$FullName,
        [bool]$IsContainer,
        [string]$Name,
        [string]$ResolvedRoot,
        [string[]]$ExcludedDirectories,
        [string[]]$ExcludedFilePatterns
    )

    $relative = Get-RelativePathCompat -BasePath $ResolvedRoot -TargetPath $FullName
    $segments = $relative -split '[\\/]'

    foreach ($segment in $segments) {
        if ($ExcludedDirectories -contains $segment) {
            return $true
        }
    }

    if (-not $IsContainer) {
        foreach ($pattern in $ExcludedFilePatterns) {
            if ($Name -like $pattern) {
                return $true
            }
        }
    }

    return $false
}

function Get-SafeFiles {
    param(
        [string]$ResolvedRoot,
        [string[]]$ExcludedDirectories,
        [string[]]$ExcludedFilePatterns
    )

    $results = New-Object System.Collections.Generic.List[object]
    $queue = New-Object System.Collections.Queue
    $queue.Enqueue($ResolvedRoot)

    while ($queue.Count -gt 0) {
        $currentDir = [string]$queue.Dequeue()

        try {
            $children = Get-ChildItem -LiteralPath $currentDir -Force -ErrorAction Stop
        }
        catch {
            Write-Warning ("Skipping inaccessible path: {0}" -f $currentDir)
            continue
        }

        foreach ($child in $children) {
            $isContainer = [bool]$child.PSIsContainer
            if (Test-PathExcluded -FullName $child.FullName `
                                  -IsContainer $isContainer `
                                  -Name $child.Name `
                                  -ResolvedRoot $ResolvedRoot `
                                  -ExcludedDirectories $ExcludedDirectories `
                                  -ExcludedFilePatterns $ExcludeFilePatterns) {
                continue
            }

            if ($isContainer) {
                $attributes = $child.Attributes
                $isReparsePoint = (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)
                if (-not $isReparsePoint) {
                    $queue.Enqueue($child.FullName)
                }
            }
            else {
                $results.Add($child)
            }
        }
    }

    return $results
}

function Get-NamespaceInfo {
    param([string]$Content)

    $fileScoped = [regex]::Match($Content, '(?m)^\s*namespace\s+([A-Za-z_][A-Za-z0-9_\.]*)\s*;')
    if ($fileScoped.Success) {
        return [pscustomobject]@{
            Name = $fileScoped.Groups[1].Value
            Kind = 'file-scoped'
        }
    }

    $blockScoped = [regex]::Match($Content, '(?m)^\s*namespace\s+([A-Za-z_][A-Za-z0-9_\.]*)\s*$')
    if ($blockScoped.Success) {
        return [pscustomobject]@{
            Name = $blockScoped.Groups[1].Value
            Kind = 'block-scoped'
        }
    }

    return [pscustomobject]@{
        Name = '(global)'
        Kind = 'global'
    }
}

function Get-ProjectNameFromRelativePath {
    param([string]$RelativePath)
    $parts = $RelativePath -split '[\\/]'
    if ($parts.Length -gt 0) { return $parts[0] }
    return ''
}

function Get-Visibility {
    param([string]$PrefixText)

    if ($PrefixText -match '\bprotected\s+internal\b') { return 'protected internal' }
    if ($PrefixText -match '\bprivate\s+protected\b')  { return 'private protected' }
    if ($PrefixText -match '\bpublic\b')               { return 'public' }
    if ($PrefixText -match '\binternal\b')             { return 'internal' }
    if ($PrefixText -match '\bprotected\b')            { return 'protected' }
    if ($PrefixText -match '\bprivate\b')              { return 'private' }

    return 'private'
}

function Get-LineNumber {
    param(
        [string]$Content,
        [int]$Index
    )

    if ($Index -le 0) { return 1 }
    $prefix = $Content.Substring(0, [Math]::Min($Index, $Content.Length))
    return ([regex]::Matches($prefix, "`r`n|`n|`r").Count + 1)
}

function Strip-CommentsAndStrings {
    param([string]$Content)

    $chars = $Content.ToCharArray()
    $inLineComment = $false
    $inBlockComment = $false
    $inString = $false
    $stringChar = [char]0
    $escape = $false
    $verbatim = $false

    for ($i = 0; $i -lt $chars.Length; $i++) {
        $c = $chars[$i]
        $next = if ($i + 1 -lt $chars.Length) { $chars[$i + 1] } else { [char]0 }

        if ($inLineComment) {
            if ($c -eq "`n") {
                $inLineComment = $false
            }
            elseif ($c -ne "`r") {
                $chars[$i] = ' '
            }
            continue
        }

        if ($inBlockComment) {
            if ($c -eq '*' -and $next -eq '/') {
                $chars[$i] = ' '
                if ($i + 1 -lt $chars.Length) { $chars[$i + 1] = ' ' }
                $inBlockComment = $false
                $i++
            }
            else {
                if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
            }
            continue
        }

        if ($inString) {
            if ($verbatim) {
                if ($c -eq '"' -and $next -eq '"') {
                    if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
                    if ($i + 1 -lt $chars.Length) { $chars[$i + 1] = ' ' }
                    $i++
                    continue
                }
                if ($c -eq '"') {
                    if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
                    $inString = $false
                    $verbatim = $false
                    $stringChar = [char]0
                    continue
                }
                if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
                continue
            }

            if ($escape) {
                if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
                $escape = $false
                continue
            }

            if ($c -eq '\') {
                if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
                $escape = $true
                continue
            }

            if ($c -eq $stringChar) {
                if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
                $inString = $false
                $stringChar = [char]0
                continue
            }

            if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
            continue
        }

        if ($c -eq '/' -and $next -eq '/') {
            $chars[$i] = ' '
            if ($i + 1 -lt $chars.Length) { $chars[$i + 1] = ' ' }
            $inLineComment = $true
            $i++
            continue
        }

        if ($c -eq '/' -and $next -eq '*') {
            $chars[$i] = ' '
            if ($i + 1 -lt $chars.Length) { $chars[$i + 1] = ' ' }
            $inBlockComment = $true
            $i++
            continue
        }

        if ($c -eq '@' -and $next -eq '"') {
            $chars[$i] = ' '
            if ($i + 1 -lt $chars.Length) { $chars[$i + 1] = ' ' }
            $inString = $true
            $verbatim = $true
            $stringChar = '"'
            $i++
            continue
        }

        if ($c -eq '"' -or $c -eq "'") {
            if ($c -ne "`r" -and $c -ne "`n") { $chars[$i] = ' ' }
            $inString = $true
            $verbatim = $false
            $stringChar = $c
            continue
        }
    }

    return -join $chars
}

function Find-TypeBodyStart {
    param(
        [string]$SanitizedContent,
        [int]$DeclarationIndex
    )

    for ($i = $DeclarationIndex; $i -lt $SanitizedContent.Length; $i++) {
        $c = $SanitizedContent[$i]
        if ($c -eq '{') {
            return $i
        }
        if ($c -eq ';') {
            return -1
        }
    }

    return -1
}

function Get-TypeDeclarations {
    param([string]$Content)

    $sanitized = Strip-CommentsAndStrings -Content $Content
    $pattern = '(?m)^\s*(?:(?<modifiers>(?:(?:public|internal|private|protected|static|abstract|sealed|partial|readonly|unsafe|new)\s+)+))?(?<kind>class|interface|struct|record|enum)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)'
    $matches = [regex]::Matches($sanitized, $pattern)

    $decls = New-Object System.Collections.Generic.List[object]
    $bodyStarts = @()

    foreach ($match in $matches) {
        $bodyStart = Find-TypeBodyStart -SanitizedContent $sanitized -DeclarationIndex $match.Index
        $bodyStarts += $bodyStart
    }

    for ($i = 0; $i -lt $matches.Count; $i++) {
        $match = $matches[$i]
        $bodyStart = $bodyStarts[$i]
        $parentName = ''
        $scope = 'top-level'

        if ($bodyStart -ge 0) {
            for ($j = 0; $j -lt $i; $j++) {
                $candidateBodyStart = $bodyStarts[$j]
                if ($candidateBodyStart -ge 0 -and $candidateBodyStart -lt $match.Index) {
                    $candidateName = $matches[$j].Groups['name'].Value
                    $candidateEnd = if ($j + 1 -lt $matches.Count) { $matches[$j + 1].Index } else { $sanitized.Length }
                    $segmentStart = $candidateBodyStart
                    $segmentLength = [Math]::Max(0, [Math]::Min($candidateEnd, $sanitized.Length) - $segmentStart)
                    if ($segmentLength -gt 0) {
                        $segment = $sanitized.Substring($segmentStart, $segmentLength)
                        $openCount = ([regex]::Matches($segment, '\{')).Count
                        $closeCount = ([regex]::Matches($segment, '\}')).Count
                        if ($openCount -gt $closeCount) {
                            $parentName = $candidateName
                        }
                    }
                }
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($parentName)) {
            $scope = 'nested'
        }

        $decls.Add([pscustomobject]@{
            Kind       = $match.Groups['kind'].Value
            Name       = $match.Groups['name'].Value
            Modifiers  = $match.Groups['modifiers'].Value
            Index      = $match.Index
            Line       = Get-LineNumber -Content $Content -Index $match.Index
            Scope      = $scope
            ParentType = $(if ($parentName) { $parentName } else { '' })
        })
    }

    return $decls
}

$resolvedRoot = Resolve-RepositoryRoot -ExplicitRoot $Root
$outputPath = Resolve-OutputPath -ResolvedRoot $resolvedRoot -ExplicitOutput $Output -DefaultName 'codebase-index.md'

$files = Get-SafeFiles -ResolvedRoot $resolvedRoot `
                       -ExcludedDirectories $ExcludeDirectories `
                       -ExcludedFilePatterns $ExcludeFilePatterns |
    Where-Object { $_.Extension -eq '.cs' }

if (-not $IncludeGenerated) {
    $files = $files | Where-Object {
        $_.Name -notlike '*.g.cs' -and
        $_.Name -notlike '*.g.i.cs' -and
        $_.Name -notlike '*.designer.cs'
    }
}

if (-not $IncludeAssemblyInfo) {
    $files = $files | Where-Object { $_.Name -ne 'AssemblyInfo.cs' }
}

$symbols = New-Object System.Collections.Generic.List[object]

foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrWhiteSpace($content)) {
        continue
    }

    $relativePath = Get-RelativePathCompat -BasePath $resolvedRoot -TargetPath $file.FullName
    $project = Get-ProjectNameFromRelativePath -RelativePath $relativePath
    $nsInfo = Get-NamespaceInfo -Content $content
    $decls = Get-TypeDeclarations -Content $content

    foreach ($decl in $decls) {
        $visibility = Get-Visibility -PrefixText $decl.Modifiers
        if (-not $IncludePrivate -and $visibility -eq 'private') {
            continue
        }

        $symbols.Add([pscustomobject]@{
            Project          = $project
            Kind             = $decl.Kind
            Name             = $decl.Name
            Visibility       = $visibility
            DeclarationScope = $decl.Scope
            ParentType       = $decl.ParentType
            Namespace        = $nsInfo.Name
            NamespaceKind    = $nsInfo.Kind
            Line             = $decl.Line
            File             = ('.\' + ($relativePath -replace '/', '\'))
        })
    }
}

$ordered = $symbols | Sort-Object Project, Namespace, DeclarationScope, ParentType, Kind, Name, File, Line
$projectCounts = $ordered | Group-Object Project | Sort-Object Name
$scopeCounts = $ordered | Group-Object DeclarationScope | Sort-Object Name
$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Codebase Index')
$lines.Add('')
$lines.Add(("Generated: {0}  " -f $timestamp))
$lines.Add(("Root: {0}" -f $resolvedRoot))
$lines.Add('')
$lines.Add('This file is auto-generated.')
$lines.Add('')
$lines.Add('**Scope**')
$lines.Add('- Declared symbols only')
$lines.Add('- No inference')
$lines.Add('- No semantic interpretation')
$lines.Add(('- Private declarations included: {0}' -f ([bool]$IncludePrivate)))
$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Summary')
$lines.Add('')

foreach ($group in $projectCounts) {
    $lines.Add(('- {0}: {1} symbols' -f $group.Name, $group.Count))
}

$lines.Add('')
foreach ($group in $scopeCounts) {
    $lines.Add(('- {0}: {1} symbols' -f $group.Name, $group.Count))
}

$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('| Project | Kind | Name | Visibility | DeclarationScope | ParentType | Namespace | NamespaceKind | Line | File |')
$lines.Add('|---------|------|------|------------|------------------|------------|-----------|---------------|------|------|')

foreach ($row in $ordered) {
    $parent = if ([string]::IsNullOrWhiteSpace($row.ParentType)) { '' } else { $row.ParentType }
    $lines.Add(("| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} |" -f $row.Project, $row.Kind, $row.Name, $row.Visibility, $row.DeclarationScope, $parent, $row.Namespace, $row.NamespaceKind, $row.Line, $row.File))
}

$lines.Add('')
$lines.Add('## Notes')
$lines.Add('')
$lines.Add('- `DeclarationScope = top-level` means the declaration was not detected inside another type declaration.')
$lines.Add('- `DeclarationScope = nested` means the declaration was detected within the open body of another type.')
$lines.Add('- `ParentType` is best-effort and only populated when nested scope could be inferred.')
$lines.Add('- `NamespaceKind` distinguishes file-scoped, block-scoped, and global namespace forms.')
$lines.Add('- This index preserves detail rather than suppressing helper or nested declarations.')
$lines.Add('- It is still a declaration scraper, not a compiler-backed semantic index.')
$lines.Add('')
$lines.Add('End of codebase-index.md')

$lines | Set-Content -LiteralPath $outputPath -Encoding UTF8
Write-Host ("Generated {0}" -f $outputPath)
