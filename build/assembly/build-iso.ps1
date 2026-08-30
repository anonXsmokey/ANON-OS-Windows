[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$WindowsIso,[Parameter(Mandatory=$true)][string]$ShellPublish,[string]$OutputRoot=(Join-Path $PSScriptRoot '..\out'),[ValidateSet('x64','ARM64')][string]$Architecture='x64',[int]$ImageIndex=1,[switch]$KeepWork)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
foreach($tool in @('dism.exe','oscdimg.exe','reg.exe')){if(-not(Get-Command $tool -ErrorAction SilentlyContinue)){throw "Missing required tool: $tool"}}
if(-not(Test-Path -LiteralPath $WindowsIso)){throw "Windows ISO not found: $WindowsIso"};if(-not(Test-Path -LiteralPath $ShellPublish)){throw "Shell publish directory not found: $ShellPublish"}
$iso=(Resolve-Path $WindowsIso).Path;New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null;$stamp=Get-Date -Format 'yyyyMMdd-HHmmss';$work=Join-Path $OutputRoot "iso-$stamp";$source=Join-Path $work 'source';$mount=Join-Path $work 'mount';New-Item -ItemType Directory -Force -Path $source,$mount|Out-Null
$disk=Mount-DiskImage -ImagePath $iso -PassThru;try{$vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1;if(-not$vol){throw 'Could not resolve mounted Windows ISO drive.'};Copy-Item "$($vol.DriveLetter):\*" $source -Recurse -Force}finally{Dismount-DiskImage -ImagePath $iso}
$wim=Join-Path $source 'sources\install.wim';$esd=Join-Path $source 'sources\install.esd';if(-not(Test-Path $wim)){if(-not(Test-Path $esd)){throw 'Windows source contains neither install.wim nor install.esd.'};$export=Join-Path $work 'install.wim';&dism /Export-Image /SourceImageFile:$esd /SourceIndex:$ImageIndex /DestinationImageFile:$export /Compress:max /CheckIntegrity;if($LASTEXITCODE){throw "DISM export failed: $LASTEXITCODE"};Remove-Item $esd -Force;Move-Item $export $wim -Force}
$images=&dism /Get-WimInfo /WimFile:$wim 2>&1;if($LASTEXITCODE){throw 'Unable to inspect install image.'};if(-not(($images|Select-String -Pattern "Index\s*:\s*$ImageIndex").Count)){throw "Image index $ImageIndex was not found."}
&dism /Mount-Wim /WimFile:$wim /Index:$ImageIndex /MountDir:$mount;if($LASTEXITCODE){throw "DISM mount failed: $LASTEXITCODE"}
$mounted=$true
try{
 $payload=Join-Path $mount 'ProgramData\ANON\Shell\M0';New-Item -ItemType Directory -Force -Path $payload|Out-Null;Copy-Item (Join-Path $ShellPublish '*') $payload -Recurse -Force
 $marker=Join-Path $mount 'ProgramData\ANON\ANON-OS.txt';New-Item -ItemType Directory -Force -Path (Split-Path $marker)|Out-Null;"ANON OS Windows`r`nArchitecture: $Architecture`r`nShell: ANON.Shell.M0`r`n"|Set-Content $marker -Encoding UTF8
 $hive=Join-Path $mount 'Windows\System32\Config\SOFTWARE';$tempHive='ANON_OFFLINE_SOFTWARE';&reg.exe LOAD "HKLM\$tempHive" $hive|Out-Null;if($LASTEXITCODE){throw 'Unable to load offline SOFTWARE hive.'}
 try{$shellExe='C:\ProgramData\ANON\Shell\M0\ANON.Shell.M0.exe';&reg.exe ADD "HKLM\$tempHive\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /t REG_SZ /d ('"'+$shellExe+'"') /f|Out-Null;if($LASTEXITCODE){throw 'Unable to configure Winlogon shell.'}}finally{&reg.exe UNLOAD "HKLM\$tempHive"|Out-Null}
}finally{&dism /Unmount-Wim /MountDir:$mount /Commit;if($LASTEXITCODE){throw "DISM commit failed: $LASTEXITCODE"};$mounted=$false}
$rootMarker=Join-Path $source 'ANON-OS.txt';"ANON OS Windows`r`nArchitecture: $Architecture`r`nImageIndex: $ImageIndex`r`nShell: ProgramData/ANON/Shell/M0/ANON.Shell.M0.exe`r`n"|Set-Content $rootMarker -Encoding UTF8
$bootEtfs=Join-Path $source 'boot\etfsboot.com';$efi=Join-Path $source 'efi\microsoft\boot\efisys.bin';if(-not(Test-Path $bootEtfs)){throw 'BIOS boot image missing.'};if(-not(Test-Path $efi)){throw 'UEFI boot image missing.'}
$outIso=Join-Path $OutputRoot "ANON-OS-Windows-$stamp-$Architecture.iso";&oscdimg -m -o -u2 -udfver102 "-bootdata:2#p0,e,b$bootEtfs#pEF,e,b$efi" $source $outIso;if($LASTEXITCODE){throw "oscdimg failed: $LASTEXITCODE"}
$hash=(Get-FileHash $outIso -Algorithm SHA256).Hash;"$hash  $(Split-Path $outIso -Leaf)"|Set-Content (Join-Path $OutputRoot 'SHA256SUMS.txt') -Encoding ASCII
[ordered]@{Product='ANON OS Windows';Architecture=$Architecture;ImageIndex=$ImageIndex;BuiltUtc=[DateTime]::UtcNow.ToString('o');SourceIsoSha256=(Get-FileHash $iso -Algorithm SHA256).Hash;OutputIso=(Split-Path $outIso -Leaf);OutputIsoSha256=$hash;Validation='STRUCTURE-VALIDATED-VM-REQUIRED'}|ConvertTo-Json|Set-Content (Join-Path $OutputRoot 'BUILD-MANIFEST.json') -Encoding UTF8
Write-Host "ISO assembled: $outIso";if(-not$KeepWork){Remove-Item $work -Recurse -Force}
