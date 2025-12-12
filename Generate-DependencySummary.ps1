param (
    [string]$RootPath = ".",
    [string]$OutputFile = "dependency-summary.md"
)

Write-Host "Generating dependency summary (refined + fan-in)..."

$csFiles = Get-ChildItem $RootPath -Recurse -Include *.cs -File

$lines = @()
$lines += "# Dependency Summary"
$lines += ""
$lines += "_Auto-generated. Heuristic, structural only. Do not edit by hand._"
$lines += ""

# --------------------------------------------------
# Helpers
# --------------------------------------------------

function Is-PlausibleNamespace {
    param ($value)

    # Refinement 1:
    # - must contain at least one dot
    # - must start with an uppercase letter
    return ($value -match '^[A-Z]' -and $value.Contains('.'))
}

# --------------------------------------------------
# Collect dependencies
# --------------------------------------------------

$fileDependencies = @{}
$namespaceUsageCount = @{}

foreach ($file in $csFiles) {

    $relative = $file.FullName.Replace($RootPath, '').TrimStart('\')
    $content  = Get-Content $file.FullName

    $rawUsings = $content |
        Select-String '^\s*using\s+' |
        ForEach-Object {
            ($_ -replace '^\s*using\s+', '').TrimEnd(';').Trim()
        }

    # Refinement 1: filter implausible namespaces
    $filteredUsings = $rawUsings |
        Where-Object { Is-PlausibleNamespace $_ } |
        Sort-Object -Unique

    # Refinement 2: collapse System.*
    $systemUsings = $filteredUsings | Where-Object { $_ -like 'System.*' }
    $nonSystemUsings = $filteredUsings | Where-Object { $_ -notlike 'System.*' }

    $finalUsings = @()

    if ($systemUsings.Count -gt 0) {
        $finalUsings += "System (common)"
    }

    $finalUsings += $nonSystemUsings

    # Track fan-in (Refinement 3)
    foreach ($ns in $nonSystemUsings) {
        if (-not $namespaceUsageCount.ContainsKey($ns)) {
            $namespaceUsageCount[$ns] = 0
        }
        $namespaceUsageCount[$ns]++
    }

    $fileDependencies[$relative] = @{
        Usings = $finalUsings
        Tags   = @()
    }

    if ($file.Name -like '*ViewModel.cs') { $fileDependencies[$relative].Tags += "ViewModel" }
    if ($file.Name -like '*Service.cs')   { $fileDependencies[$relative].Tags += "Service" }
    if ($file.Name -like '*Strategy.cs')  { $fileDependencies[$relative].Tags += "Strategy" }
    if ($file.Name -like '*Engine.cs')    { $fileDependencies[$relative].Tags += "Engine" }
}

# --------------------------------------------------
# Per-file breakdown
# --------------------------------------------------

$lines += "## Per-File Dependencies"

foreach ($file in $fileDependencies.Keys | Sort-Object) {

    $lines += "### $file"

    if ($fileDependencies[$file].Tags.Count -gt 0) {
        $lines += "**Tags:** " + ($fileDependencies[$file].Tags -join ", ")
    }

    if ($fileDependencies[$file].Usings.Count -gt 0) {
        $lines += "**Uses Namespaces:**"
        foreach ($u in $fileDependencies[$file].Usings) {
            $lines += "- $u"
        }
    }
    else {
        $lines += "_No namespace dependencies detected._"
    }

    $lines += ""
}

# --------------------------------------------------
# Aggregated Views
# --------------------------------------------------

$lines += "## Aggregated Dependency Views"

function Group-ByTag {
    param ($tag)

    $group = $fileDependencies.GetEnumerator() |
        Where-Object { $_.Value.Tags -contains $tag }

    if ($group) {
        $lines += "### $tag Files"
        foreach ($item in $group) {
            $lines += "- $($item.Key)"
        }
        $lines += ""
    }
}

Group-ByTag "ViewModel"
Group-ByTag "Service"
Group-ByTag "Strategy"
Group-ByTag "Engine"

# --------------------------------------------------
# High Fan-In Summary (Refinement 3)
# --------------------------------------------------

$lines += "## High Fan-In Dependencies"
$lines += "_Namespaces referenced by multiple files. Indicates potential coupling or fragility._"
$lines += ""

$highFanIn = $namespaceUsageCount.GetEnumerator() |
    Where-Object { $_.Value -gt 1 } |
    Sort-Object Value -Descending

if ($highFanIn) {
    foreach ($entry in $highFanIn) {
        $lines += "- $($entry.Key) (used by $($entry.Value) files)"
    }
}
else {
    $lines += "_No high fan-in dependencies detected._"
}

$lines += ""

# --------------------------------------------------
# Write Output
# --------------------------------------------------

$lines | Set-Content $OutputFile -Encoding UTF8

Write-Host "dependency-summary.md generated successfully (refined + fan-in)."
