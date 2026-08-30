[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$IsoPath,[string]$VmName='ANON-OS-M0-Validation',[string]$OutputPath)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$vmware=Get-Command vmrun.exe -ErrorAction SilentlyContinue;$virtualbox=Get-Command VBoxManage.exe -ErrorAction SilentlyContinue
if(-not$vmware -and -not$virtualbox){throw 'No supported VM runner found. Install VMware Workstation or VirtualBox before release validation.'}
if(-not$OutputPath){$OutputPath=Join-Path (Split-Path $IsoPath) 'VM-VALIDATION-MANIFEST.json'}
$hash=(Get-FileHash -LiteralPath (Resolve-Path $IsoPath) -Algorithm SHA256).Hash
[ordered]@{SchemaVersion=1;VmName=$VmName;Iso=(Resolve-Path $IsoPath).Path;IsoSha256=$hash;Runner=if($virtualbox){'VBoxManage'}else{'vmrun'};VmBoot='REQUIRED';WindowsInstall='REQUIRED';FirstLogon='REQUIRED';ShellSmoke='REQUIRED';ValidatedUtc=$null}|ConvertTo-Json|Set-Content $OutputPath -Encoding UTF8
Write-Host "VM validation manifest created: $OutputPath";Write-Host 'No gate is marked PASS by runner detection alone.'
