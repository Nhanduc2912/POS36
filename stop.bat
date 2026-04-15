@echo off
chcp 65001 >nul
echo ╔════════════════════════════════════════════════════════════╗
echo ║         POS36 - Dừng Hệ Thống                             ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

echo Đang dừng Backend...
taskkill /FI "WindowTitle eq POS36 Backend*" /F >nul 2>&1

echo Đang dừng Frontend...
taskkill /FI "WindowTitle eq POS36 Frontend*" /F >nul 2>&1

echo Đang dừng các process Node.js và dotnet...
taskkill /IM node.exe /F >nul 2>&1
taskkill /IM dotnet.exe /F >nul 2>&1

echo.
echo [OK] Hệ thống đã dừng!
timeout /t 2 /nobreak >nul
