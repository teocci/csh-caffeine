@echo off
REM ============================================================================
REM Caffeine - Single-File Release Build Script
REM ============================================================================
REM This script creates a single self-contained executable that includes
REM the .NET runtime and all dependencies in one file.
REM ============================================================================

setlocal enabledelayedexpansion

set PROJECT_DIR=%~dp0caffeine
set OUTPUT_DIR=%~dp0publish
set CONFIGURATION=Release
set RUNTIME=win-x64

echo.
echo ============================================
echo   Caffeine - Building Single-File Release
echo ============================================
echo.

REM Clean previous publish output
if exist "%OUTPUT_DIR%" (
    echo Cleaning previous build...
    rmdir /s /q "%OUTPUT_DIR%"
)

echo Building %CONFIGURATION% for %RUNTIME%...
echo.

REM PublishSingleFile=true triggers all other single-file settings from csproj
dotnet publish "%PROJECT_DIR%\caffeine.csproj" ^
    -c %CONFIGURATION% ^
    -r %RUNTIME% ^
    -p:PublishSingleFile=true ^
    -p:DebugType=none ^
    -o "%OUTPUT_DIR%"

if %ERRORLEVEL% neq 0 (
    echo.
    echo ============================================
    echo   BUILD FAILED
    echo ============================================
    exit /b %ERRORLEVEL%
)

echo.
echo ============================================
echo   BUILD SUCCESSFUL
echo ============================================
echo.
echo Output: %OUTPUT_DIR%\caffeine.exe
echo.

REM Show file size
for %%A in ("%OUTPUT_DIR%\caffeine.exe") do (
    set SIZE=%%~zA
    set /a SIZE_MB=!SIZE! / 1048576
    echo Size: !SIZE_MB! MB ^(!SIZE! bytes^)
)

echo.
echo Done!
pause
