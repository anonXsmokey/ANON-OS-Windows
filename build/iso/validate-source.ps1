[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$WindowsIso,
    [string]$OutputManifest = (Join-Path $PSScriptRoot '..\out\SOURCE-MANIFEST.json')
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not (Test-Path -LiteralPath $WindowsIso -PathType Leaf)) { throw "Windows ISO not found: $WindowsIso" }
$iso = (Resolve-Path -LiteralPath $WindowsIso).Path
$hash = (Get-FileHash -LiteralPath $iso -Algorithm SHA256).Hash
$size = (Get-Item -LiteralPath $iso).Length

$sevenZip = Get-Command 7z.exe -ErrorAction SilentlyContinue
$dism = Get-Command dism.exe -ErrorAction SilentlyContinue
if (-not $dism) { throw 'DISM is required.' }

$outDir = Split-Path -Parent $OutputManifest
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
[ordered]@{
    schemaVersion = 1
    source = $iso
    sha256 = $hash
    sizeBytes = $size
    validatedUtc = [DateTime]::UtcNow.ToString('o')
    tools = [ordered]@{
        dism = $dism.Source
        sevenZip = if ($sevenZip) { $sevenZip.Source } else { $null }
    }
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $OutputManifest -Encoding UTF8

Write-Host "Source validated: $iso"
Write-Host "SHA256: $hash"
