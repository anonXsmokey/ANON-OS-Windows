[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ManifestPath,[Parameter(Mandatory=$true)][string]$VmResultPath)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
foreach($p in @($ManifestPath,$VmResultPath)){if(-not(Test-Path -LiteralPath $p)){throw "Required validation file not found: $p"}}
$m=Get-Content $ManifestPath -Raw|ConvertFrom-Json;$v=Get-Content $VmResultPath -Raw|ConvertFrom-Json
$expected=@('PASS')
$checks=[ordered]@{
 ShellBuild=[string]$m.Gates.ShellBuild
 IsoStructure=[string]$m.Gates.IsoStructure
 VmBoot=[string]$v.VmBoot
 WindowsInstall=[string]$v.WindowsInstall
 FirstLogon=[string]$v.FirstLogon
 ShellSmoke=[string]$v.ShellSmoke
}
$failed=$checks.GetEnumerator()|Where-Object{$_.Value -notin $expected}|ForEach-Object{"$($_.Key)=$($_.Value)"}
if($failed){throw "Release blocked. Failed/unverified gates: $($failed -join ', ')"}
$m.Gates.ShellBuild='PASS';$m.Gates.IsoStructure='PASS';$m.Gates.VmBoot='PASS';$m.Gates.WindowsInstall='PASS';$m.Gates.FirstLogon='PASS';$m.Gates.ShellSmoke='PASS'
$m.Status='release';$m.ReleaseDecision='RELEASE_APPROVED';$m.VmValidation=$v
$m|ConvertTo-Json -Depth 10|Set-Content $ManifestPath -Encoding UTF8
Write-Host 'ALL RELEASE GATES: PASS';Write-Host 'RELEASE DECISION: RELEASE_APPROVED';Write-Host "Manifest: $ManifestPath"
