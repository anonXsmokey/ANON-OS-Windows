[CmdletBinding()]
param(
    [ValidateSet('Inventory','Build','Test')][string]$Mode='Inventory',
    [string]$WindowsIso,
    [string]$OutputRoot=(Join-Path $PSScriptRoot 'out'),
    [ValidateSet('x64','ARM64')][string]$Architecture='x64',
    [switch]$SkipShellPublish
)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$repoRoot=Split-Path -Parent $PSScriptRoot
$manifest=Get-Content (Join-Path $PSScriptRoot 'anon-build.json') -Raw|ConvertFrom-Json
function Require-Command([string]$Name){if(-not(Get-Command $Name -ErrorAction SilentlyContinue)){throw "Required command '$Name' was not found."}}
function Get-ToolStatus([string]$Name){$c=Get-Command $Name -ErrorAction SilentlyContinue;[pscustomobject]@{Name=$Name;Available=$null-ne$c;Path=if($c){$c.Source}else{$null}}}
function Get-Sha256([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash}
$tools=@('dotnet','dism.exe','oscdimg.exe','Mount-DiskImage')|%{Get-ToolStatus $_}
$resolvedIso=$null;if($WindowsIso -and(Test-Path -LiteralPath $WindowsIso)){$resolvedIso=(Resolve-Path $WindowsIso).Path}
New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null
$inventory=[ordered]@{GeneratedUtc=[DateTime]::UtcNow.ToString('o');Repository=$manifest.repository;Architecture=$Architecture;WindowsIso=$resolvedIso;WindowsIsoSha256=if($resolvedIso){Get-Sha256 $resolvedIso}else{$null};Tools=$tools;ShellProject=Join-Path $repoRoot $manifest.shellProject}
$inventory|ConvertTo-Json -Depth 8|Set-Content (Join-Path $OutputRoot 'BUILD-INVENTORY.json') -Encoding UTF8
if($Mode-eq'Inventory'){$tools|Format-Table -AutoSize;exit 0}
Require-Command dotnet;Require-Command dism.exe
if(-not$resolvedIso){throw 'Build mode requires -WindowsIso pointing to the original Windows installation ISO.'}
$local=Join-Path $PSScriptRoot 'assembly\build-local.ps1';if(-not(Test-Path $local)){throw "Assembly build script missing: $local"}
if($Mode-eq'Build'){
  & $local -WindowsIso $resolvedIso -OutputRoot $OutputRoot -ImageIndex 6
  if($LASTEXITCODE){throw "ANON ISO build failed: $LASTEXITCODE"}
  exit 0
}
if($Mode-eq'Test'){
  $isos=@(Get-ChildItem $OutputRoot -Filter 'ANON-OS-Windows-*.iso'|Sort-Object LastWriteTime -Descending)
  if($isos.Count-eq 0){throw 'No ANON ISO found to validate.'}
  & (Join-Path $PSScriptRoot 'assembly\validate-iso.ps1') -IsoPath $isos[0].FullName -ExpectedArchitecture x64 -ExpectedImageIndex 6
  if($LASTEXITCODE){throw "ISO validation failed: $LASTEXITCODE"}
  exit 0
}
