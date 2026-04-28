[CmdletBinding()]
param (
    [string]$Root,
    [string]$Output,
    [switch]$IncludePrivate,
    [switch]$IncludeGenerated,
    [switch]$IncludeAssemblyInfo,
    [int]$TopDependencyCount = 40,
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

function Get-TypeDeclarations {
    param([string]$Content)

    $sanitized = Strip-CommentsAndStrings -Content $Content
    $pattern = '(?m)^\s*(?:(?<modifiers>(?:(?:public|internal|private|protected|static|abstract|sealed|partial|readonly|unsafe|new)\s+)+))?(?<kind>class|interface|struct|record|enum)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)'
    $matches = [regex]::Matches($sanitized, $pattern)
    $decls = New-Object System.Collections.Generic.List[object]

    foreach ($match in $matches) {
        $decls.Add([pscustomobject]@{
            Kind      = $match.Groups['kind'].Value
            Name      = $match.Groups['name'].Value
            Modifiers = $match.Groups['modifiers'].Value
        })
    }

    return $decls
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

function Get-ProjectNameFromRelativePath {
    param([string]$RelativePath)

    $parts = $RelativePath -split '[\\/]'
    if ($parts.Length -gt 0) { return $parts[0] }
    return ''
}

function Get-NodeId {
    param([string]$Name)

    return ($Name -replace '[^A-Za-z0-9_]', '_')
}

$resolvedRoot = Resolve-RepositoryRoot -ExplicitRoot $Root
$outputPath = Resolve-OutputPath -ResolvedRoot $resolvedRoot -ExplicitOutput $Output -DefaultName 'type-dependency-diagram.md'
$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

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

$typeRows = New-Object System.Collections.Generic.List[object]
$fileRows = New-Object System.Collections.Generic.List[object]

foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrWhiteSpace($content)) {
        continue
    }

    $relativePath = Get-RelativePathCompat -BasePath $resolvedRoot -TargetPath $file.FullName
    $project = Get-ProjectNameFromRelativePath -RelativePath $relativePath
    $decls = Get-TypeDeclarations -Content $content
    $declaredNames = @()

    foreach ($decl in $decls) {
        $visibility = Get-Visibility -PrefixText $decl.Modifiers
        if (-not $IncludePrivate -and $visibility -eq 'private') {
            continue
        }

        $declaredNames += $decl.Name
        $typeRows.Add([pscustomobject]@{
            Name    = $decl.Name
            Project = $project
            File    = ('.\' + ($relativePath -replace '/', '\'))
        })
    }

    if ($declaredNames.Count -gt 0) {
        $fileRows.Add([pscustomobject]@{
            File          = ('.\' + ($relativePath -replace '/', '\'))
            Project       = $project
            Content       = Strip-CommentsAndStrings -Content $content
            DeclaredNames = @($declaredNames)
        })
    }
}

$knownTypes = @{}
foreach ($row in $typeRows) {
    if (-not $knownTypes.ContainsKey($row.Name)) {
        $knownTypes[$row.Name] = New-Object System.Collections.Generic.List[object]
    }
    $knownTypes[$row.Name].Add($row)
}

$edges = @{}
foreach ($file in $fileRows) {
    foreach ($source in $file.DeclaredNames) {
        foreach ($target in $knownTypes.Keys) {
            if ($target -eq $source) {
                continue
            }

            if ($file.DeclaredNames -contains $target) {
                continue
            }

            if ($file.Content -match ("\b{0}\b" -f [regex]::Escape($target))) {
                $key = "{0}|{1}" -f $source, $target
                if (-not $edges.ContainsKey($key)) {
                    $targetProject = ($knownTypes[$target] | Select-Object -First 1).Project
                    $edges[$key] = [pscustomobject]@{
                        Source        = $source
                        Target        = $target
                        SourceProject = $file.Project
                        TargetProject = $targetProject
                        SourceFile    = $file.File
                    }
                }
            }
        }
    }
}

$edgeRows = @($edges.Values | Sort-Object Source, Target)
$incomingCounts = $edgeRows | Group-Object Target | Sort-Object @{ Expression = 'Count'; Descending = $true }, Name
$outgoingCounts = $edgeRows | Group-Object Source | Sort-Object @{ Expression = 'Count'; Descending = $true }, Name
$totalTypes = @($typeRows | Select-Object -ExpandProperty Name -Unique).Count
$totalEdges = $edgeRows.Count
$density = if ($totalTypes -gt 1) { $totalEdges / ($totalTypes * ($totalTypes - 1)) } else { 0 }

$topIncoming = @($incomingCounts | Select-Object -First $TopDependencyCount)
$topOutgoing = @($outgoingCounts | Select-Object -First $TopDependencyCount)
$diagramTargets = @{}
foreach ($group in $topIncoming) {
    $diagramTargets[$group.Name] = $true
}

$diagramEdges = $edgeRows | Where-Object { $diagramTargets.ContainsKey($_.Target) } | Select-Object -First 160

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Type Dependency Diagram')
$lines.Add('')
$lines.Add(("Generated: {0}" -f $timestamp))
$lines.Add(("Root: {0}" -f $resolvedRoot))
$lines.Add('')
$lines.Add('This file is auto-generated.')
$lines.Add('It reflects direct textual references between declared repository C# types.')
$lines.Add('No compiler binding. No inference. No semantic interpretation.')
$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Summary')
$lines.Add('')
$lines.Add(('- Declared type symbols: {0}' -f $totalTypes))
$lines.Add(('- Direct type-reference edges: {0}' -f $totalEdges))
$lines.Add(('- Dependency-density reading: {0:P4}' -f $density))
$lines.Add(('- Private declarations included: {0}' -f ([bool]$IncludePrivate)))
$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Mermaid Diagram')
$lines.Add('')
$lines.Add('```mermaid')
$lines.Add('graph TD')

foreach ($edge in $diagramEdges) {
    $sourceId = Get-NodeId -Name $edge.Source
    $targetId = Get-NodeId -Name $edge.Target
    $lines.Add(('  {0}["{1}"] --> {2}["{3}"]' -f $sourceId, $edge.Source, $targetId, $edge.Target))
}

if ($diagramEdges.Count -eq 0) {
    $lines.Add('  NoEdges["No dependency edges detected"]')
}

$lines.Add('```')
$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add(('## Top Incoming Dependency Hubs'))
$lines.Add('')
$lines.Add('| Type | Incoming References |')
$lines.Add('|------|---------------------|')
foreach ($group in $topIncoming) {
    $lines.Add(('| {0} | {1} |' -f $group.Name, $group.Count))
}

$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Top Outgoing Dependency Sources')
$lines.Add('')
$lines.Add('| Type | Outgoing References |')
$lines.Add('|------|---------------------|')
foreach ($group in $topOutgoing) {
    $lines.Add(('| {0} | {1} |' -f $group.Name, $group.Count))
}

$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Notes')
$lines.Add('')
$lines.Add('- This diagram is intentionally structural evidence only.')
$lines.Add('- Dense nodes are classification candidates, not automatic architecture violations.')
$lines.Add('- Phase 3 must classify density before refactoring decisions.')
$lines.Add('')
$lines.Add('End of type-dependency-diagram.md')

$lines | Set-Content -LiteralPath $outputPath -Encoding UTF8
Write-Host ("Generated {0}" -f $outputPath)
