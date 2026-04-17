[CmdletBinding()]
param (
    [string]$Root,
    [string]$Output,
    [switch]$IncludeTargetFrameworks,
    [string[]]$ExcludeDirectories = @('.git', '.vs', 'bin', 'obj', 'packages', 'node_modules', 'TestResults', '.dotnet-cli', '.dotnet-sdk', 'artifacts')
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

function Get-SafeProjectFiles {
    param(
        [string]$ResolvedRoot,
        [string[]]$ExcludedDirectories
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
            if ($child.PSIsContainer) {
                if ($ExcludedDirectories -contains $child.Name) {
                    continue
                }

                $attributes = $child.Attributes
                $isReparsePoint = (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)
                if (-not $isReparsePoint) {
                    $queue.Enqueue($child.FullName)
                }
                continue
            }

            if ($child.Extension -eq '.csproj') {
                $results.Add($child)
            }
        }
    }

    return $results
}

function Get-XmlChildCollection {
    param(
        [Parameter(Mandatory=$true)]$ParentNode,
        [Parameter(Mandatory=$true)][string]$ChildName
    )

    if (-not $ParentNode) {
        return @()
    }

    $result = @()
    foreach ($child in @($ParentNode.ChildNodes)) {
        if ($child -and $child.Name -eq $ChildName) {
            $result += $child
        }
    }

    return @($result)
}

$resolvedRoot = Resolve-RepositoryRoot -ExplicitRoot $Root
$outputPath = Resolve-OutputPath -ResolvedRoot $resolvedRoot -ExplicitOutput $Output -DefaultName 'dependency-summary.md'
$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

$projectFiles = Get-SafeProjectFiles -ResolvedRoot $resolvedRoot -ExcludedDirectories $ExcludeDirectories |
    Sort-Object FullName

$projectRefs = New-Object System.Collections.Generic.List[object]
$packageRefs = New-Object System.Collections.Generic.List[object]
$frameworkRefs = New-Object System.Collections.Generic.List[object]

foreach ($projectFile in $projectFiles) {
    [xml]$xml = Get-Content -LiteralPath $projectFile.FullName -Raw
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectFile.Name)

    $propertyGroups = Get-XmlChildCollection -ParentNode $xml.Project -ChildName 'PropertyGroup'
    $targetFramework = ''
    foreach ($pg in $propertyGroups) {
        foreach ($node in @($pg.ChildNodes)) {
            if ($node.Name -eq 'TargetFramework' -and -not [string]::IsNullOrWhiteSpace($node.InnerText)) {
                $targetFramework = [string]$node.InnerText
                break
            }
            if ($node.Name -eq 'TargetFrameworks' -and -not [string]::IsNullOrWhiteSpace($node.InnerText)) {
                $targetFramework = [string]$node.InnerText
                break
            }
        }
        if ($targetFramework) { break }
    }

    if ($targetFramework) {
        $frameworkRefs.Add([pscustomobject]@{
            Project   = $projectName
            Framework = $targetFramework
        })
    }

    $itemGroups = Get-XmlChildCollection -ParentNode $xml.Project -ChildName 'ItemGroup'
    foreach ($itemGroup in $itemGroups) {
        $projRefs = Get-XmlChildCollection -ParentNode $itemGroup -ChildName 'ProjectReference'
        foreach ($projRef in $projRefs) {
            $include = [string]$projRef.Include
            if ([string]::IsNullOrWhiteSpace($include)) { continue }

            $dependencyName = [System.IO.Path]::GetFileNameWithoutExtension($include)

            $projectRefs.Add([pscustomobject]@{
                SourceProject = $projectName
                DependsOn     = $dependencyName
            })
        }

        $pkgRefs = Get-XmlChildCollection -ParentNode $itemGroup -ChildName 'PackageReference'
        foreach ($pkgRef in $pkgRefs) {
            $packageName = [string]$pkgRef.Include
            if ([string]::IsNullOrWhiteSpace($packageName)) { continue }

            $version = '(unspecified)'
            if ($pkgRef.Version) {
                $version = [string]$pkgRef.Version
            }
            else {
                foreach ($child in @($pkgRef.ChildNodes)) {
                    if ($child.Name -eq 'Version' -and -not [string]::IsNullOrWhiteSpace($child.InnerText)) {
                        $version = [string]$child.InnerText
                        break
                    }
                }
            }

            $packageRefs.Add([pscustomobject]@{
                Project = $projectName
                Package = $packageName
                Version = $version
            })
        }
    }
}

$projectRefs = $projectRefs | Sort-Object SourceProject, DependsOn -Unique
$packageRefs = $packageRefs | Sort-Object Project, Package, Version -Unique
$frameworkRefs = $frameworkRefs | Sort-Object Project, Framework -Unique

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('# Dependency Summary')
$lines.Add('')
$lines.Add(("Generated: {0}" -f $timestamp))
$lines.Add(("Root: {0}" -f $resolvedRoot))
$lines.Add('')
$lines.Add('This file is auto-generated.')
$lines.Add('It reflects **declared dependencies only**.')
$lines.Add('No inference. No semantic interpretation.')
$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Project-to-Project Dependencies')
$lines.Add('')
$lines.Add('| Source Project | Depends On |')
$lines.Add('|---------------|------------|')

foreach ($row in $projectRefs) {
    $lines.Add(("| {0} | {1} |" -f $row.SourceProject, $row.DependsOn))
}

if ($IncludeTargetFrameworks) {
    $lines.Add('')
    $lines.Add('------------------------------------------------------')
    $lines.Add('')
    $lines.Add('## Target Frameworks')
    $lines.Add('')
    foreach ($row in $frameworkRefs) {
        $lines.Add(('- {0}: {1}' -f $row.Project, $row.Framework))
    }
}

$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## External Package Dependencies')
$lines.Add('')

$packageGroups = $packageRefs | Group-Object Project | Sort-Object Name
foreach ($group in $packageGroups) {
    $lines.Add(("### {0}" -f $group.Name))
    $lines.Add('')

    foreach ($pkg in ($group.Group | Sort-Object Package, Version)) {
        $lines.Add(('- {0} ({1})' -f $pkg.Package, $pkg.Version))
    }

    $lines.Add('')
}

$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('## Notes')
$lines.Add('')
$lines.Add('- Dependencies are derived from `<ProjectReference>` and `<PackageReference>` only.')
$lines.Add('- Versions may be `(unspecified)` when provided through central package management, imported props/targets, or other MSBuild indirection rather than directly on the project node.')
$lines.Add('- Absence of a dependency here is authoritative only within the declared project-file scope above.')
$lines.Add('- This document is **structural**, not architectural.')
$lines.Add('- Boundary concerns must be evaluated against:')
$lines.Add('  - SYSTEM_MAP.md')
$lines.Add('  - Project Bible.md')
$lines.Add('')
$lines.Add('------------------------------------------------------')
$lines.Add('')
$lines.Add('End of dependency-summary.md')

$lines | Set-Content -LiteralPath $outputPath -Encoding UTF8
Write-Host ("Generated {0}" -f $outputPath)
