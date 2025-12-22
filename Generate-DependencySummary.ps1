# Generate-DependencySummary.ps1
# Emits a strict, declarative dependency summary
# Declared dependencies only — no inference

param (
    [string]$Root = ".",
    [string]$Output = "dependency-summary.md"
)

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$projects = Get-ChildItem -Path $Root -Recurse -Filter *.csproj

$projectDeps = @()
$packageDeps = @{}

foreach ($proj in $projects) {
    [xml]$xml = Get-Content $proj.FullName

    $projName = $xml.Project.PropertyGroup.AssemblyName
    if (-not $projName) {
        $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj.Name)
    }

    # Project references
    $projRefs = $xml.Project.ItemGroup.ProjectReference
    foreach ($ref in $projRefs) {
        $refPath = $ref.Include
        if ($refPath) {
            $target = [System.IO.Path]::GetFileNameWithoutExtension($refPath)
            $projectDeps += [PSCustomObject]@{
                Source = $projName
                Target = $target
            }
        }
    }

    # Package references
    $pkgRefs = $xml.Project.ItemGroup.PackageReference
    foreach ($pkg in $pkgRefs) {
        if (-not $packageDeps.ContainsKey($projName)) {
            $packageDeps[$projName] = @()
        }

        $packageDeps[$projName] += [PSCustomObject]@{
            Name    = $pkg.Include
            Version = $pkg.Version
        }
    }
}

# ---- Emit Markdown ----

$md = @()
$md += "# Dependency Summary"
$md += ""
$md += "Generated: $timestamp"
$md += "Root: $Root"
$md += ""
$md += "This file is auto-generated."
$md += "It reflects **declared dependencies only**."
$md += "No inference. No semantic interpretation."
$md += ""
$md += "------------------------------------------------------"
$md += ""
$md += "## Project-to-Project Dependencies"
$md += ""
$md += "| Source Project | Depends On |"
$md += "|---------------|------------|"

if ($projectDeps.Count -eq 0) {
    $md += "| _(none)_ | _(none)_ |"
}
else {
    foreach ($d in $projectDeps | Sort-Object Source, Target) {
        $md += "| $($d.Source) | $($d.Target) |"
    }
}

$md += ""
$md += "------------------------------------------------------"
$md += ""
$md += "## External Package Dependencies"
$md += ""

foreach ($proj in $packageDeps.Keys | Sort-Object) {
    $md += "### $proj"
    $md += ""

    $pkgs = $packageDeps[$proj] | Sort-Object Name
    if ($pkgs.Count -eq 0) {
        $md += "- _(none)_"
    }
    else {
        foreach ($p in $pkgs) {
            $ver = if ($p.Version) { $p.Version } else { "(unspecified)" }
            $md += "- $($p.Name) ($ver)"
        }
    }

    $md += ""
}

$md += "------------------------------------------------------"
$md += ""
$md += "## Notes"
$md += ""
$md += "- Dependencies are derived from `<ProjectReference>` and `<PackageReference>` only."
$md += "- Absence of a dependency here is authoritative."
$md += "- This document is **structural**, not architectural."
$md += "- Boundary concerns must be evaluated against:"
$md += "  - SYSTEM_MAP.md"
$md += "  - Project Bible.md"
$md += ""
$md += "------------------------------------------------------"
$md += ""
$md += "End of dependency-summary.md"

$md | Set-Content -Encoding UTF8 $Output

Write-Host "Generated $Output"
