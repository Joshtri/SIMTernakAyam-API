@echo off
echo ============================================
echo  Fixing HargaPasar Build Errors
echo ============================================
echo.
cd /d C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam

echo Step 1: Cleaning project...
dotnet clean
echo.

echo Step 2: Building project...
dotnet build --no-incremental
echo.

if %ERRORLEVEL% EQU 0 (
    echo ============================================
    echo  Build SUCCESS!
    echo ============================================
    echo.
    echo Step 3: Creating migration...
    dotnet ef migrations add AddHargaPerKgToHargaPasar
    echo.
    echo Step 4: Updating database...
    dotnet ef database update
    echo.
    echo ============================================
    echo  ALL DONE!
    echo ============================================
) else (
    echo ============================================
    echo  Build FAILED - Please check errors above
    echo ============================================
)

echo.
pause
