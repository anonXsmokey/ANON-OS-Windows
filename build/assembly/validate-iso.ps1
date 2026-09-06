[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$IsoPath,
    [string]$ExpectedArchitecture='x64',
    [ValidateRange(1,999)][int]$ExpectedImageIndex=6
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $IsoPath)){throw "ISO not found: $IsoPath"}
$iso=(Resolve-Path $IsoPath).Path
$hash=(Get-FileHash $iso -Algorithm SHA256).Hash
$disk=$null;$installMounted=$false;$bootMounted=$false;$installMount=$null;$bootMount=$null
try {
    $disk=Mount-DiskImage -ImagePath $iso -PassThru
    $vol=$disk|Get-Volume|Where-Object DriveLetter|Select-Object -First 1
    if(-not$vol){throw 'Unable to resolve ISO volume.'}
    $root="$($vol.DriveLetter):\"

    $required=@(
        'bootmgr','bootmgr.efi','boot\etfsboot.com','boot\bcd',
        'efi\microsoft\boot\efisys.bin','efi\microsoft\boot\bcd',
        'sources\boot.wim','sources\install.wim','Autounattend.xml','ANON-OS.txt'
    )
    foreach($item in $required){
        if(-not(Test-Path (Join-Path $root $item))){throw "Required ISO component missing: $item"}
    }

    $marker=Get-Content (Join-Path $root 'ANON-OS.txt') -Raw
    if($marker -notmatch 'ANON OS Windows'){throw 'ANON marker invalid.'}
    if($marker -notmatch "Architecture:\s*$ExpectedArchitecture"){throw 'Architecture marker mismatch.'}
    if($marker -notmatch '(?m)^PerformancePet:\s*ANON\.PerformancePet\s*$'){throw 'Performance Pet marker missing.'}
    if($marker -notmatch '(?i)setup\.exe\s+/unattend:X:\\Autounattend\.xml'){throw 'Release marker does not identify the explicit unattended Setup bridge.'}

    $wim=Join-Path $root 'sources\install.wim'
    $info=((&dism.exe /English /Get-WimInfo /WimFile:$wim 2>&1|Out-String))
    if($LASTEXITCODE){throw 'DISM could not inspect install.wim.'}
    $indexPattern="(?m)^\s*Index\s*:\s*$([regex]::Escape([string]$ExpectedImageIndex))\s*$"
    if($info -notmatch $indexPattern){throw "install.wim index $ExpectedImageIndex not found. DISM output: $info"}

    $installMount=Join-Path ([IO.Path]::GetTempPath()) ('ANON-ISO-INSTALL-'+[guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Force -Path $installMount|Out-Null
    &dism.exe /Mount-Wim /WimFile:$wim /Index:$ExpectedImageIndex /MountDir:$installMount /ReadOnly 2>&1|Write-Host
    if($LASTEXITCODE){throw "install.wim read-only mount failed: $LASTEXITCODE"}
    $installMounted=$true

    foreach($item in @(
        'ProgramData\ANON\Shell\M0\ANON.Shell.M0.exe',
        'ProgramData\ANON\Shell\Bootstrap\ANON.Shell.Bootstrap.exe',
        'ProgramData\ANON\Shell\PerformancePet\ANON.PerformancePet.exe',
        'ProgramData\ANON\ANON-OS.txt',
        'Windows\Setup\Scripts\SetupComplete.cmd',
        'Windows\Panther\unattend.xml'
    )){
        if(-not(Test-Path (Join-Path $installMount $item))){throw "Required installed-image component missing: $item"}
    }

    $bootWim=Join-Path $root 'sources\boot.wim'
    $bootMount=Join-Path ([IO.Path]::GetTempPath()) ('ANON-ISO-BOOT-'+[guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Force -Path $bootMount|Out-Null
    &dism.exe /Mount-Wim /WimFile:$bootWim /Index:2 /MountDir:$bootMount /ReadOnly 2>&1|Write-Host
    if($LASTEXITCODE){throw "boot.wim read-only mount failed: $LASTEXITCODE"}
    $bootMounted=$true

    foreach($item in @(
        'Windows\System32\winpeshl.ini',
        'Windows\System32\Autounattend.xml',
        'Windows\System32\ANON-SETUP-MARKER.txt'
    )){
        if(-not(Test-Path (Join-Path $bootMount $item))){throw "Required installer component missing: $item"}
    }

    $winpe=Get-Content (Join-Path $bootMount 'Windows\System32\winpeshl.ini') -Raw
    if($winpe -notmatch '(?i)setup\.exe,\s*/unattend:%SYSTEMDRIVE%\\Autounattend\.xml'){
        throw 'boot.wim does not explicitly launch Setup with the embedded answer file.'
    }

    $bootAnswer=Get-Content (Join-Path $bootMount 'Windows\System32\Autounattend.xml') -Raw
    if($bootAnswer -notmatch '/IMAGE/INDEX' -or $bootAnswer -notmatch 'oobeSystem' -or $bootAnswer -notmatch '<ProductKey>'){
        throw 'Embedded boot.wim Autounattend.xml is missing required Setup/OOBE/ProductKey configuration.'
    }

    $answer=Get-Content (Join-Path $root 'Autounattend.xml') -Raw
    if($answer -notmatch '/IMAGE/INDEX' -or $answer -notmatch 'oobeSystem' -or $answer -notmatch '<ProductKey>'){
        throw 'Media Autounattend.xml is missing required Setup/OOBE/ProductKey configuration.'
    }

    Write-Host 'ISO structure validation: PASS'
    Write-Host 'Installer bridge validation: PASS'
    Write-Host 'Embedded WinPE answer-file validation: PASS'
    Write-Host 'ANON payload validation: PASS'
    Write-Host 'Performance Pet validation: PASS'
    Write-Host 'Panther fallback validation: PASS'
    Write-Host 'Autounattend validation: PASS'
    Write-Host "Image index: $ExpectedImageIndex"
    Write-Host "SHA256: $hash"
    Write-Host "Architecture: $ExpectedArchitecture"
} finally {
    if($bootMounted){&dism.exe /Unmount-Wim /MountDir:$bootMount /Discard 2>&1|Write-Host}
    if($installMounted){&dism.exe /Unmount-Wim /MountDir:$installMount /Discard 2>&1|Write-Host}
    if($bootMount -and(Test-Path $bootMount)){Remove-Item $bootMount -Recurse -Force -ErrorAction SilentlyContinue}
    if($installMount -and(Test-Path $installMount)){Remove-Item $installMount -Recurse -Force -ErrorAction SilentlyContinue}
    if($disk){Dismount-DiskImage -ImagePath $iso -ErrorAction SilentlyContinue}
}
Write-Host 'VM validation remains mandatory: clean install, reboot, OOBE completion, first logon and ANON shell startup.'
