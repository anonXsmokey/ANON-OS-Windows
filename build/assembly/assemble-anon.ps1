[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$WindowsIso,
    [Parameter(Mandatory=$true)][string]$ShellPublish,
    [string]$OutputRoot = (Join-Path $PSScriptRoot '..\out'),
    [ValidateSet('x64','ARM64')][string]$Architecture = 'x64'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# This stage assembles a working WinPE-based engineering image only after the
# supplied Windows source and shell payload have been explicitly validated.
# It does not redistribute Microsoft installation media.

foreach ($tool in @('dism.exe','oscdimg.exe')) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "Missing required tool: $tool" }
}
if (-not (Test-Path -LiteralPath $WindowsIso)) { throw "Windows ISO not found: $WindowsIso" }
if (-not (Test-Path -LiteralPath $ShellPublish)) { throw "Shell publish directory not found: $ShellPublish" }

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$work = Join-Path $OutputRoot "assembly-$stamp"
New-Item -ItemType Directory -Force -Path $work | Out-Null

$isoMount = Mount-DiskImage -ImagePath (Resolve-Path $WindowsIso).Path -PassThru
try {
    $volume = $isoMount | Get-Volume
    $sourceDrive = ($volume | Get-Partition | Get-Volume).DriveLetter
    if (-not $sourceDrive) { throw 'Unable to resolve mounted Windows ISO drive.' }
    $sourceRoot = "${sourceDrive}:\"
    Copy-Item "$sourceRoot*" $work -Recurse -Force
}
finally {
    Dismount-DiskImage -ImagePath (Resolve-Path $WindowsIso).Path
}

$payload = Join-Path $work 'ANON\Shell\M0'
New-Item -ItemType Directory -Force -Path $payload | Out-Null
Copy-Item (Join-Path $ShellPublish '*') $payload -Recurse -Force

$manifest = [ordered]@{
    schemaVersion = 1
    product = 'ANON OS Windows'
    architecture = $Architecture
    buildUtc = [DateTime]::UtcNow.ToString('o')
    sourceIsoSha256 = (Get-FileHash -LiteralPath $WindowsIso -Algorithm SHA256).Hash
    shellPayload = 'ANON/Shell/M0'
    status = 'ASSEMBLY-STAGING-ONLY'
    note = 'Boot/install integration is intentionally not asserted by this staging artifact.'
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $work 'ANON-BUILD-MANIFEST.json') -Encoding UTF8

Write-Host "Assembly staging tree prepared: $work"
Write-Host 'No ISO was emitted: installer integration and VM validation are mandatory release gates.'
