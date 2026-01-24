@echo off
echo ============================================
echo  Adding HargaPerKg Column Back to Database
echo ============================================
echo.
cd /d C:\Users\LENOVO\source\repos\SIMTernakAyam\SIMTernakAyam

echo Step 1: Building project...
dotnet build
echo.

if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo.
    echo Step 2: Applying migration AddBackHargaPerKgColumn...
    dotnet ef database update
    echo.
    
    if %ERRORLEVEL% EQU 0 (
        echo ============================================
        echo  SUCCESS! HargaPerKg column added to database
        echo ============================================
        echo.
        echo Database now has both columns:
        echo   - HargaPerEkor
        echo   - HargaPerKg
    ) else (
        echo ============================================
        echo  Migration FAILED!
        echo ============================================
    )
) else (
    echo ============================================
    echo  Build FAILED! Fix errors first.
    echo ============================================
)

echo.
pause
