param (
    [string]$RootPath = ".",
    [string]$OutputFile = "codebase-index.md"
)

Write-Host "Generating codebase index..."

$csFiles   = Get-ChildItem $RootPath -Recurse -Include *.cs -File
$xamlFiles = Get-ChildItem $RootPath -Recurse -Include *.xaml -File
$projects  = Get-ChildItem $RootPath -Recurse -Include *.csproj -File

$lines = @()
$lines += "# Codebase Index"
$lines += ""
$lines += "_Auto-generated. Do not edit by hand._"
$lines += ""

# ----------------------------
# Projects
# ----------------------------
$lines += "## Projects"
foreach ($proj in $projects) {
    $lines += "- $($proj.FullName.Replace($RootPath, '').TrimStart('\'))"
}
$lines += ""

# ----------------------------
# C# Files
# ----------------------------
$lines += "## C# Source Files"

foreach ($file in $csFiles) {

    $relative = $file.FullName.Replace($RootPath, '').TrimStart('\')
    $content  = Get-Content $file.FullName

    $namespace = $content |
        Select-String '^\s*namespace\s+' |
        Select-Object -First 1 |
        ForEach-Object { ($_ -replace '^\s*namespace\s+', '').Trim() }

    $classMatches = $content |
        Select-String '^\s*public\s+class\s+'

    $interfaceMatches = $content |
        Select-String '^\s*public\s+interface\s+'

    $enumMatches = $content |
        Select-String '^\s*public\s+enum\s+'

    $entryPoints = $content |
        Select-String 'static\s+void\s+Main\s*\('

    # ----------------------------
    # Heuristic: DataCarrier detection
    # ----------------------------
    $propertyMatches = $content |
        Select-String 'public\s+.+\{\s*get;\s*set;\s*\}'

    $methodMatches = $content |
        Select-String 'public\s+.+\(' |
        Where-Object { $_ -notmatch 'class|interface|enum' }

    $isDataCarrier =
        ($classMatches.Count -gt 0) -and
        ($propertyMatches.Count -gt 0) -and
        ($methodMatches.Count -eq 0)

    $tags = @()

    if ($file.Name -like '*ViewModel.cs' -or
        ($content | Select-String 'INotifyPropertyChanged')) {
        $tags += "ViewModel"
    }

    if ($file.Name -like '*Strategy.cs') { $tags += "Strategy" }
    if ($file.Name -like '*Engine.cs')   { $tags += "Engine" }
    if ($file.Name -like '*Service.cs')  { $tags += "Service" }

    if ($isDataCarrier) {
        $tags += "DataCarrier (heuristic)"
    }

    $lines += "### $relative"

    if ($namespace) {
        $lines += "**Namespace:** $namespace"
    }

    if ($tags.Count -gt 0) {
        $lines += "**Tags:** " + ($tags -join ", ")
    }

    if ($classMatches) {
        $lines += "**Classes:**"
        foreach ($match in $classMatches) {
            $lines += "- $($match.Line.Trim())"
        }
    }

    if ($interfaceMatches) {
        $lines += "**Interfaces:**"
        foreach ($match in $interfaceMatches) {
            $name = ($match.Line -replace '.*public\s+interface\s+', '').Split(' ')[0]
            $lines += "- $name"
        }
    }

    if ($enumMatches) {
        $lines += "**Enums:**"
        foreach ($match in $enumMatches) {
            $name = ($match.Line -replace '.*public\s+enum\s+', '').Split(' ')[0]
            $lines += "- $name"
        }
    }

    if ($entryPoints) {
        $lines += "**Entry Points:**"
        $lines += "- Main()"
    }

    if (-not ($classMatches -or $interfaceMatches -or $enumMatches -or $entryPoints)) {
        $lines += "_No public symbols detected._"
    }

    $lines += ""
}

# ----------------------------
# XAML Files
# ----------------------------
$lines += "## XAML Files"
foreach ($xaml in $xamlFiles) {
    $relative = $xaml.FullName.Replace($RootPath, '').TrimStart('\')
    $lines += "- $relative"
}

# ----------------------------
# Write Output
# ----------------------------
$lines | Set-Content $OutputFile -Encoding UTF8

Write-Host "codebase-index.md generated successfully."
