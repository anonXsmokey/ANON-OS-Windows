[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$WindowsIso,
    [Parameter(Mandatory=$true)][string]$ShellPublish,
    [string]$OutputRoot = (Join-Path $PSScriptRoot '..\out'),
    [ValidateSet('x64','ARM64')][string]$Architecture = 'x64',
    [int]$ImageIndex = 1,
    [switch]$KeepWork
)

$ErrorActionPreference='Stop'; Set-StrictMode -Version Latest
foreach($tool in @('dism.exe','oscdimg.exe')){if(-not(Get-Command $tool -ErrorAction SilentlyContinue)){throw "Missing required tool: $tool"}}
if(-not(Test-Path -LiteralPath $WindowsIso)){throw "Windows ISO not found: $WindowsIso"}
if(-not(Test-Path -LiteralPath $ShellPublish)){throw "Shell publish directory not found: $ShellPublish"}
$iso=(Resolve-Path $WindowsIso).Path
$root=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss';$work=Join-Path $OutputRoot "iso-$stamp";$source=Join-Path $work 'source';$mount=Join-Path $work 'mount'
New-Item -ItemType Directory -Force -Path $source,$mount|Out-Null

$disk=Mount-DiskImage -ImagePath $iso -PassThru
try{
  $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
  if(-not$vol){throw 'Could not resolve Windows ISO drive.'}
  Copy-Item "$($vol.DriveLetter):\*" $source -Recurse -Force
}finally{Dismount-DiskImage -ImagePath $iso}

$wim=Join-Path $source 'sources\install.wim';$esd=Join-Path $source 'sources\install.esd'
if(-not(Test-Path $wim)){
  if(-not(Test-Path $esd)){throw 'Windows source contains neither sources\install.wim nor sources\install.esd.'}
  $wim=Join-Path $work 'install.wim'
  &dism /Export-Image /SourceImageFile:$esd /SourceIndex:$ImageIndex /DestinationImageFile:$wim /Compress:max /CheckIntegrity
  if($LASTEXITCODE){throw "DISM export failed: $LASTEXITCODE"}
  Remove-Item $esd -Force
  Copy-Item $wim (Join-Path $source 'sources\install.wim') -Force
  $wim=Join-Path $source 'sources\install.wim'
}

$images=&dism /Get-WimInfo /WimFile:$wim 2>&1
if($LASTEXITCODE){throw 'Unable to inspect install image.'}
$indexText=($images|Select-String -Pattern "Index : $ImageIndex").Line
if(-not$indexText){throw "Image index $ImageIndex was not found in install.wim."}

&dism /Mount-Wim /WimFile:$wim /Index:$ImageIndex /MountDir:$mount
if($LASTEXITCODE){throw "DISM mount failed: $LASTEXITCODE"}
$mounted=$true
try{
  $payload=Join-Path $mount 'ProgramData\ANON\Shell\M0';New-Item -ItemType Directory -Force -Path $payload|Out-Null
  Copy-Item (Join-Path $ShellPublish '*') $payload -Recurse -Force
  $startup=Join-Path $mount 'ProgramData\Microsoft\Windows\Start Menu\Programs\Startup';New-Item -ItemType Directory -Force -Path $startup|Out-Null
  $launcher=Join-Path $startup 'ANON OS Shell.cmd'
  "@echo off`r`nstart \"ANON OS Shell\" \"%ProgramData%\ANON\Shell\M0\ANON.Shell.M0.exe\"`r`n"|Set-Content $launcher -Encoding ASCII
  $identity=Join-Path $mount 'ProgramData\ANON\ANON-OS.txt'
  "ANON OS Windows`r`nArchitecture: $Architecture`r`nShell: ANON.Shell.M0`r`n"|Set-Content $identity -Encoding UTF8
  &dism /Image:$mount /Set-ProductKey:00000-00000-00000-00000-00000 2>$null
}finally{
  &dism /Unmount-Wim /MountDir:$mount /Commit
  if($LASTEXITCODE){throw "DISM commit failed: $LASTEXITCODE"}
  $mounted=$false
}

$boot=$source
$bootEtfs=Join-Path $source 'boot\etfsboot.com';$efi=Join-Path $source 'efi\microsoft\boot\efisys.bin'
if(-not(Test-Path $bootEtfs)){throw 'BIOS boot sector boot\etfsboot.com is missing from source ISO.'}
if(-not(Test-Path $efi)){throw 'UEFI boot image efi\microsoft\boot\efisys.bin is missing from source ISO.'}
$outIso=Join-Path $OutputRoot "ANON-OS-Windows-$stamp-$Architecture.iso"
&oscdimg -m -o -u2 -udfver102 "-bootdata:2#p0,e,b$bootEtfs#pEF,e,b$efi" $source $outIso
if($LASTEXITCODE){throw "oscdimg failed: $LASTEXITCODE"}
$hash=(Get-FileHash $outIso -Algorithm SHA256).Hash
$hash|Set-Content (Join-Path $OutputRoot 'SHA256SUMS.txt')
[ordered]@{Product='ANON OS Windows';Architecture=$Architecture;ImageIndex=$ImageIndex;BuiltUtc=[DateTime]::UtcNow.ToString('o');SourceIsoSha256=(Get-FileHash $iso -Algorithm SHA256).Hash;OutputIso=(Split-Path $outIso -Leaf);OutputIsoSha256=$hash;Validation='ASSEMBLED-NOT-VM-VALIDATED'}|ConvertTo-Json|Set-Content (Join-Path $OutputRoot 'BUILD-MANIFEST.json') -Encoding UTF8
Write-Host "ISO assembled: $outIso"
if(-not$KeepWork){Remove-Item $work -Recurse -Force}
