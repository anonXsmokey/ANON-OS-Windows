[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$IsoPath,
 [string]$VmName='ANON-OS-M0-Smoke',
 [string]$WorkDir=(Join-Path $PSScriptRoot '..\out\vm')
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$vbox=Get-Command VBoxManage.exe -ErrorAction SilentlyContinue
if(-not$vbox){throw 'VBoxManage.exe is required for automated VM smoke testing.'}
New-Item -ItemType Directory -Force -Path $WorkDir|Out-Null
$vmDir=Join-Path $WorkDir $VmName
&$vbox.Source createvm --name $VmName --basefolder $WorkDir --register
try{
 &$vbox.Source modifyvm $VmName --memory 4096 --cpus 2 --firmware efi --vram 128 --boot1 dvd --boot2 disk
 &$vbox.Source createhd --filename (Join-Path $vmDir "$VmName.vdi") --size 40960
 &$vbox.Source storagectl $VmName --name SATA --add sata --controller IntelAhci
 &$vbox.Source storageattach $VmName --storagectl SATA --port 0 --device 0 --type hdd --medium (Join-Path $vmDir "$VmName.vdi")
 &$vbox.Source storagectl $VmName --name IDE --add ide
 &$vbox.Source storageattach $VmName --storagectl IDE --port 1 --device 0 --type dvddrive --medium (Resolve-Path $IsoPath).Path
 &$vbox.Source startvm $VmName --type headless
 Start-Sleep -Seconds 20
 $state=&$vbox.Source showvminfo $VmName --machinereadable | Select-String 'VMState=' | Select-Object -First 1
 if(-not$state){throw 'Unable to read VM state.'}
 Write-Host "VM started: $state"
 Write-Host 'Automated harness reached VM boot. Interactive installation/first-logon smoke test remains required before release.'
}finally{
 try{&$vbox.Source controlvm $VmName poweroff 2>$null}catch{}
 try{&$vbox.Source unregistervm $VmName --delete 2>$null}catch{}
}
