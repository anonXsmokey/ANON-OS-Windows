[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$IsoPath,[ValidateSet('x64','ARM64')][string]$Architecture='x64')
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$iso=(Resolve-Path -LiteralPath $IsoPath).Path
& (Join-Path $PSScriptRoot 'validate-iso.ps1') -IsoPath $iso -ExpectedArchitecture $Architecture
$root=Split-Path $iso -Parent
$check=Join-Path $root 'RELEASE-CHECK.json'
if(Test-Path $check){$data=Get-Content $check -Raw|ConvertFrom-Json;if($data.ReleaseStatus -eq 'RELEASED'){throw 'Release-check already claims RELEASED before VM validation.'}}
$hash=(Get-FileHash $iso -Algorithm SHA256).Hash
$manifest=[ordered]@{Product='ANON OS Windows';Architecture=$Architecture;ISO=(Split-Path $iso -Leaf);SHA256=$hash;Structure='PASS';VM='PENDING';Install='PENDING';FirstLogon='PENDING';ReleaseStatus='BLOCKED'}
$manifest|ConvertTo-Json|Set-Content (Join-Path $root 'VM-VALIDATION-REQUIRED.json') -Encoding UTF8
Write-Host 'Build validation recorded. VM installation and first-logon tests are still required.'
