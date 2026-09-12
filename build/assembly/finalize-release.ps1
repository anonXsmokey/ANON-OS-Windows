[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ManifestPath,[Parameter(Mandatory=$true)][string]$VmResultPath)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
foreach($p in @($ManifestPath,$VmResultPath)){if(-not(Test-Path -LiteralPath $p)){throw "Required validation file not found: $p"}}
$m=Get-Content $ManifestPath -Raw|ConvertFrom-Json
$v=Get-Content $VmResultPath -Raw|ConvertFrom-Json
$expected='PASS'
$runtimeGates=@('VmBoot','WindowsInstall','FirstLogon','ShellSmoke','Gaming','Recovery','Benchmark')
$missing=@()
foreach($gate in $runtimeGates){
    $property=$v.PSObject.Properties[$gate]
    if($null -eq $property){$missing+=$gate}
}
if($missing.Count){throw "VM validation result is incomplete. Missing gates: $($missing -join ', ')"}
$checks=[ordered]@{
 ShellBuild=[string]$m.Gates.ShellBuild
 IsoStructure=[string]$m.Gates.IsoStructure
 VmBoot=[string]$v.VmBoot
 WindowsInstall=[string]$v.WindowsInstall
 FirstLogon=[string]$v.FirstLogon
 ShellSmoke=[string]$v.ShellSmoke
 Gaming=[string]$v.Gaming
 Recovery=[string]$v.Recovery
 Benchmark=[string]$v.Benchmark
}
$failed=$checks.GetEnumerator()|Where-Object{$_.Value -ne $expected}|ForEach-Object{"$($_.Key)=$($_.Value)"}
if($failed){throw "Release blocked. Failed/unverified gates: $($failed -join ', ')"}
foreach($gate in $checks.Keys){$m.Gates.$gate='PASS'}
$m.Status='release'
$m.ReleaseDecision='RELEASE_APPROVED'
$m.VmValidation=$v
$m|ConvertTo-Json -Depth 10|Set-Content $ManifestPath -Encoding UTF8
Write-Host 'ALL RELEASE GATES: PASS'
Write-Host 'RELEASE DECISION: RELEASE_APPROVED'
Write-Host "Manifest: $ManifestPath"
