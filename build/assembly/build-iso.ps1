[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$WindowsIso,
 [Parameter(Mandatory=$true)][string]$ShellPublish,
 [Parameter(Mandatory=$true)][string]$BootstrapPublish,
 [string]$OutputRoot=(Join-Path $PSScriptRoot '..\out'),
 [ValidateSet('x64','ARM64')][string]$Architecture='x64',
 [ValidateRange(1,999)][int]$ImageIndex=6,
 [switch]$KeepWork
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
function Find-Tool([string]$Name,[string[]]$Candidates){$cmd=Get-Command $Name -ErrorAction SilentlyContinue;if($cmd){return $cmd.Source};foreach($p in $Candidates){if(Test-Path -LiteralPath $p){return (Resolve-Path -LiteralPath $p).Path}};throw "Missing required tool: $Name"}
$dism=Find-Tool 'dism.exe' @("$env:SystemRoot\System32\dism.exe")
$reg=Find-Tool 'reg.exe' @("$env:SystemRoot\System32\reg.exe")
$oscdimg=Find-Tool 'oscdimg.exe' @("E:\Windows Kits\10\ADK\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe","E:\Windows Kits\10\ADK\Assessment and Deployment Kit\Deployment Tools\x86\Oscdimg\oscdimg.exe")
$robocopy=Find-Tool 'robocopy.exe' @("$env:SystemRoot\System32\robocopy.exe")
if(-not(Test-Path -LiteralPath $WindowsIso)){throw "Windows ISO not found: $WindowsIso"}
if(-not(Test-Path -LiteralPath $ShellPublish)){throw "Shell publish directory not found: $ShellPublish"}
if(-not(Test-Path -LiteralPath $BootstrapPublish)){throw "Bootstrap publish directory not found: $BootstrapPublish"}
$iso=(Resolve-Path $WindowsIso).Path
New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
$work=Join-Path $OutputRoot "iso-$stamp"
$source=Join-Path $work 'source'
$mount=Join-Path $work 'mount'
$bootMount=Join-Path $work 'bootmount'
New-Item -ItemType Directory -Force -Path $source,$mount,$bootMount|Out-Null
$disk=Mount-DiskImage -ImagePath $iso -PassThru
try{$vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1;if(-not$vol){throw 'Could not resolve mounted Windows ISO drive.'};&$robocopy "$($vol.DriveLetter):\" $source /E /COPY:DAT /DCOPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null;if($LASTEXITCODE -gt 7){throw "ISO source copy failed: $LASTEXITCODE"}}finally{Dismount-DiskImage -ImagePath $iso}
$wim=Join-Path $source 'sources\install.wim'
$esd=Join-Path $source 'sources\install.esd'
if(-not(Test-Path $wim)){if(-not(Test-Path $esd)){throw 'Windows source contains neither install.wim nor install.esd.'};$export=Join-Path $work 'install.wim';&$dism /Export-Image /SourceImageFile:$esd /SourceIndex:$ImageIndex /DestinationImageFile:$export /Compress:max /CheckIntegrity;if($LASTEXITCODE){throw "DISM export failed: $LASTEXITCODE"};Remove-Item $esd -Force;Move-Item $export $wim -Force}
(Get-Item -LiteralPath $wim).IsReadOnly=$false
$images=@(& $dism /Get-WimInfo /WimFile:$wim 2>&1);if($LASTEXITCODE){throw 'Unable to inspect install image.'};$matches=@($images|Select-String -Pattern "Index\s*:\s*$ImageIndex");if($matches.Count -eq 0){throw "Image index $ImageIndex was not found."}
# Customize the installed Windows image.
&$dism /Mount-Wim /WimFile:$wim /Index:$ImageIndex /MountDir:$mount;if($LASTEXITCODE){throw "DISM install.wim mount failed: $LASTEXITCODE"}
try{
 $payload=Join-Path $mount 'ProgramData\ANON\Shell\M0';New-Item -ItemType Directory -Force -Path $payload|Out-Null;Copy-Item (Join-Path $ShellPublish '*') $payload -Recurse -Force
 $bootstrap=Join-Path $mount 'ProgramData\ANON\Shell\Bootstrap';New-Item -ItemType Directory -Force -Path $bootstrap|Out-Null;Copy-Item (Join-Path $BootstrapPublish '*') $bootstrap -Recurse -Force
 $logs=Join-Path $mount 'ProgramData\ANON\Logs';New-Item -ItemType Directory -Force -Path $logs|Out-Null
 $marker=Join-Path $mount 'ProgramData\ANON\ANON-OS.txt';"ANON OS Windows`r`nArchitecture: $Architecture`r`nImageIndex: $ImageIndex`r`nShell: ANON.Shell.M0`r`nBootstrap: ANON.Shell.Bootstrap`r`nSetup: Unattended-ready`r`n"|Set-Content $marker -Encoding UTF8
 $hive=Join-Path $mount 'Windows\System32\Config\SOFTWARE';$tempHive='ANON_OFFLINE_SOFTWARE';&$reg LOAD "HKLM\$tempHive" $hive|Out-Null;if($LASTEXITCODE){throw 'Unable to load offline SOFTWARE hive.'}
 try{$shellExe='C:\ProgramData\ANON\Shell\Bootstrap\ANON.Shell.Bootstrap.exe';&$reg ADD "HKLM\$tempHive\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /t REG_SZ /d $shellExe /f|Out-Null;if($LASTEXITCODE){throw 'Unable to configure Winlogon shell.'};&$reg ADD "HKLM\$tempHive\SOFTWARE\ANON\OS" /v Version /t REG_SZ /d 'Windows-native V1' /f|Out-Null;if($LASTEXITCODE){throw 'Unable to configure ANON registry state.'}}finally{&$reg UNLOAD "HKLM\$tempHive"|Out-Null}
}finally{&$dism /Unmount-Wim /MountDir:$mount /Commit;if($LASTEXITCODE){throw "DISM install.wim commit failed: $LASTEXITCODE"}}
# Add unattended setup assets. Autounattend.xml is discovered from ISO root by Windows Setup.
$answer=Get-Content (Join-Path $PSScriptRoot 'Autounattend.xml') -Raw
$answer=$answer.Replace('<Value>6</Value>','<Value>'+[string]$ImageIndex+'</Value>')
$answer|Set-Content (Join-Path $source 'Autounattend.xml') -Encoding UTF8
$oemScripts=Join-Path $source 'sources\$OEM$\$\Setup\Scripts';New-Item -ItemType Directory -Force -Path $oemScripts|Out-Null
Copy-Item (Join-Path $PSScriptRoot 'SetupComplete.cmd') (Join-Path $oemScripts 'SetupComplete.cmd') -Force
Copy-Item (Join-Path $PSScriptRoot 'ANON-FirstLogon.cmd') (Join-Path $oemScripts 'ANON-FirstLogon.cmd') -Force
# Patch the setup WinPE (boot.wim index 2) so 25H2 ConX media uses the legacy Setup path,
# which honors the answer-file/OOBE automation used by this project.
$bootWim=Join-Path $source 'sources\boot.wim'
if(-not(Test-Path $bootWim)){throw 'Windows source boot.wim missing.'}
&$dism /Mount-Wim /WimFile:$bootWim /Index:2 /MountDir:$bootMount;if($LASTEXITCODE){throw "DISM boot.wim mount failed: $LASTEXITCODE"}
try{
 $winpeSystem32=Join-Path $bootMount 'Windows\System32'
 $setupCmd=Join-Path $winpeSystem32 'ANON-Setup.cmd'
 @'
@echo off
wpeinit
set "MEDIA="
for %%D in (C D E F G H I J K L M N O P Q R S T U V W Y Z) do if exist "%%D:\sources\setup.exe" set "MEDIA=%%D:"
if not defined MEDIA (
  echo ANON OS: Windows Setup media not found.
  pause
  exit /b 1
)
"%MEDIA%\sources\setup.exe" /legacy
exit /b %errorlevel%
'@|Set-Content $setupCmd -Encoding ASCII
 @'
[LaunchApps]
%SYSTEMROOT%\System32\ANON-Setup.cmd
'@|Set-Content (Join-Path $winpeSystem32 'winpeshl.ini') -Encoding ASCII
}finally{&$dism /Unmount-Wim /MountDir:$bootMount /Commit;if($LASTEXITCODE){throw "DISM boot.wim commit failed: $LASTEXITCODE"}}
$rootMarker=Join-Path $source 'ANON-OS.txt';"ANON OS Windows`r`nArchitecture: $Architecture`r`nImageIndex: $ImageIndex`r`nShell: ProgramData/ANON/Shell/Bootstrap/ANON.Shell.Bootstrap.exe`r`nInstaller: unattended + legacy-Setup bridge`r`n"|Set-Content $rootMarker -Encoding UTF8
$bootEtfs=Join-Path $source 'boot\etfsboot.com';$efi=Join-Path $source 'efi\microsoft\boot\efisys.bin';if(-not(Test-Path $bootEtfs)){throw 'BIOS boot image missing.'};if(-not(Test-Path $efi)){throw 'UEFI boot image missing.'}
$bootOrder=Join-Path $work 'bootOrder.txt';@('boot\bcd','boot\boot.sdi','boot\bootfix.bin','boot\bootsect.exe','boot\etfsboot.com','boot\memtest.efi','boot\memtest.exe','boot\en-us\bootsect.exe.mui','boot\fonts\chs_boot.ttf','boot\fonts\cht_boot.ttf','boot\fonts\jpn_boot.ttf','boot\fonts\kor_boot.ttf','boot\fonts\wgl4_boot.ttf','sources\boot.wim')|Set-Content $bootOrder -Encoding ASCII
$outIso=Join-Path $OutputRoot "ANON-OS-Windows-$stamp-$Architecture.iso"
&$oscdimg -m -o -u2 -udfver102 "-yo$bootOrder" "-bootdata:2#p0,e,b$bootEtfs#pEF,e,b$efi" $source $outIso;if($LASTEXITCODE){throw "oscdimg failed: $LASTEXITCODE"}
$hash=(Get-FileHash $outIso -Algorithm SHA256).Hash;"$hash  $(Split-Path $outIso -Leaf)"|Set-Content (Join-Path $OutputRoot 'SHA256SUMS.txt') -Encoding ASCII
[ordered]@{Product='ANON OS Windows';Architecture=$Architecture;ImageIndex=$ImageIndex;BuiltUtc=[DateTime]::UtcNow.ToString('o');SourceIsoSha256=(Get-FileHash $iso -Algorithm SHA256).Hash;OutputIso=(Split-Path $outIso -Leaf);OutputIsoSha256=$hash;Validation='STRUCTURE-VALIDATED-VM-REQUIRED';Installer='Autounattend + boot.wim legacy Setup bridge';Shell='Bootstrap with Explorer fallback'}|ConvertTo-Json|Set-Content (Join-Path $OutputRoot 'BUILD-MANIFEST.json') -Encoding UTF8
Write-Host "ISO assembled: $outIso"
if(-not$KeepWork){Remove-Item $work -Recurse -Force}
