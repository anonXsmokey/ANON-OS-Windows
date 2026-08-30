[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$IsoPath,
 [string]$VmName='ANON-OS-M0-Smoke',
 [string]$WorkDir=(Join-Path $PSScriptRoot '..\out\vm'),
 [int]$BootWaitSeconds=30
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$vbox=Get-Command VBoxManage.exe -ErrorAction SilentlyContinue
if(-not$vbox){throw 'VBoxManage.exe is required for automated VM smoke testing.'}
New-Item -ItemType Directory -Force -Path $WorkDir|Out-Null
$vmDir=Join-Path $WorkDir $VmName
$created=$false
try {
  & $vbox.Source createvm --name $VmName --basefolder $WorkDir --register
  if($LASTEXITCODE){throw "VirtualBox createvm failed: $LASTEXITCODE"}
  $created=$true
  & $vbox.Source modifyvm $VmName --memory 4096 --cpus 2 --firmware efi --vram 128 --boot1 dvd --boot2 disk --boot3 none
  if($LASTEXITCODE){throw "VirtualBox modifyvm failed: $LASTEXITCODE"}
  $diskPath=Join-Path $vmDir "$VmName.vdi"
  & $vbox.Source createhd --filename $diskPath --size 40960
  if($LASTEXITCODE){throw "VirtualBox createhd failed: $LASTEXITCODE"}
  & $vbox.Source storagectl $VmName --name SATA --add sata --controller IntelAhci
  & $vbox.Source storageattach $VmName --storagectl SATA --port 0 --device 0 --type hdd --medium $diskPath
  & $vbox.Source storagectl $VmName --name IDE --add ide
  & $vbox.Source storageattach $VmName --storagectl IDE --port 1 --device 0 --type dvddrive --medium (Resolve-Path $IsoPath).Path
  if($LASTEXITCODE){throw 'VirtualBox storage configuration failed.'}

  & $vbox.Source startvm $VmName --type headless
  if($LASTEXITCODE){throw "VirtualBox startvm failed: $LASTEXITCODE"}
  Start-Sleep -Seconds $BootWaitSeconds

  $info=& $vbox.Source showvminfo $VmName --machinereadable 2>&1
  $state=($info|Select-String '^VMState="').Line
  if(-not$state){throw 'Unable to read VM state.'}
  if($state -notmatch 'running|paused'){throw "VM did not reach a running state: $state"}

  $result=[ordered]@{
    VmName=$VmName
    IsoSha256=(Get-FileHash -LiteralPath (Resolve-Path $IsoPath) -Algorithm SHA256).Hash
    VmState=$state
    BootHarness='PASS'
    InstallValidation='REQUIRED'
    FirstLogonValidation='REQUIRED'
    ShellSmokeTest='REQUIRED'
    CreatedUtc=[DateTime]::UtcNow.ToString('o')
  }
  $result|ConvertTo-Json|Set-Content (Join-Path $WorkDir 'VM-SMOKE-RESULT.json') -Encoding UTF8
  Write-Host 'VM boot harness: PASS'
} finally {
  if($created){try{&$vbox.Source controlvm $VmName poweroff 2>$null}catch{}}
  if($created){try{&$vbox.Source unregistervm $VmName --delete 2>$null}catch{}}
}
