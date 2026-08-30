[CmdletBinding()]
param(
 [ValidateSet('install.wim','install.esd')][string]$ImageFile='install.wim',
 [ValidateRange(1,999)][int]$ImageIndex=1,
 [ValidateSet('BALANCED','GAMING','PERFORMANCE','CUSTOM')][string]$Profile='GAMING',
 [bool]$Ai=$false,
 [ValidateSet('minimal','standard','custom')][string]$Telemetry='minimal',
 [bool]$Recovery=$true,
 [bool]$RestorePoint=$true,
 [string]$OutputPath=(Join-Path $PSScriptRoot 'ANON-INSTALL-PLAN.json')
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$plan=[ordered]@{
 schemaVersion=1
 source=[ordered]@{imageFile=$ImageFile;imageIndex=$ImageIndex}
 target=[ordered]@{firmware='UEFI';architecture='x64'}
 profile=$Profile
 features=[ordered]@{ai=$Ai;telemetry=$Telemetry;recovery=$Recovery;restorePoint=$RestorePoint}
}
$plan|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Host "ANON installation plan: $OutputPath"
