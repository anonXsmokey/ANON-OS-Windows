[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$ManifestPath,
 [switch]$RequireVm
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $ManifestPath)){throw "Release manifest not found: $ManifestPath"}
$m=Get-Content -LiteralPath $ManifestPath -Raw|ConvertFrom-Json
$required=@('ShellBuild','IsoStructure','VmBoot','WindowsInstall','FirstLogon','ShellSmoke')
$missing=@()
foreach($gate in $required){$value=$m.Gates.$gate;if($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)){$missing+=$gate}}
if($missing.Count){throw "Manifest is incomplete. Missing gates: $($missing -join ', ')"}
if($m.ReleaseDecision -ne 'BLOCKED_UNTIL_ALL_GATES_PASS'){throw "Unexpected release decision: $($m.ReleaseDecision)"}
if($RequireVm -and ($m.Gates.VmBoot -eq 'required' -or $m.Gates.WindowsInstall -eq 'required')){throw 'VM installation gates are not marked PASS; release remains blocked.'}
Write-Host 'Release gate schema: PASS'
Write-Host "Decision: $($m.ReleaseDecision)"
