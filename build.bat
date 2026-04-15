@echo off
chcp 65001 >nul
echo ╔════════════════════════════════════════════════════════════╗
echo ║         POS36 - Build Production                           ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

echo [1/2] Build Backend...
cd POS36.Api
dotnet publish -c Release -o ..\publish\backend
if %errorLevel% neq 0 (
    echo [ERROR] Lỗi khi build Backend!
    pause
    exit /b 1
)
echo [OK] Backend build thành công

echo.
echo [2/2] Build Frontend...
cd ..\POS36.Web
call npm run build
if %errorLevel% neq 0 (
    echo [ERROR] Lỗi khi build Frontend!
    pause
    exit /b 1
)
xcopy /E /I /Y dist ..\publish\frontend
echo [OK] Frontend build thành công

cd ..
echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  Build hoàn tất!                                           ║
echo ║  Files nằm trong thư mục: publish\                         ║
echo ╚════════════════════════════════════════════════════════════╝
pause
