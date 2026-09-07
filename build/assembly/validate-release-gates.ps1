[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$ManifestPath,
 [switch]$RequireVm
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $ManifestPath)){throw "Release manifest not found: $ManifestPath"}
$m=Get-Content -LiteralPath $ManifestPath -Raw|ConvertFrom-Json
$required=@('ShellBuild','IsoStructure','VmBoot','WindowsInstall','FirstLogon','ShellSmoke','Gaming','Recovery','Benchmark')
$missing=@()
foreach($gate in $required){
    $property=$m.Gates.PSObject.Properties[$gate]
    if($null -eq $property -or [string]::IsNullOrWhiteSpace([string]$property.Value)){$missing+=$gate}
}
if($missing.Count){throw "Manifest is incomplete. Missing gates: $($missing -join ', ')"}
if($m.ReleaseDecision -ne 'BLOCKED_UNTIL_ALL_GATES_PASS' -and $m.ReleaseDecision -ne 'RELEASE_APPROVED'){
    throw "Unexpected release decision: $($m.ReleaseDecision)"
}
if($RequireVm){
    $unverified=@($required|Where-Object{[string]$m.Gates.$_ -ne 'PASS'})
    if($unverified.Count){throw "VM release gates are not all PASS: $($unverified -join ', ')"}
}
Write-Host 'Release gate schema: PASS'
Write-Host "Decision: $($m.ReleaseDecision)"
