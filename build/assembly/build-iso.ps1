[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$WindowsIso,
 [Parameter(Mandatory=$true)][string]$ShellPublish,
 [Parameter(Mandatory=$true)][string]$BootstrapPublish,
 [Parameter(Mandatory=$true)][string]$PerformancePetPublish,
 [string]$OutputRoot=(Join-Path $PSScriptRoot '..\out'),
 [ValidateSet('x64','ARM64')][string]$Architecture='x64',
 [ValidateRange(1,999)][int]$ImageIndex=6,
 [switch]$KeepWork
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
function Find-Tool([string]$Name,[string[]]$Candidates){
    $cmd=Get-Command $Name -ErrorAction SilentlyContinue
    if($cmd){return $cmd.Source}
    foreach($p in $Candidates){if($p -and (Test-Path -LiteralPath $p)){return (Resolve-Path -LiteralPath $p).Path}}
    throw "Missing required tool: $Name"
}
$dism=Find-Tool 'dism.exe' @("$env:SystemRoot\System32\dism.exe")
$reg=Find-Tool 'reg.exe' @("$env:SystemRoot\System32\reg.exe")
$oscdimgCandidates=@(
    $env:OSCDIMG_PATH,
    "E:\Windows Kits\10\ADK\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe",
    "E:\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe",
    "${env:ProgramFiles(x86)}\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe",
    "$env:ProgramFiles\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe",
    "${env:ProgramFiles(x86)}\Windows Kits\10\ADK\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe"
)
$oscdimg=Find-Tool 'oscdimg.exe' $oscdimgCandidates
$robocopy=Find-Tool 'robocopy.exe' @("$env:SystemRoot\System32\robocopy.exe")
foreach($p in @($WindowsIso,$ShellPublish,$BootstrapPublish,$PerformancePetPublish)){
    if(-not(Test-Path -LiteralPath $p)){throw "Required input missing: $p"}
}
$iso=(Resolve-Path $WindowsIso).Path
New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
$work=Join-Path $OutputRoot "iso-$stamp"
$source=Join-Path $work 'source'
$mount=Join-Path $work 'mount'
$bootMount=Join-Path $work 'bootmount'
New-Item -ItemType Directory -Force -Path $source,$mount,$bootMount|Out-Null
try {
    $disk=Mount-DiskImage -ImagePath $iso -PassThru
    try {
        $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
        if(-not$vol){throw 'Could not resolve mounted Windows ISO drive.'}
        &$robocopy "$($vol.DriveLetter):\" $source /E /A-:R /COPY:DAT /DCOPY:DAT /R:5 /W:2 /NFL /NDL /NJH /NJS /NP|Out-Null
        if($LASTEXITCODE -ge 8){throw "ISO source copy failed: $LASTEXITCODE"}
    }finally{Dismount-DiskImage -ImagePath $iso -ErrorAction SilentlyContinue}

    $wim=Join-Path $source 'sources\install.wim'
    $esd=Join-Path $source 'sources\install.esd'
    if(-not(Test-Path $wim)){
        if(-not(Test-Path $esd)){throw 'Windows source contains neither install.wim nor install.esd.'}
        $export=Join-Path $work 'install.wim'
        &$dism /Export-Image /SourceImageFile:$esd /SourceIndex:$ImageIndex /DestinationImageFile:$export /Compress:max /CheckIntegrity
        if($LASTEXITCODE){throw "DISM export failed: $LASTEXITCODE"}
        Remove-Item $esd -Force
        Move-Item $export $wim -Force
    }
    (Get-Item $wim).IsReadOnly=$false

    $images=@(&$dism /Get-WimInfo /WimFile:$wim 2>&1)
    if($LASTEXITCODE){throw 'Unable to inspect install.wim.'}
    $indexPattern="^\s*Index\s*:\s*$([regex]::Escape([string]$ImageIndex))\s*$"
    $indexFound=@($images|Where-Object{$_-match $indexPattern}).Count -gt 0
    if(-not$indexFound){
        Write-Host 'Available install.wim indexes:'
        $images|Where-Object{$_-match '^\s*Index\s*:'}|ForEach-Object{Write-Host "  $_"}
        throw "Image index $ImageIndex was not found."
    }

    &$dism /Mount-Wim /WimFile:$wim /Index:$ImageIndex /MountDir:$mount
    if($LASTEXITCODE){throw "DISM install.wim mount failed: $LASTEXITCODE"}
    try{
        $payload=Join-Path $mount 'ProgramData\ANON\Shell\M0'
        New-Item -ItemType Directory -Force -Path $payload|Out-Null
        Copy-Item (Join-Path $ShellPublish '*') $payload -Recurse -Force

        $bootstrap=Join-Path $mount 'ProgramData\ANON\Shell\Bootstrap'
        New-Item -ItemType Directory -Force -Path $bootstrap|Out-Null
        Copy-Item (Join-Path $BootstrapPublish '*') $bootstrap -Recurse -Force

        $pet=Join-Path $mount 'ProgramData\ANON\Shell\PerformancePet'
        New-Item -ItemType Directory -Force -Path $pet|Out-Null
        Copy-Item (Join-Path $PerformancePetPublish '*') $pet -Recurse -Force

        $logs=Join-Path $mount 'ProgramData\ANON\Logs'
        New-Item -ItemType Directory -Force -Path $logs|Out-Null

        $scripts=Join-Path $mount 'Windows\Setup\Scripts'
        New-Item -ItemType Directory -Force -Path $scripts|Out-Null
        Copy-Item (Join-Path $PSScriptRoot 'SetupComplete.cmd') (Join-Path $scripts 'SetupComplete.cmd') -Force

        $answer=Get-Content (Join-Path $PSScriptRoot 'Autounattend.xml') -Raw
        $answer=$answer.Replace('<Value>6</Value>','<Value>'+[string]$ImageIndex+'</Value>')
        $answerPath=Join-Path $source 'Autounattend.xml'
        $answer|Set-Content $answerPath -Encoding UTF8

        # Do not embed the full media answer file into install.wim\Windows\Panther.
        # Windows Setup itself caches and annotates the active answer file in Panther
        # as configuration passes and reboots progress. Duplicating the complete
        # windowsPE+specialize+oobeSystem answer file there can make Setup process
        # an unmanaged copy on the first reboot and trigger the generic
        # "computer restarted unexpectedly" installation failure.
        $marker=Join-Path $mount 'ProgramData\ANON\ANON-OS.txt'
        "ANON OS Windows`r`nArchitecture: $Architecture`r`nImageIndex: $ImageIndex`r`nInstaller: Windows 11 25H2 legacy Setup bridge`r`nShell: FirstLogonCommands -> HKCU Run -> Bootstrap -> ANON.Shell.M0`r`nPerformancePet: ANON.PerformancePet`r`nAnswerFile: Windows Setup-managed caching (no embedded Panther override)`r`n"|Set-Content $marker -Encoding UTF8

        $hive=Join-Path $mount 'Windows\System32\Config\SOFTWARE'
        $tempHive='ANON_OFFLINE_SOFTWARE'
        &$reg LOAD "HKLM\$tempHive" $hive|Out-Null
        if($LASTEXITCODE){throw 'Unable to load offline SOFTWARE hive.'}
        try{
            $regVersion='Windows-native V1'
            &$reg ADD "HKLM\$tempHive\SOFTWARE\ANON\OS" /v Version /t REG_SZ /d $regVersion /f|Out-Null
            if($LASTEXITCODE){throw 'Unable to configure ANON OS version.'}
        }finally{&$reg UNLOAD "HKLM\$tempHive"|Out-Null}
    }finally{
        &$dism /Unmount-Wim /MountDir:$mount /Commit
        if($LASTEXITCODE){throw "DISM install.wim commit failed: $LASTEXITCODE"}
    }

    # Windows 11 24H2/25H2 installation media can route the normal boot-time
    # launcher through the newer ConX Setup client. For ANON we force the
    # legacy Setup engine used by the unattended workflow.
    # IMPORTANT: use X:\sources\setup.exe, not X:\setup.exe.
    $bootWim=Join-Path $source 'sources\boot.wim'
    if(-not(Test-Path $bootWim)){throw 'Windows source boot.wim missing.'}
    &$dism /Mount-Wim /WimFile:$bootWim /Index:2 /MountDir:$bootMount
    if($LASTEXITCODE){throw "DISM boot.wim mount failed: $LASTEXITCODE"}
    try{
        $sys=Join-Path $bootMount 'Windows\System32'
        $answer|Set-Content (Join-Path $sys 'Autounattend.xml') -Encoding UTF8
        $answer|Set-Content (Join-Path $bootMount 'Autounattend.xml') -Encoding UTF8
        @'
[LaunchApps]
%SYSTEMDRIVE%\sources\setup.exe, /legacy
'@|Set-Content (Join-Path $sys 'winpeshl.ini') -Encoding ASCII
        "ANON legacy Setup bridge enabled.`r`nLaunch: %SYSTEMDRIVE%\sources\setup.exe /legacy`r`nAnswer file: media-root Autounattend.xml + WinPE embedded fallback`r`n"|Set-Content (Join-Path $sys 'ANON-SETUP-MARKER.txt') -Encoding ASCII

        # Self-check the exact strings that will later be validated from the ISO.
        $winpeCheck=Get-Content (Join-Path $sys 'winpeshl.ini') -Raw
        if($winpeCheck -notmatch '(?im)^%SYSTEMDRIVE%\\sources\\setup\.exe,\s*/legacy\s*$'){
            throw 'Generated WinPE bridge self-check failed.'
        }
        $markerCheck=Get-Content (Join-Path $sys 'ANON-SETUP-MARKER.txt') -Raw
        if($markerCheck -notmatch '(?im)^Launch:\s*%SYSTEMDRIVE%\\sources\\setup\.exe\s+/legacy\s*$'){
            throw 'Generated WinPE marker self-check failed.'
        }
    }finally{
        &$dism /Unmount-Wim /MountDir:$bootMount /Commit
        if($LASTEXITCODE){throw "DISM boot.wim commit failed: $LASTEXITCODE"}
    }

    $rootMarker=Join-Path $source 'ANON-OS.txt'
    "ANON OS Windows`r`nArchitecture: $Architecture`r`nImageIndex: $ImageIndex`r`nInstaller: boot.wim winpeshl.ini -> X:\sources\setup.exe /legacy -> media-root Autounattend.xml`r`nShell: FirstLogonCommands -> HKCU Run -> Bootstrap -> ANON.Shell.M0`r`nPerformancePet: ANON.PerformancePet`r`nAnswerFile: Windows Setup-managed caching (no embedded Panther override)`r`n"|Set-Content $rootMarker -Encoding UTF8

    foreach($required in @('bootmgr','bootmgr.efi','boot\bcd','efi\microsoft\boot\bcd','sources\boot.wim','sources\install.wim','Autounattend.xml')){
        if(-not(Test-Path (Join-Path $source $required))){throw "Required release component missing: $required"}
    }
    $bootEtfs=Join-Path $source 'boot\etfsboot.com'
    $efi=Join-Path $source 'efi\microsoft\boot\efisys.bin'
    if(-not(Test-Path $bootEtfs)){throw 'BIOS boot image missing.'}
    if(-not(Test-Path $efi)){throw 'UEFI boot image missing.'}

    $bootOrderCandidates=@(
        'boot\bcd','boot\boot.sdi','boot\bootfix.bin','boot\bootsect.exe','boot\etfsboot.com',
        'boot\memtest.efi','boot\memtest.exe','boot\en-us\bootsect.exe.mui',
        'boot\fonts\chs_boot.ttf','boot\fonts\cht_boot.ttf','boot\fonts\jpn_boot.ttf',
        'boot\fonts\kor_boot.ttf','boot\fonts\wgl4_boot.ttf','sources\boot.wim'
    )
    $bootOrderItems=@($bootOrderCandidates | Where-Object { Test-Path (Join-Path $source $_) })
    if($bootOrderItems.Count -eq 0){throw 'No valid boot-order optimization files were found.'}
    $bootOrder=Join-Path $work 'bootOrder.txt'
    $bootOrderItems|Set-Content $bootOrder -Encoding ASCII

    $outIso=Join-Path $OutputRoot "ANON-OS-Windows-$stamp-$Architecture.iso"
    &$oscdimg -m -o -u2 -udfver102 "-yo$bootOrder" "-bootdata:2#p0,e,b$bootEtfs#pEF,e,b$efi" $source $outIso
    if($LASTEXITCODE){throw "oscdimg failed: $LASTEXITCODE"}

    $hash=(Get-FileHash $outIso -Algorithm SHA256).Hash
    "$hash  $(Split-Path $outIso -Leaf)"|Set-Content (Join-Path $OutputRoot 'SHA256SUMS.txt') -Encoding ASCII
    [ordered]@{
        Product='ANON OS Windows'
        Architecture=$Architecture
        ImageIndex=$ImageIndex
        BuiltUtc=[DateTime]::UtcNow.ToString('o')
        SourceIsoSha256=(Get-FileHash $iso -Algorithm SHA256).Hash
        OutputIso=(Split-Path $outIso -Leaf)
        OutputIsoSha256=$hash
        Validation='STRUCTURE+PAYLOAD;VM-FIRST-BOOT-REQUIRED'
        Installer='boot.wim winpeshl.ini -> X:\sources\setup.exe /legacy + media-root Autounattend.xml + Windows Setup-managed Panther cache'
        Shell='FirstLogonCommands -> HKCU Run -> Bootstrap -> ANON.Shell.M0'
        PerformancePet='ANON.PerformancePet'
        AnswerFile='Media root + WinPE embedded; no install.wim Panther override'
    }|ConvertTo-Json|Set-Content (Join-Path $OutputRoot 'BUILD-MANIFEST.json') -Encoding UTF8
    Write-Host "ISO assembled: $outIso"
}finally{
    if(-not$KeepWork -and (Test-Path $work)){Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue}
}