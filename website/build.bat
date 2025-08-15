@echo off
REM AgileCloud.ai Website Production Build Script for Windows
REM Prepares the website for deployment with optimization

setlocal enabledelayedexpansion

echo 🚀 Starting AgileCloud.ai website production build...

REM Change to website directory
cd /d "%~dp0"

REM Clean previous build
echo [INFO] Cleaning previous build...
if exist dist rmdir /s /q dist
mkdir dist

REM Copy public files
echo [INFO] Copying public files...
xcopy public\* dist\ /s /e /i /y >nul

REM Copy assets
echo [INFO] Copying assets...
xcopy assets dist\assets\ /s /e /i /y >nul

REM Verify all solution pages have JavaScript includes
echo [INFO] Verifying solution pages...
set solution_pages=enterprise-ai.html data-ingestion.html rapid-prototyping.html global-vector-db.html intelligent-automation.html scalable-architecture.html

for %%p in (%solution_pages%) do (
    if exist "dist\solutions\%%p" (
        findstr /c:"main.js" "dist\solutions\%%p" >nul
        if !errorlevel! equ 0 (
            echo [SUCCESS] ✓ JavaScript verified in %%p
        ) else (
            echo [ERROR] ✗ Missing JavaScript in %%p
            exit /b 1
        )
    ) else (
        echo [WARNING] Solution page not found: %%p
    )
)

REM Check for Node.js (optional optimization)
where npm >nul 2>&1
if %errorlevel% equ 0 (
    echo [INFO] Node.js detected, checking for optimizations...
    
    REM Install dependencies if needed
    if not exist node_modules (
        echo [INFO] Installing dependencies...
        npm install
    )
    
    REM Check if optimization tools are available
    npx csso --version >nul 2>&1
    if %errorlevel% equ 0 (
        echo [INFO] Minifying CSS...
        npx csso assets\css\styles.css --output dist\assets\css\styles.min.css
        
        REM Update HTML files to use minified CSS (basic replacement)
        powershell -Command "Get-ChildItem dist -Filter *.html -Recurse | ForEach-Object { (Get-Content $_.FullName) -replace 'assets/css/styles\.css', 'assets/css/styles.min.css' | Set-Content $_.FullName }"
        echo [SUCCESS] CSS minified
    ) else (
        echo [WARNING] csso not available, skipping CSS minification
    )
    
    npx terser --version >nul 2>&1
    if %errorlevel% equ 0 (
        echo [INFO] Minifying JavaScript...
        npx terser assets\js\main.js --output dist\assets\js\main.min.js --compress --mangle
        
        REM Update HTML files to use minified JS
        powershell -Command "Get-ChildItem dist -Filter *.html -Recurse | ForEach-Object { (Get-Content $_.FullName) -replace 'assets/js/main\.js', 'assets/js/main.min.js' | Set-Content $_.FullName }"
        echo [SUCCESS] JavaScript minified
    ) else (
        echo [WARNING] terser not available, skipping JavaScript minification
    )
    
    npx html-minifier-terser --version >nul 2>&1
    if %errorlevel% equ 0 (
        echo [INFO] Minifying HTML...
        for /r dist %%f in (*.html) do (
            npx html-minifier-terser "%%f" --output "%%f" --remove-comments --collapse-whitespace --minify-css --minify-js
        )
        echo [SUCCESS] HTML minified
    ) else (
        echo [WARNING] html-minifier-terser not available, skipping HTML minification
    )
) else (
    echo [WARNING] Node.js not detected, skipping optimization steps
)

REM Validate build
echo [INFO] Validating build...

REM Check required files
set required_files=index.html 404.html robots.txt sitemap.xml web.config assets\css\styles.css assets\js\main.js

for %%f in (%required_files%) do (
    if exist "dist\%%f" (
        echo [SUCCESS] ✓ %%f
    ) else (
        echo [ERROR] ✗ Missing required file: %%f
        exit /b 1
    )
)

REM Check solution pages
for %%p in (%solution_pages%) do (
    if exist "dist\solutions\%%p" (
        echo [SUCCESS] ✓ %%p
    ) else (
        echo [ERROR] ✗ Missing solution page: %%p
        exit /b 1
    )
)

REM Generate build report
echo [INFO] Generating build report...
echo AgileCloud.ai Website Build Report > dist\build-report.txt
echo Generated: %date% %time% >> dist\build-report.txt
echo. >> dist\build-report.txt

REM Count files
for /f %%i in ('dir dist /s /b /a-d ^| find /c /v ""') do set file_count=%%i
echo Files in build: !file_count! total files >> dist\build-report.txt
echo. >> dist\build-report.txt

echo HTML Pages: >> dist\build-report.txt
dir dist\*.html /s /b >> dist\build-report.txt
echo. >> dist\build-report.txt

echo Assets: >> dist\build-report.txt
dir dist\assets\*.* /s /b >> dist\build-report.txt

echo [SUCCESS] Build report generated: dist\build-report.txt

REM Final validation
echo [INFO] Running final validation...

REM Check HTML structure using PowerShell
powershell -Command "Get-ChildItem dist -Filter *.html -Recurse | ForEach-Object { if (-not (Select-String -Path $_.FullName -Pattern '<!DOCTYPE html>' -Quiet)) { Write-Host '[ERROR] Missing DOCTYPE in' $_.Name; exit 1 } }"
if %errorlevel% neq 0 exit /b 1

powershell -Command "Get-ChildItem dist -Filter *.html -Recurse | ForEach-Object { if (-not (Select-String -Path $_.FullName -Pattern '</html>' -Quiet)) { Write-Host '[ERROR] Missing closing HTML tag in' $_.Name; exit 1 } }"
if %errorlevel% neq 0 exit /b 1

echo [SUCCESS] All HTML files have proper structure

REM Check meta tags
powershell -Command "Get-ChildItem dist -Filter *.html -Recurse | ForEach-Object { if (-not (Select-String -Path $_.FullName -Pattern 'name=\"description\"' -Quiet)) { Write-Host '[WARNING] Missing meta description in' $_.Name } }"

powershell -Command "Get-ChildItem dist -Filter *.html -Recurse | ForEach-Object { if (-not (Select-String -Path $_.FullName -Pattern 'rel=\"canonical\"' -Quiet)) { Write-Host '[WARNING] Missing canonical URL in' $_.Name } }"

echo [SUCCESS] 🎉 Build completed successfully!
echo [SUCCESS] 📁 Build output: dist\
echo [SUCCESS] 📊 Build report: dist\build-report.txt
echo.
echo 🚀 DEPLOYMENT READY
echo.
echo Your website is ready for deployment. The optimized files are in the 'dist\' directory.
echo.
echo Next steps:
echo 1. Commit your changes to git
echo 2. Push to main/master branch to trigger automatic deployment
echo 3. Monitor the GitHub Actions workflow
echo 4. Visit https://agilecloud.ai once deployment completes
echo.
echo Manual deployment options:
echo - Azure: Use the GitHub Actions workflow (automatic)
echo - GitHub Pages: Copy dist\ contents to gh-pages branch
echo - Other hosts: Upload dist\ contents to your web server
echo.
echo Build completed at: %date% %time%

pause
