@echo off
cd /d C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam
echo Cleaning project...
dotnet clean
echo.
echo Rebuilding project...
dotnet build
echo.
if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo.
    echo Creating migration for HargaPerKg...
    dotnet ef migrations add AddHargaPerKgToHargaPasar
    echo.
    echo Applying database migrations...
    dotnet ef database update
) else (
    echo Build failed!
)
echo.
pause
