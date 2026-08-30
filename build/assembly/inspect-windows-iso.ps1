[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$WindowsIso)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
foreach($tool in @('dism.exe')){if(-not(Get-Command $tool -ErrorAction SilentlyContinue)){throw "Missing required tool: $tool"}}
if(-not(Test-Path -LiteralPath $WindowsIso)){throw "Windows ISO not found: $WindowsIso"}
$iso=(Resolve-Path $WindowsIso).Path;$disk=$null
try{
 $disk=Mount-DiskImage -ImagePath $iso -PassThru
 $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
 if(-not$vol){throw 'Could not resolve mounted ISO volume.'}
 $root="$($vol.DriveLetter):\";$wim=Join-Path $root 'sources\install.wim';$esd=Join-Path $root 'sources\install.esd'
 if(Test-Path $wim){$image=$wim;$format='WIM'}elseif(Test-Path $esd){$image=$esd;$format='ESD'}else{throw 'ISO contains neither sources\install.wim nor sources\install.esd.'}
 $info=&dism.exe /Get-WimInfo /WimFile:$image 2>&1
 if($LASTEXITCODE){throw 'DISM could not inspect the Windows installation image.'}
 $boot=[ordered]@{BIOS=(Test-Path (Join-Path $root 'boot\etfsboot.com'));UEFI=(Test-Path (Join-Path $root 'efi\microsoft\boot\efisys.bin'))}
 [ordered]@{Iso=$iso;Sha256=(Get-FileHash $iso -Algorithm SHA256).Hash;ImageFormat=$format;ImagePath=$image;BIOSBoot=$boot.BIOS;UEFIBoot=$boot.UEFI;WimInfo=($info -join "`n")}|ConvertTo-Json -Depth 5
}finally{if($disk){Dismount-DiskImage -ImagePath $iso -ErrorAction SilentlyContinue}}
