[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$IsoPath,[string]$ExpectedArchitecture='x64')
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$iso=(Resolve-Path $IsoPath).Path
$hash=(Get-FileHash -LiteralPath $iso -Algorithm SHA256).Hash
$disk=$null
try {
  $disk=Mount-DiskImage -ImagePath $iso -PassThru
  $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
  if(-not$vol){throw 'Unable to resolve ISO volume.'}
  $root="$($vol.DriveLetter):\"
  $required=@(
    'bootmgr',
    'boot\etfsboot.com',
    'efi\microsoft\boot\efisys.bin',
    'sources\install.wim',
    'ProgramData\ANON\Shell\M0\ANON.Shell.M0.exe',
    'ProgramData\ANON\ANON-OS.txt'
  )
  foreach($item in $required){if(-not(Test-Path -LiteralPath (Join-Path $root $item))){throw "Required ISO component missing: $item"}}
  $marker=Get-Content (Join-Path $root 'ProgramData\ANON\ANON-OS.txt') -Raw
  if($marker -notmatch 'ANON OS Windows'){throw 'ANON marker content is invalid.'}
  if($marker -notmatch "Architecture:\s*$ExpectedArchitecture"){throw "ISO architecture marker does not match '$ExpectedArchitecture'."}

  $wim=Join-Path $root 'sources\install.wim'
  $wimInfo=& dism.exe /Get-WimInfo /WimFile:$wim 2>&1
  if($LASTEXITCODE){throw 'DISM could not inspect install.wim.'}
  if(-not($wimInfo -match 'Index\s*:\s*1')){throw 'install.wim index 1 was not found.'}

  Write-Host 'ISO structure validation: PASS'
  Write-Host 'ANON payload validation: PASS'
  Write-Host 'Windows image validation: PASS'
  Write-Host "SHA256: $hash"
  Write-Host "Architecture: $ExpectedArchitecture"
} finally {
  if($disk){Dismount-DiskImage -ImagePath $iso -ErrorAction SilentlyContinue}
}
Write-Host 'NOTE: Boot/install/first-logon validation still requires a VM.'
