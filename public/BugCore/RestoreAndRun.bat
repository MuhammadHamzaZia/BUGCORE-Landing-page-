@echo off
echo ========================================================
echo   BugCore Automatic Restore, Build & Run Helper
echo ========================================================
echo.
echo Restoring NuGet packages...
dotnet restore BugCore.sln
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] NuGet restore failed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Building BugCore solution...
dotnet build BugCore.sln -c Debug
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Launching BugCore Web Server...
cd src\BugCore.Web
dotnet run --launch-profile http
pause
