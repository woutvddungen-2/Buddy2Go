@echo off
setlocal enabledelayedexpansion

echo ===============================================
echo   Buddy2Go Build & Deployment Package Builder
echo ===============================================

REM ----------------- Read CLI flags ------------------
set MODE=deploy
if "%1"=="--local" set MODE=local
if "%1"=="--deploy" set MODE=deploy

echo Selected mode: %MODE%
echo.

REM ----------------- Version Selection ------------------
if "%MODE%"=="local" (
    set VERSION_FILE=version-local.txt
) else (
    set VERSION_FILE=version-deploy.txt
)

if not exist "%VERSION_FILE%" (
    echo V0.1.0 > "%VERSION_FILE%"
)

set /p CURRENT_VERSION=<"%VERSION_FILE%"
echo Current version (%MODE%): %CURRENT_VERSION%

for /f "tokens=1-3 delims=." %%a in ("%CURRENT_VERSION:V=%") do (
    set MAJOR=%%a
    set MINOR=%%b
    set PATCH=%%c
)

set /a PATCH+=1
set NEW_VERSION=V%MAJOR%.%MINOR%.%PATCH%
echo New version: %NEW_VERSION%

echo %NEW_VERSION% > "%VERSION_FILE%"
echo.


REM ----------------- AppSettings Selection ------------------
if "%MODE%"=="local" (
    echo Using LOCAL appsettings.json...
    copy /Y Client\wwwroot\appsettings.json Client\wwwroot\build.appsettings.json >nul
) else (
    echo Using DEPLOYMENT docker.appsettings.json...
    copy /Y Client\wwwroot\docker.appsettings.json Client\wwwroot\build.appsettings.json >nul
)
echo.


REM ---------------- CONFIG ----------------
set BUILD_OUTPUT_DIR=C:\Users\admin\source\Builds\Docker
set TARGET_DIR=%BUILD_OUTPUT_DIR%\%MODE%

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

set DEPLOY_FOLDER=Buddy2Go_%MODE%_%NEW_VERSION%
set TAR_NAME=Buddy2Go_%MODE%_%NEW_VERSION%.tar.gz
set OUTPUT_PACKAGE=%TARGET_DIR%\%TAR_NAME%

set IMG_SERVER_VER=buddy2go-server:%NEW_VERSION%
set IMG_CLIENT_VER=buddy2go-client:%NEW_VERSION%
set IMG_SERVER_LATEST=buddy2go-server:latest
set IMG_CLIENT_LATEST=buddy2go-client:latest
set IMG_MYSQL=mysql:8.0


REM =================================================
echo Building server image...
docker build -t %IMG_SERVER_LATEST% -f Server/Dockerfile .
if errorlevel 1 exit /b 1

echo Building client image...
docker build -t %IMG_CLIENT_LATEST% -f Client/Dockerfile .
if errorlevel 1 exit /b 1
echo.


REM =================================================
echo Tagging versioned images...
docker tag %IMG_SERVER_LATEST% %IMG_SERVER_VER%
docker tag %IMG_CLIENT_LATEST% %IMG_CLIENT_VER%

REM =================================================
echo Preparing deployment folder...

if exist "%DEPLOY_FOLDER%" rd /s /q "%DEPLOY_FOLDER%"
mkdir "%DEPLOY_FOLDER%"
mkdir "%DEPLOY_FOLDER%\images"
mkdir "%DEPLOY_FOLDER%\certs"
mkdir "%DEPLOY_FOLDER%\backup"

echo Exporting Docker images...
docker save %IMG_SERVER_LATEST% %IMG_SERVER_VER% -o "%DEPLOY_FOLDER%\images\server_%NEW_VERSION%.tar"
docker save %IMG_CLIENT_LATEST% %IMG_CLIENT_VER% -o "%DEPLOY_FOLDER%\images\client_%NEW_VERSION%.tar"
docker save %IMG_MYSQL% -o "%DEPLOY_FOLDER%\images\mysql_8_0.tar"

echo Copying certificates...
xcopy /E /I /Y Server\certs "%DEPLOY_FOLDER%\certs" >nul
xcopy /E /I /Y Client\certs "%DEPLOY_FOLDER%\certs" >nul

echo Copying docker-compose.deploy.yml...
copy /Y docker-compose.deploy.yml "%DEPLOY_FOLDER%\docker-compose.yml" >nul


REM =================================================
echo Writing README.RUN.txt...
(
echo Buddy2Go %MODE% Package %NEW_VERSION%
echo ---------------------------------------------
echo docker load -i images/server_%NEW_VERSION%.tar
echo docker load -i images/client_%NEW_VERSION%.tar
echo docker load -i images/mysql_8_0.tar
echo.
echo Start containers:
echo    docker compose up -d
) > "%DEPLOY_FOLDER%\README.RUN.txt"


REM =================================================
echo Creating TAR.GZ package...
tar -czvf "%TAR_NAME%" "%DEPLOY_FOLDER%" >nul

echo Copying to output folder: %TARGET_DIR%
copy /Y "%TAR_NAME%" "%OUTPUT_PACKAGE%" >nul

REM Delete the project-root tar.gz (e.g., Buddy2Go_local_V0.1.4.tar.gz)
if exist "%TAR_NAME%" (
    echo Deleting %TAR_NAME%...
    del /Q "%TAR_NAME%" >nul 2>&1
)

REM Delete the project-root output folder (e.g., Buddy2Go_local_V0.1.4)
if exist "%DEPLOY_FOLDER%" (
    echo Deleting temporary folder %DEPLOY_FOLDER%...
    rd /s /q "%DEPLOY_FOLDER%"
)

REM Delete the temporary build.appsettings.json
echo Cleaning temporary build file...
del /Q Client\wwwroot\build.appsettings.json >nul 2>&1

echo.
echo ===============================================
echo DONE — package created:
echo %OUTPUT_PACKAGE%
echo ===============================================
echo.

pause
exit /b 0
