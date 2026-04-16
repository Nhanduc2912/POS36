@echo off

echo.
echo ============================================================
echo   POS36 - Dung He Thong
echo ============================================================
echo.

echo Dung Backend (dotnet)...
taskkill /FI "WindowTitle eq POS36 Backend*" /F >nul 2>&1
taskkill /IM dotnet.exe /F >nul 2>&1

echo Dung Frontend (node)...
taskkill /FI "WindowTitle eq POS36 Frontend*" /F >nul 2>&1
taskkill /IM node.exe /F >nul 2>&1

echo.
echo [OK] He thong da dung!
timeout /t 2 /nobreak >nul
