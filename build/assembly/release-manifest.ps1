[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$IsoPath,[string]$OutputPath)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
if(-not$OutputPath){$OutputPath=Join-Path (Split-Path -Parent $IsoPath) 'RELEASE-MANIFEST.json'}
$resolved=(Resolve-Path $IsoPath).Path
$hash=(Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash
$info=Get-Item -LiteralPath $resolved
$manifest=[ordered]@{
 Product='ANON OS Windows';Channel='private-alpha';Status='candidate'
 Version=(Get-Date).ToUniversalTime().ToString('yyyy.MM.dd.HHmm');Architecture='x64'
 IsoPath=$resolved;IsoBytes=$info.Length;Sha256=$hash;GeneratedUtc=[DateTime]::UtcNow.ToString('o')
 Gates=[ordered]@{
  ShellBuild='required'
  IsoStructure='required'
  VmBoot='required'
  WindowsInstall='required'
  FirstLogon='required'
  ShellSmoke='required'
  Gaming='required'
  Recovery='required'
  Benchmark='required'
 }
 ReleaseDecision='BLOCKED_UNTIL_ALL_GATES_PASS'
}
$manifest|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $OutputPath -Encoding UTF8
Write-Host "Release manifest: $OutputPath";Write-Host "SHA256: $hash"