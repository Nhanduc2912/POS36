@echo off

echo.
echo ============================================================
echo   POS36 - Build Production
echo ============================================================
echo.

echo [1/2] Build Backend (dotnet publish)...
cd /d "%~dp0..\POS36.Api"
dotnet publish -c Release -o "%~dp0..\publish\backend"
if %errorLevel% neq 0 (
    echo [ERROR] Loi khi build Backend!
    pause
    exit /b 1
)
echo [OK] Backend build thanh cong

echo.
echo [2/2] Build Frontend (npm run build)...
cd /d "%~dp0..\POS36.Web"
call npm run build
if %errorLevel% neq 0 (
    echo [ERROR] Loi khi build Frontend!
    pause
    exit /b 1
)
xcopy /E /I /Y dist "%~dp0..\publish\frontend"
echo [OK] Frontend build thanh cong

echo.
echo ============================================================
echo   Build hoan tat!
echo   Output: publish\backend  va  publish\frontend
echo ============================================================
pause
