@echo off
setlocal

:: Auto-detect KSP install
set "KSP="
if exist "C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program" set "KSP=C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program"
if exist "C:\Program Files\Epic Games\KerbalSpaceProgram\English" set "KSP=C:\Program Files\Epic Games\KerbalSpaceProgram\English"
if exist "D:\Steam\steamapps\common\Kerbal Space Program" set "KSP=D:\Steam\steamapps\common\Kerbal Space Program"
if exist "D:\Games\Kerbal Space Program" set "KSP=D:\Games\Kerbal Space Program"

if "%KSP%"=="" (
    echo KSP not found. Drag this folder into your KSP directory and run install.bat manually.
    set /p "KSP=Enter KSP install path: "
)

if not exist "%KSP%\GameData\KspOptimizer" mkdir "%KSP%\GameData\KspOptimizer"
copy /Y "%~dp0GameData\KspOptimizer\KspOptimizer.dll" "%KSP%\GameData\KspOptimizer\" >nul
copy /Y "%~dp0GameData\KspOptimizer\0Harmony.dll" "%KSP%\GameData\KspOptimizer\" >nul
copy /Y "%~dp0GameData\KspOptimizer\KSP-OPTIMIZER.version" "%KSP%\GameData\KspOptimizer\" >nul

echo.
echo KSP Optimizer installed to: %KSP%
echo Launch KSP and click the green toolbar button in flight view.
pause
