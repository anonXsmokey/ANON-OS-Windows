[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$IsoPath,[string]$ExpectedArchitecture='x64')
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$iso=(Resolve-Path $IsoPath).Path;$hash=(Get-FileHash -LiteralPath $iso -Algorithm SHA256).Hash
$disk=Mount-DiskImage -ImagePath $iso -PassThru
try{
 $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
 if(-not$vol){throw 'Unable to resolve ISO volume.'}
 $root="$($vol.DriveLetter):\"
 $required=@('bootmgr','boot\etfsboot.com','efi\microsoft\boot\efisys.bin','sources\install.wim','ProgramData\ANON\Shell\M0\ANON.Shell.M0.exe','ProgramData\ANON\ANON-OS.txt')
 foreach($item in $required){if(-not(Test-Path (Join-Path $root $item))){throw "Required ISO component missing: $item"}}
 $marker=Get-Content (Join-Path $root 'ProgramData\ANON\ANON-OS.txt') -Raw
 if($marker -notmatch 'ANON OS Windows'){throw 'ANON marker content is invalid.'}
 Write-Host 'ISO structure validation: PASS'
 Write-Host 'ANON payload validation: PASS'
 Write-Host "SHA256: $hash"
 Write-Host "Architecture requested: $ExpectedArchitecture"
}finally{Dismount-DiskImage -ImagePath $iso}
Write-Host 'NOTE: Boot/install validation still requires a VM.'
