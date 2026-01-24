@echo off
cd /d C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam
echo Creating migration for HargaPerKg...
dotnet ef migrations add AddHargaPerKgToHargaPasar
echo.
echo Building project...
dotnet build
echo.
echo Applying database migrations...
dotnet ef database update
echo.
echo Done!
pause
