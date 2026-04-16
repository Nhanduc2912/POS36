@echo off
chcp 65001 >nul
echo ╔════════════════════════════════════════════════════════════╗
echo ║         POS36 - Cài Đặt Tự Động                           ║
echo ║         Hệ Thống Quản Lý Bán Hàng F&B                      ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

REM Kiểm tra quyền Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Vui lòng chạy file này với quyền Administrator!
    pause
    exit /b 1
)

echo [1/6] Kiểm tra .NET SDK...
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Chưa cài đặt .NET 9.0 SDK!
    echo Vui lòng tải tại: https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)
echo [OK] .NET SDK đã cài đặt

echo.
echo [2/6] Kiểm tra Node.js...
node --version >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Chưa cài đặt Node.js!
    echo Vui lòng tải tại: https://nodejs.org/
    pause
    exit /b 1
)
echo [OK] Node.js đã cài đặt

echo.
echo [3/6] Kiểm tra SQL Server...
sqlcmd -? >nul 2>&1
if %errorLevel% neq 0 (
    echo [WARNING] Chưa cài đặt SQL Server hoặc sqlcmd!
    echo Bạn có thể:
    echo   1. Cài SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
    echo   2. Hoặc sử dụng Docker: docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pos36_Secret_Password_123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
    echo.
    set /p continue="Tiếp tục cài đặt? (y/n): "
    if /i not "%continue%"=="y" exit /b 1
) else (
    echo [OK] SQL Server đã cài đặt
)

echo.
echo [4/6] Cài đặt Backend Dependencies...
cd /d "%~dp0..\POS36.Api"
dotnet restore
if %errorLevel% neq 0 (
    echo [ERROR] Lỗi khi cài đặt Backend dependencies!
    pause
    exit /b 1
)
echo [OK] Backend dependencies đã cài đặt

echo.
echo [5/6] Cài đặt Frontend Dependencies...
cd /d "%~dp0..\POS36.Web"
call npm install
if %errorLevel% neq 0 (
    echo [ERROR] Lỗi khi cài đặt Frontend dependencies!
    pause
    exit /b 1
)
echo [OK] Frontend dependencies đã cài đặt

echo.
echo [6/6] Tạo Database...
cd /d "%~dp0.."
echo Vui lòng cấu hình Connection String trong POS36.Api\appsettings.json
echo Sau đó chạy: dotnet ef database update -p POS36.Api
echo.

echo ╔════════════════════════════════════════════════════════════╗
echo ║  Cài đặt hoàn tất!                                         ║
echo ║  Chạy file run.bat để khởi động hệ thống                   ║
echo ╚════════════════════════════════════════════════════════════╝
pause
