[CmdletBinding()]
param(
    [string]$WindowsIso = 'E:\ANON-OS\ANON-OS-Windows\Win11_iso\Win11_25H2_EnglishInternational_x64_v2.iso',
    [ValidateRange(1,999)][int]$ImageIndex = 6,
    [string]$OutputRoot = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $scriptRoot '..\out'
}

if (-not (Test-Path -LiteralPath $WindowsIso)) {
    throw "Windows 11 source ISO not found: $WindowsIso"
}

$root = (Resolve-Path (Join-Path $scriptRoot '..\..')).Path
$out = (Resolve-Path (New-Item -ItemType Directory -Force -Path $OutputRoot)).Path

$required = @(
    'build\assembly\build-local.ps1',
    'build\assembly\validate-iso.ps1',
    'build\assembly\release-manifest.ps1',
    'build\assembly\validate-release-gates.ps1',
    'ux\shell\M0\ANON.Shell.M0.csproj',
    'ux\shell\Bootstrap\ANON.Shell.Bootstrap.csproj',
    'ux\shell\PerformancePet\ANON.PerformancePet.csproj'
)
foreach ($path in $required) {
    if (-not (Test-Path (Join-Path $root $path))) { throw "Required project/build file missing: $path" }
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) { throw 'dotnet SDK is required.' }

$disM = Get-Command dism.exe -ErrorAction SilentlyContinue
if (-not $disM) { throw 'DISM is required; run this on Windows.' }

Write-Host '=== ANON OS FINAL BUILD ===' -ForegroundColor Cyan
Write-Host "Source ISO : $WindowsIso"
Write-Host "Image index: $ImageIndex"
Write-Host "Output     : $out"

& (Join-Path $root 'build\assembly\build-local.ps1') -WindowsIso (Resolve-Path $WindowsIso).Path -ImageIndex $ImageIndex -OutputRoot $out
if ($LASTEXITCODE) { throw 'build-local.ps1 failed.' }

$iso = Get-ChildItem $out -Filter 'ANON-OS-Windows-*.iso' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $iso) { throw 'No ANON OS ISO was produced.' }

$sha = (Get-FileHash -LiteralPath $iso.FullName -Algorithm SHA256).Hash
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$packageRoot = Join-Path $out "ANON-OS-Windows-$stamp-x64-FINAL-PACKAGE"
New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null

Copy-Item -LiteralPath $iso.FullName -Destination $packageRoot
foreach ($name in @('SHA256SUMS','BUILD-MANIFEST.json','RELEASE-MANIFEST.json')) {
    $source = Join-Path $out $name
    if (Test-Path $source) { Copy-Item -LiteralPath $source -Destination $packageRoot }
}

$readme = @"
ANON OS WINDOWS — FINAL BUILD PACKAGE

ISO: $($iso.Name)
SHA256: $sha
Architecture: x64
Windows image index: $ImageIndex

Static validation: PASS (build-local.ps1 completed)
Runtime VM validation: REQUIRED BEFORE RELEASE

The ISO must be booted in a clean VM and all runtime gates must pass before the build is called a release.
"@
Set-Content -LiteralPath (Join-Path $packageRoot 'RELEASE-STATUS.txt') -Value $readme -Encoding UTF8

$zip = Join-Path $out "ANON-OS-Windows-$stamp-x64-FINAL-PACKAGE.zip"
if (Test-Path $zip) { Remove-Item -Force $zip }

# Compress-Archive has a practical size limit for large ZIP entries and failed
# on the ~8.6 GB ISO. Prefer 7-Zip when installed, otherwise stream the files
# through .NET ZipArchive, which supports ZIP64 without loading the ISO into memory.
$sevenZip = @(
    (Get-Command 7z.exe -ErrorAction SilentlyContinue),
    (Get-Command 7zz.exe -ErrorAction SilentlyContinue)
) | Where-Object { $_ } | Select-Object -First 1

if ($sevenZip) {
    Write-Host "Packaging with $($sevenZip.Source) (ZIP64, store mode)..." -ForegroundColor Cyan
    & $sevenZip.Source a -tzip -mx=0 -mm=Copy $zip (Join-Path $packageRoot '*') | Out-Host
    if ($LASTEXITCODE) { throw "7-Zip package creation failed with exit code $LASTEXITCODE." }
}
else {
    Write-Host '7-Zip not found; packaging with .NET ZipArchive (ZIP64, store mode)...' -ForegroundColor Cyan
    Add-Type -AssemblyName System.IO.Compression

    $zipStream = [System.IO.File]::Open(
        $zip,
        [System.IO.FileMode]::Create,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)
    try {
        $archive = [System.IO.Compression.ZipArchive]::new(
            $zipStream,
            [System.IO.Compression.ZipArchiveMode]::Create,
            $false)
        try {
            foreach ($file in Get-ChildItem -LiteralPath $packageRoot -File) {
                Write-Host "Adding $($file.Name) ..."
                $entry = $archive.CreateEntry(
                    $file.Name,
                    [System.IO.Compression.CompressionLevel]::NoCompression)
                $entryStream = $entry.Open()
                try {
                    $sourceStream = [System.IO.File]::OpenRead($file.FullName)
                    try {
                        $sourceStream.CopyTo($entryStream, 1048576)
                    }
                    finally {
                        $sourceStream.Dispose()
                    }
                }
                finally {
                    $entryStream.Dispose()
                }
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $zipStream.Dispose()
    }
}

if (-not (Test-Path -LiteralPath $zip)) {
    throw 'Final package ZIP was not created.'
}

$zipSize = (Get-Item -LiteralPath $zip).Length
Write-Host "Package ZIP size: $zipSize bytes"

Write-Host ''
Write-Host 'FINAL BUILD OUTPUTS' -ForegroundColor Green
Write-Host "ISO : $($iso.FullName)"
Write-Host "ZIP : $zip"
Write-Host "SHA : $sha"
Write-Host ''
Write-Host 'Release decision: BLOCKED until clean-VM runtime gates pass.' -ForegroundColor Yellow
