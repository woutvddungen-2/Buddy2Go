@echo off
setlocal enabledelayedexpansion

echo ===============================================
echo   Buddy2Go Build & Deployment Package Builder
echo ===============================================

REM -------------------------------------------------
REM Clean CLI flag parsing
REM Usage:
REM   --env local|deploy
REM   --ssl on|off
REM   --config release|debug
REM -------------------------------------------------

set MODE_ENV=deploy
set SSL_FLAG=false
set DOTNET_CONFIG=Release

:parse_args
if "%~1"=="" goto after_args

REM --env local|deploy
if /I "%~1"=="--env" (
    if /I "%~2"=="local" (
        set MODE_ENV=local
    ) else if /I "%~2"=="deploy" (
        set MODE_ENV=deploy
    ) else (
        echo [WARN] Unknown value for --env: %~2 ^(expected local or deploy^)
    )
    shift
    shift
    goto parse_args
)

REM --ssl on|off
if /I "%~1"=="--ssl" (
    if /I "%~2"=="on" (
        set SSL_FLAG=true
    ) else if /I "%~2"=="off" (
        set SSL_FLAG=false
    ) else (
        echo [WARN] Unknown value for --ssl: %~2 ^(expected on or off^)
    )
    shift
    shift
    goto parse_args
)

REM --config release|debug
if /I "%~1"=="--config" (
    if /I "%~2"=="release" (
        set DOTNET_CONFIG=Release
    ) else if /I "%~2"=="debug" (
        set DOTNET_CONFIG=Debug
    ) else (
        echo [WARN] Unknown value for --config: %~2 ^(expected release or debug^)
    )
    shift
    shift
    goto parse_args
)

echo [WARN] Unknown argument ignored: %1
shift
goto parse_args

:after_args

echo Environment   : %MODE_ENV%
echo SSL Enabled   : %SSL_FLAG%
echo DotNet Config : %DOTNET_CONFIG%

echo --------------------------------------------------
echo.

REM ----------------- Get Git Commit Hash ------------------
for /f "delims=" %%c in ('git rev-parse --short HEAD 2^>nul') do set GIT_COMMIT=%%c
if "%GIT_COMMIT%"=="" set GIT_COMMIT=unknown
echo Git commit: %GIT_COMMIT%
echo.

REM ----------------- Version Selection ------------------
if "%MODE_ENV%"=="local" (
    set VERSION_FILE=Build/version-local.txt
) else (
    set VERSION_FILE=Build/version-deploy.txt
)

if not exist "%VERSION_FILE%" (
    echo V0.1.0.0 > "%VERSION_FILE%"
)

set /p CURRENT_VERSION=<"%VERSION_FILE%"
echo Current version (%MODE_ENV%): %CURRENT_VERSION%

for /f "tokens=1-4 delims=." %%a in ("%CURRENT_VERSION:V=%") do (
    set MAJOR=%%a
    set MINOR=%%b
    set PATCH=%%c
    set FIX=%%d
)

set /a FIX+=1
set NEW_VERSION=V%MAJOR%.%MINOR%.%PATCH%.%FIX%
echo New version: %NEW_VERSION%
echo %NEW_VERSION% > "%VERSION_FILE%"
echo.

REM ----------------- AppSettings Selection ------------------
if "%MODE_ENV%"=="local" (
    if "%SSL_FLAG%"=="true" (
        echo Using LOCAL HTTPS appsettings.json...
        copy /Y Client\Build\appsettings\appsettings.https.local.json Client\Build\appsettings\appsettings.json >nul
    ) else (
        echo Using LOCAL HTTP appsettings.json...
        copy /Y Client\Build\appsettings\appsettings.http.local.json Client\Build\appsettings\appsettings.json >nul
    )
) else (
    echo Using DEPLOY appsettings.json...
    copy /Y Client\Build\appsettings\appsettings.https.deploy.json Client\Build\appsettings\appsettings.json >nul
    
)
echo.


REM ---------------- CONFIG ----------------
set BUILD_OUTPUT_DIR=C:\Users\admin\source\Builds\Docker
set TARGET_DIR=%BUILD_OUTPUT_DIR%\%MODE_ENV%

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

