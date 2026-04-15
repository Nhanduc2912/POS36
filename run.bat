@echo off
chcp 65001 >nul
echo ╔════════════════════════════════════════════════════════════╗
echo ║         POS36 - Khởi Động Hệ Thống                        ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

REM Tạo thư mục logs nếu chưa có
if not exist "logs" mkdir logs

echo [1/2] Khởi động Backend API...
start "POS36 Backend" cmd /k "cd POS36.Api && dotnet run > ..\logs\backend.log 2>&1"
timeout /t 5 /nobreak >nul

echo [2/2] Khởi động Frontend...
start "POS36 Frontend" cmd /k "cd POS36.Web && npm run dev > ..\logs\frontend.log 2>&1"

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║  Hệ thống đang khởi động...                                ║
echo ║                                                            ║
echo ║  Backend API:  http://localhost:5198                       ║
echo ║  Swagger UI:   http://localhost:5198/swagger               ║
echo ║  Frontend:     http://localhost:5173                       ║
echo ║                                                            ║
echo ║  Đóng cửa sổ này để dừng hệ thống                          ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo Nhấn Ctrl+C để dừng...
pause >nul
