@echo off
chcp 65001 >nul

echo.
echo ============================================================
echo   POS36 - Auto Setup (Cai Dat Tu Dong)
echo ============================================================
echo.

REM Kiem tra quyen Administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Phai chay voi quyen Administrator!
    echo         Chuot phai vao file nay - "Run as Administrator"
    echo.
    pause
    exit /b 1
)

echo [1/5] Kiem tra .NET 9.0 SDK...
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Chua cai dat .NET 9.0 SDK!
    echo         Tai tai: https://dotnet.microsoft.com/download/dotnet/9.0
    echo.
    pause
    exit /b 1
)
for /f "tokens=*" %%v in ('dotnet --version') do echo [OK] .NET SDK: %%v

echo.
echo [2/5] Kiem tra Node.js...
node --version >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Chua cai dat Node.js 18+!
    echo         Tai tai: https://nodejs.org/
    echo.
    pause
    exit /b 1
)
for /f "tokens=*" %%v in ('node --version') do echo [OK] Node.js: %%v

echo.
echo [3/5] Cai dat Backend dependencies (dotnet restore)...
cd /d "%~dp0..\POS36.Api"
if not exist "POS36.Api.csproj" (
    echo [ERROR] Khong tim thay POS36.Api.csproj!
    echo         Duong dan: %CD%
    pause
    exit /b 1
)
dotnet restore
if %errorLevel% neq 0 (
    echo [ERROR] Loi khi chay dotnet restore!
    pause
    exit /b 1
)
echo [OK] Backend dependencies da cai dat xong

echo.
echo [4/5] Cai dat Frontend dependencies (npm install)...
cd /d "%~dp0..\POS36.Web"
if not exist "package.json" (
    echo [ERROR] Khong tim thay package.json!
    echo         Duong dan: %CD%
    pause
    exit /b 1
)
call npm install
if %errorLevel% neq 0 (
    echo [ERROR] Loi khi chay npm install!
    pause
    exit /b 1
)
echo [OK] Frontend dependencies da cai dat xong

echo.
echo [5/5] Kiem tra file .env cho Frontend...
set "ENV_FILE=%~dp0..\POS36.Web\.env"
if not exist "%ENV_FILE%" (
    echo     Tao file .env cho Frontend...
    (
        echo VITE_API_URL=http://localhost:5098/api
        echo VITE_SIGNALR_URL=http://localhost:5098/kitchenHub
        echo VITE_EMAILJS_SERVICE_ID=your_service_id
        echo VITE_EMAILJS_TEMPLATE_ID=your_template_id
        echo VITE_EMAILJS_PUBLIC_KEY=your_public_key
    ) > "%ENV_FILE%"
    echo [OK] Da tao POS36.Web\.env
    echo [!] Sua lai EmailJS config trong POS36.Web\.env neu can
) else (
    echo [OK] File .env da ton tai
)

echo.
echo ============================================================
echo   CAI DAT HOAN TAT!
echo.
echo   BUOC TIEP THEO (QUAN TRONG):
echo   1. Cau hinh SQL Server trong:
echo      POS36.Api\appsettings.json
echo      (sua ConnectionStrings.DefaultConnection)
echo.
echo   2. Import database:
echo      Chay file Pos36DB.sql vao SQL Server
echo.
echo   3. Chay he thong:
echo      Double-click: scripts\run.bat
echo ============================================================
echo.
pause
