# Generate-CodebaseIndex.ps1
# Emits a declarative index of declared symbols only
# No inference. No semantic interpretation.

param (
    [string]$Root = ".",
    [string]$Output = "codebase-index.md"
)

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$symbolPattern = '^\s*(public|internal)?\s*(sealed\s+|abstract\s+|static\s+)?(class|record|interface|enum|struct)\s+([A-Za-z0-9_]+)'
$namespacePattern = '^\s*namespace\s+([A-Za-z0-9_.]+)'

$rows = @()

Get-ChildItem -Path $Root -Recurse -Filter *.cs |
    Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' } |
    ForEach-Object {

        $file = $_.FullName.Replace((Resolve-Path $Root).Path, ".")
        $namespace = ""

        Get-Content $_.FullName | ForEach-Object {

            if (-not $namespace -and $_ -match $namespacePattern) {
                $namespace = $matches[1]
            }

            if ($_ -match $symbolPattern) {
                $visibility = if ($matches[1]) { $matches[1] } else { "internal" }
                $kind = $matches[3].ToLower()
                $name = $matches[4]

                $rows += [PSCustomObject]@{
                    Kind       = $kind
                    Name       = $name
                    Visibility = $visibility
                    Namespace  = $namespace
                    File       = $file
                }
            }
        }
    }

# De-duplicate by Kind + Name + Namespace + File
$rows = $rows |
    Sort-Object Kind, Name, Namespace, File |
    Get-Unique -AsString

# Emit markdown
@"
# Codebase Index

Generated: $timestamp  
Root: $(Resolve-Path $Root)

This file is auto-generated.

**Scope**
- Declared symbols only
- No inference
- No semantic interpretation

------------------------------------------------------

| Kind | Name | Visibility | Namespace | File |
|------|------|------------|-----------|------|
$( $rows | ForEach-Object {
"| $($_.Kind) | $($_.Name) | $($_.Visibility) | $($_.Namespace) | $($_.File) |"
} | Out-String )

------------------------------------------------------

End of codebase-index.md
"@ | Set-Content -Encoding UTF8 $Output

Write-Host "Generated $Output"
