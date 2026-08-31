[CmdletBinding()]
param([string]$OutputPath=(Join-Path $PSScriptRoot 'tools\oscdimg.exe'))
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$existing=Get-Command oscdimg.exe -ErrorAction SilentlyContinue
if($existing){Write-Host "oscdimg.exe already available: $($existing.Source)";exit 0}
$kits=@(
  "$env:ProgramFiles(x86)\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe",
  "$env:ProgramFiles\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg\oscdimg.exe"
)
$found=$kits|Where-Object{Test-Path $_}|Select-Object -First 1
if($found){Write-Host "Found Windows ADK oscdimg.exe: $found";Write-Host "Copy it to $OutputPath or add its directory to PATH.";exit 0}
Write-Error "oscdimg.exe was not found. Install the Microsoft Windows ADK Deployment Tools, then rerun this script."
exit 1
