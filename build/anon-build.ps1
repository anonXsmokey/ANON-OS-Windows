[CmdletBinding()]
param(
    [ValidateSet('Inventory','Build','Test')][string]$Mode = 'Inventory',
    [string]$WindowsIso,
    [string]$OutputRoot = (Join-Path $PSScriptRoot 'out'),
    [ValidateSet('x64','ARM64')][string]$Architecture = 'x64',
    [switch]$SkipShellPublish
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $PSScriptRoot 'anon-build.json'
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) { throw "Required command '$Name' was not found." }
}

function Get-ToolStatus([string]$Name) {
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    [pscustomobject]@{ Name = $Name; Available = $null -ne $command; Path = if ($command) { $command.Source } else { $null } }
}

function Get-Sha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash }

$tools = @('dotnet','dism.exe','oscdimg.exe','MakeWinPEMedia.cmd') | ForEach-Object { Get-ToolStatus $_ }
$inventory = [ordered]@{
    GeneratedUtc = [DateTime]::UtcNow.ToString('o')
    Repository = $manifest.repository
    Architecture = $Architecture
    WindowsIso = if ($WindowsIso) { (Resolve-Path $WindowsIso).Path } else { $null }
    Tools = $tools
    WindowsIsoSha256 = if ($WindowsIso -and (Test-Path $WindowsIso)) { Get-Sha256 $WindowsIso } else { $null }
    ShellProject = Join-Path $repoRoot $manifest.shellProject
}

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
$inventoryPath = Join-Path $OutputRoot 'BUILD-INVENTORY.json'
$inventory | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $inventoryPath -Encoding UTF8

if ($Mode -eq 'Inventory') {
    Write-Host "ANON build inventory written to $inventoryPath"
    $tools | Format-Table -AutoSize
    exit 0
}

Require-Command 'dotnet'
Require-Command 'dism.exe'
Require-Command 'oscdimg.exe'
if (-not $WindowsIso -or -not (Test-Path $WindowsIso)) { throw 'Build mode requires a Windows installation ISO via -WindowsIso. The repository intentionally does not contain Microsoft installation media.' }

$shellProject = Join-Path $repoRoot $manifest.shellProject
if (-not (Test-Path $shellProject)) { throw "Shell project not found: $shellProject" }

$publishDir = Join-Path $OutputRoot "shell-$Architecture"
if (-not $SkipShellPublish) {
    dotnet publish $shellProject -c Release -p:Platform=$Architecture -r (if ($Architecture -eq 'x64') { 'win-x64' } else { 'win-arm64' }) --self-contained false -o $publishDir
}

if ($Mode -eq 'Build') {
    throw 'ISO assembly stage is intentionally gated: mount/service/configure/recovery manifests must be present before producing release media.'
}

if ($Mode -eq 'Test') {
    throw 'Automated VM validation is not configured yet; no release ISO is emitted until that gate exists.'
}
