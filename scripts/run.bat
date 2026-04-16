@echo off
chcp 65001 >nul

echo.
echo ============================================================
echo   POS36 - Khoi Dong He Thong
echo ============================================================
echo.

echo [1/2] Khoi dong Backend API (cua so moi se mo ra)...
start "POS36 Backend" cmd /k "cd /d %~dp0..\POS36.Api && dotnet run"

echo     Cho Backend san sang (8 giay)...
timeout /t 8 /nobreak >nul

echo [2/2] Khoi dong Frontend (cua so moi se mo ra)...
start "POS36 Frontend" cmd /k "cd /d %~dp0..\POS36.Web && npm run dev"

timeout /t 3 /nobreak >nul
echo.
echo ============================================================
echo   HE THONG DANG CHAY:
echo.
echo   Backend API  :  http://localhost:5098
echo   Swagger UI   :  http://localhost:5098/swagger
echo   Frontend     :  http://localhost:5173
echo.
echo   De dung he thong: chay scripts\stop.bat
echo ============================================================
echo.
echo Nhan phim bat ky de dong cua so nay...
pause >nul
