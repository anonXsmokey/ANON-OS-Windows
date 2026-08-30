[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$IsoPath,
 [string]$ExpectedArchitecture='x64',
 [ValidateRange(1,999)][int]$ExpectedImageIndex=6
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$iso=(Resolve-Path $IsoPath).Path
$hash=(Get-FileHash -LiteralPath $iso -Algorithm SHA256).Hash
$disk=$null;$mountedWim=$false;$mountDir=$null
try {
  $disk=Mount-DiskImage -ImagePath $iso -PassThru
  $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
  if(-not$vol){throw 'Unable to resolve ISO volume.'}
  $root="$($vol.DriveLetter):\"
  $required=@('bootmgr','boot\etfsboot.com','efi\microsoft\boot\efisys.bin','sources\install.wim','ANON-OS.txt')
  foreach($item in $required){if(-not(Test-Path -LiteralPath (Join-Path $root $item))){throw "Required ISO component missing: $item"}}
  $marker=Get-Content (Join-Path $root 'ANON-OS.txt') -Raw
  if($marker -notmatch 'ANON OS Windows'){throw 'ANON marker content is invalid.'}
  if($marker -notmatch "Architecture:\s*$ExpectedArchitecture"){throw "ISO architecture marker does not match '$ExpectedArchitecture'."}

  $wim=Join-Path $root 'sources\install.wim'
  $wimInfo=& dism.exe /Get-WimInfo /WimFile:$wim 2>&1
  if($LASTEXITCODE){throw 'DISM could not inspect install.wim.'}
  if(-not($wimInfo -match "Index\s*:\s*$ExpectedImageIndex")){throw "install.wim index $ExpectedImageIndex was not found."}

  $mountDir=Join-Path ([System.IO.Path]::GetTempPath()) ('ANON-ISO-VALIDATE-'+[guid]::NewGuid().ToString('N'))
  New-Item -ItemType Directory -Force -Path $mountDir|Out-Null
  & dism.exe /Mount-Wim /WimFile:$wim /Index:$ExpectedImageIndex /MountDir:$mountDir /ReadOnly 2>&1|Write-Host
  if($LASTEXITCODE){throw "DISM read-only mount failed: $LASTEXITCODE"}
  $mountedWim=$true
  $payload=Join-Path $mountDir 'ProgramData\ANON\Shell\M0\ANON.Shell.M0.exe'
  $anonMarker=Join-Path $mountDir 'ProgramData\ANON\ANON-OS.txt'
  foreach($item in @($payload,$anonMarker)){if(-not(Test-Path -LiteralPath $item)){throw "Required ANON image component missing: $item"}}
  Write-Host 'ISO structure validation: PASS'
  Write-Host 'ANON payload validation: PASS'
  Write-Host 'Windows image validation: PASS'
  Write-Host "Image index: $ExpectedImageIndex"
  Write-Host "SHA256: $hash"
  Write-Host "Architecture: $ExpectedArchitecture"
} finally {
  if($mountedWim){& dism.exe /Unmount-Wim /MountDir:$mountDir /Discard 2>&1|Write-Host}
  if($mountDir -and (Test-Path $mountDir)){Remove-Item $mountDir -Recurse -Force -ErrorAction SilentlyContinue}
  if($disk){Dismount-DiskImage -ImagePath $iso -ErrorAction SilentlyContinue}
}
Write-Host 'NOTE: Boot/install/first-logon validation still requires a VM.'
