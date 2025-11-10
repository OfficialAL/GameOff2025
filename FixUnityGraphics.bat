@echo off
echo Fixing Unity Graphics Issues...

REM Kill Unity processes
taskkill /f /im Unity.exe >nul 2>&1
taskkill /f /im UnityHub.exe >nul 2>&1

REM Clear Unity cache
rmdir /s /q "%LOCALAPPDATA%\Unity\Editor" >nul 2>&1
rmdir /s /q "Library\ShaderCache" >nul 2>&1
rmdir /s /q "Temp" >nul 2>&1

echo Cache cleared...

REM Launch Unity with safe graphics settings
echo Launching Unity with safe graphics...
start "" "C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe" -projectPath "%cd%" -force-d3d11 -force-low-power-device

echo Unity should launch with stable graphics now.
pause