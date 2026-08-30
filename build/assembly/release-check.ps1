[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$IsoPath,
    [string]$OutputRoot = (Split-Path -Parent $IsoPath),
    [ValidateSet('x64','ARM64')][string]$Architecture = 'x64'
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$iso=(Resolve-Path -LiteralPath $IsoPath).Path
& (Join-Path $PSScriptRoot 'validate-iso.ps1') -IsoPath $iso -ExpectedArchitecture $Architecture
$hash=(Get-FileHash -LiteralPath $iso -Algorithm SHA256).Hash
$size=(Get-Item -LiteralPath $iso).Length
$manifest=[ordered]@{
    Product='ANON OS Windows'
    Artifact=(Split-Path $iso -Leaf)
    Architecture=$Architecture
    SizeBytes=$size
    SHA256=$hash
    StructuralValidation='PASS'
    VMValidation='REQUIRED'
    ReleaseStatus='NOT_RELEASED'
}
New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null
$manifest|ConvertTo-Json -Depth 6|Set-Content (Join-Path $OutputRoot 'RELEASE-CHECK.json') -Encoding UTF8
"$hash  $(Split-Path $iso -Leaf)"|Set-Content (Join-Path $OutputRoot 'SHA256SUMS.txt') -Encoding ASCII
Write-Host 'Release check complete: structural validation passed; VM validation remains mandatory.'