set DEPLOY_FOLDER=Buddy2Go_%MODE_ENV%_%NEW_VERSION%
set TAR_NAME=Buddy2Go_%MODE_ENV%_%NEW_VERSION%.tar.gz
set OUTPUT_PACKAGE=%TARGET_DIR%\%TAR_NAME%

set IMG_SERVER_VER=buddy2go-server:%MODE_ENV%-%NEW_VERSION%
set IMG_CLIENT_VER=buddy2go-client:%MODE_ENV%-%NEW_VERSION%

set IMG_SERVER_LATEST=buddy2go-server:%MODE_ENV%-latest
set IMG_CLIENT_LATEST=buddy2go-client:%MODE_ENV%-latest

set IMG_MYSQL=mysql:8.0

REM =================================================
if "%SSL_FLAG%"=="true" (
    set DOCKER_SSL_ARG=1
) else (
    set DOCKER_SSL_ARG=0
)

echo Building server image...
docker build --build-arg USE_SSL=%DOCKER_SSL_ARG% --build-arg DOTNET_CONFIG=%DOTNET_CONFIG% -t %IMG_SERVER_LATEST% -f Server/Dockerfile .
if errorlevel 1 exit /b 1

echo Building client image...
docker build --build-arg USE_SSL=%DOCKER_SSL_ARG% --build-arg DOTNET_CONFIG=%DOTNET_CONFIG% -t %IMG_CLIENT_LATEST% -f Client/Dockerfile .

if errorlevel 1 exit /b 1

echo build with arguments%DOCKER_SSL_ARG%, %IMG_SERVER_LATEST%, %IMG_CLIENT_LATEST%
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

REM =================================================
echo Selecting docker-compose file...

if "%MODE_ENV%"=="local" (
    if "%SSL_FLAG%"=="true" (
        echo Using LOCAL HTTPS docker-compose file...
        copy /Y "Build\compose\docker-compose.https.yml" "%DEPLOY_FOLDER%\docker-compose.yml" >nul

        echo Copying certificates for LOCAL HTTPS...
        mkdir "%DEPLOY_FOLDER%\certs" 2>nul

        xcopy /E /I /Y "Server\certs" "%DEPLOY_FOLDER%\certs" >nul
        xcopy /E /I /Y "Client\certs" "%DEPLOY_FOLDER%\certs" >nul

    ) else (
        echo Using LOCAL HTTP docker-compose file...
        copy /Y "Build\compose\docker-compose.http.yml" "%DEPLOY_FOLDER%\docker-compose.yml" >nul

        echo SSL disabled — no certs copied.
    )
) else (
    echo DEPLOY MODE — No docker-compose.yml needed.
    echo Kubernetes manifests will be used.
)

REM =================================================
if "%MODE_ENV%"=="local" (
    echo Writing README.RUN.txt...
    (
    echo Buddy2Go %MODE_ENV% Package %NEW_VERSION%
    echo ---------------------------------------------
    echo SSL Enabled: %SSL_FLAG%
    echo.
    echo docker load -i images/server_%NEW_VERSION%.tar
    echo docker load -i images/client_%NEW_VERSION%.tar
    echo docker load -i images/mysql_8_0.tar
    echo.
    echo Start containers:
    echo    docker compose up -d
    ) > "%DEPLOY_FOLDER%\README.RUN.txt"
)

REM =================================================
echo Creating TAR.GZ package...
tar -czvf "%TAR_NAME%" "%DEPLOY_FOLDER%" >nul

echo Copying to output folder: %TARGET_DIR%
copy /Y "%TAR_NAME%" "%OUTPUT_PACKAGE%" >nul

REM Cleanup
del /Q "%TAR_NAME%" >nul
rd /s /q "%DEPLOY_FOLDER%"
del /Q Client\Build\appsettings\appsettings.json >nul

echo.
echo ===============================================
echo DONE — package created:
echo %OUTPUT_PACKAGE%
echo ===============================================
echo.

pause
exit /b 0
