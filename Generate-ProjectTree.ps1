# Generate-ProjectTree.ps1
# Deterministic, noise-reduced structural snapshot of the repository

param (
    [string]$Root = ".",
    [string]$Output = "project-tree.txt"
)

$excludeDirs = @(
    ".git",
    ".vs",
    "bin",
    "obj",
    "node_modules"
)

$excludeFiles = @(
    "*.user",
    "*.suo",
    "*.lock.json"
)

function Should-ExcludePath($path) {
    foreach ($dir in $excludeDirs) {
        if ($path -match "[\\/]" + [regex]::Escape($dir) + "([\\/]|$)") {
            return $true
        }
    }
    foreach ($pattern in $excludeFiles) {
        if ($path -like $pattern) {
            return $true
        }
    }
    return $false
}

$rootPath = Resolve-Path $Root

$entries = Get-ChildItem -Path $rootPath -Recurse -Force |
    Where-Object {
        -not (Should-ExcludePath $_.FullName)
    } |
    Sort-Object FullName |
    ForEach-Object {
        $_.FullName.Replace($rootPath.Path, "").TrimStart('\','/')
    }

$header = @(
    "Project Tree Snapshot"
    "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    "Root: $rootPath"
    ""
)

$header + $entries | Set-Content -Encoding UTF8 $Output

Write-Host "Generated $Output"
