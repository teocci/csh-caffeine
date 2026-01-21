# ============================================================================
# Caffeine - Single-File Release Build Script (PowerShell)
# ============================================================================
# This script creates a single self-contained executable that includes
# the .NET runtime and all dependencies in one file.
# ============================================================================

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$NoCompress,
    [switch]$IncludePdb
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "caffeine"
$OutputDir = Join-Path $ScriptDir "publish"
$ProjectFile = Join-Path $ProjectDir "caffeine.csproj"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Caffeine - Building Single-File Release" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration: $Configuration"
Write-Host "Runtime:       $Runtime"
Write-Host "Compression:   $(-not $NoCompress)"
Write-Host ""

# Clean previous publish output
if (Test-Path $OutputDir) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}

# Build arguments - PublishSingleFile=true triggers all other single-file settings from csproj
$publishArgs = @(
    "publish"
    $ProjectFile
    "-c", $Configuration
    "-r", $Runtime
    "-p:PublishSingleFile=true"
    "-o", $OutputDir
)

if ($NoCompress) {
    $publishArgs += "-p:EnableCompressionInSingleFile=false"
}

if (-not $IncludePdb) {
    $publishArgs += "-p:DebugType=none"
}

Write-Host "Building..." -ForegroundColor Yellow
Write-Host ""

# Execute build
& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Red
    Write-Host "  BUILD FAILED" -ForegroundColor Red
    Write-Host "============================================" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  BUILD SUCCESSFUL" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Show output info
$exePath = Join-Path $OutputDir "caffeine.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)

    Write-Host "Output: $exePath" -ForegroundColor White
    Write-Host "Size:   $sizeMB MB ($($fileInfo.Length) bytes)" -ForegroundColor White
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
