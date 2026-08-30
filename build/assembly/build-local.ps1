[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$WindowsIso,
 [int]$ImageIndex=1,
 [string]$OutputRoot=(Join-Path $PSScriptRoot '..\out')
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
if(-not(Test-Path -LiteralPath $WindowsIso)){throw "Windows ISO not found: $WindowsIso"}
$root=(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$publish=Join-Path $root 'build\shell-publish'
New-Item -ItemType Directory -Force -Path $publish|Out-Null
Push-Location $root
try{
 dotnet restore ux/shell/M0/ANON.Shell.M0.csproj --runtime win-x64
 if($LASTEXITCODE){throw 'Shell restore failed.'}
 dotnet publish ux/shell/M0/ANON.Shell.M0.csproj -c Release -r win-x64 --self-contained true --no-restore -o $publish
 if($LASTEXITCODE){throw 'Shell publish failed.'}
 if(-not(Test-Path (Join-Path $publish 'ANON.Shell.M0.exe'))){throw 'ANON.Shell.M0.exe missing after publish.'}
 & (Join-Path $root 'build\assembly\build-iso.ps1') -WindowsIso (Resolve-Path $WindowsIso).Path -ShellPublish $publish -OutputRoot $OutputRoot -Architecture x64 -ImageIndex $ImageIndex
 if($LASTEXITCODE){throw 'ISO assembly failed.'}
 $iso=Get-ChildItem $OutputRoot -Filter 'ANON-OS-Windows-*.iso'|Sort-Object LastWriteTime -Descending|Select-Object -First 1
 if(-not$iso){throw 'No ANON OS ISO was produced.'}
 & (Join-Path $root 'build\assembly\validate-iso.ps1') -IsoPath $iso.FullName -ExpectedArchitecture x64
 & (Join-Path $root 'build\assembly\release-manifest.ps1') -IsoPath $iso.FullName
 Write-Host "ANON OS ISO candidate: $($iso.FullName)"
}finally{Pop-Location}
