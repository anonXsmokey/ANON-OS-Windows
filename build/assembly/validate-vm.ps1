[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$IsoPath,[string]$VmName='ANON-OS-M0-Validation')
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$vmware=Get-Command vmrun.exe -ErrorAction SilentlyContinue;$virtualbox=Get-Command VBoxManage.exe -ErrorAction SilentlyContinue
if(-not$vmware -and -not$virtualbox){throw 'No supported VM runner found. Install VMware Workstation or VirtualBox before release validation.'}
$hash=(Get-FileHash -LiteralPath (Resolve-Path $IsoPath) -Algorithm SHA256).Hash
[ordered]@{VmName=$VmName;Iso=(Resolve-Path $IsoPath).Path;IsoSha256=$hash;Validation='RUNNER-DETECTED';Runner=if($virtualbox){'VBoxManage'}else{'vmrun'};BootInstallRequired=$true;ShellSmokeTestRequired=$true}|ConvertTo-Json|Set-Content (Join-Path (Split-Path $IsoPath) 'VM-VALIDATION-MANIFEST.json') -Encoding UTF8
Write-Host 'VM validation harness ready.'
Write-Host 'A VM boot/install/shell smoke test must be executed before release.'
