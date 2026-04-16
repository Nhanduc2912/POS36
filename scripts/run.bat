@echo off
chcp 65001 >nul
echo ╔════════════════════════════════════════════════════════════╗
echo ║         POS36 - Khởi Động Hệ Thống                        ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

echo [1/2] Khởi động Backend API...
start "POS36 Backend" cmd /k "cd /d %~dp0..\POS36.Api && dotnet run > ..\logs\backend.log 2>&1"
timeout /t 5 /nobreak >nul

echo [2/2] Khởi động Frontend...
start "POS36 Frontend" cmd /k "cd /d %~dp0..\POS36.Web && npm run dev > ..\logs\frontend.log 2>&1"

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  Hệ thống đang khởi động...                                ║
echo ║                                                            ║
echo ║  Backend API:  http://localhost:5098                       ║
echo ║  Swagger UI:   http://localhost:5098/swagger               ║
echo ║  Frontend:     http://localhost:5173                       ║
echo ║                                                            ║
echo ║  Nhấn Ctrl+C trong cửa sổ Backend/Frontend để dừng        ║
echo ║  Hoặc chạy stop.bat để dừng tất cả                         ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
pause
