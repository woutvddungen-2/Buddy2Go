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

REM ----------------- Version file per mode ------------------

if "%MODE%"=="local" (
    set VERSION_FILE=version-local.txt
    set OUTPUT_SUBDIR=local
) else (
    set VERSION_FILE=version-deploy.txt
    set OUTPUT_SUBDIR=deploy
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

REM ----------------- AppSettings selection ------------------

if "%MODE%"=="local" (
    echo Using LOCAL appsettings.json...
    copy /Y Client\wwwroot\appsettings.json Client\wwwroot\build.appsettings.json >nul
) else (
    echo Using DEPLOYMENT docker.appsettings.json...
    copy /Y Client\wwwroot\docker.appsettings.json Client\wwwroot\build.appsettings.json >nul
)
echo.


REM ---------------- CONFIG ----------------
set BUILD_ROOT=C:\Users\admin\source\Builds\Docker
set TARGET_DIR=%BUILD_ROOT%\%OUTPUT_SUBDIR%

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

set DEPLOY_FOLDER=Buddy2Go_%MODE%_%NEW_VERSION%
set TAR_NAME=Buddy2Go_%MODE%_%NEW_VERSION%.tar.gz
set OUTPUT_PACKAGE=%TARGET_DIR%\%TAR_NAME%

set IMG_SERVER_VER=buddy2go-server:%NEW_VERSION%
set IMG_CLIENT_VER=buddy2go-client:%NEW_VERSION%
set IMG_SERVER_LATEST=buddy2go-server:latest
set IMG_CLIENT_LATEST=buddy2go-client:latest
set IMG_MYSQL=mysql:8.0

echo Build folder: %DEPLOY_FOLDER%
echo Output package: %OUTPUT_PACKAGE%
echo.

REM ============================================================
echo Building SERVER image...
docker build -t %IMG_SERVER_LATEST% -f Server/Dockerfile .
if errorlevel 1 exit /b 1

echo Building CLIENT image...
docker build -t %IMG_CLIENT_LATEST% -f Client/Dockerfile .
if errorlevel 1 exit /b 1
echo.


REM ============================================================
echo Tagging versioned images...
docker tag %IMG_SERVER_LATEST% %IMG_SERVER_VER%
docker tag %IMG_CLIENT_LATEST% %IMG_CLIENT_VER%
echo.


REM ============================================================
echo Preparing deployment folder...
if exist "%DEPLOY_FOLDER%" rd /s /q "%DEPLOY_FOLDER%"
mkdir "%DEPLOY_FOLDER%"
mkdir "%DEPLOY_FOLDER%\images"
mkdir "%DEPLOY_FOLDER%\certs"
mkdir "%DEPLOY_FOLDER%\backup"
echo.

echo Exporting Docker images...
docker save %IMG_SERVER_LATEST% %IMG_SERVER_VER% -o "%DEPLOY_FOLDER%\images\server_%NEW_VERSION%.tar"
docker save %IMG_CLIENT_LATEST% %IMG_CLIENT_VER% -o "%DEPLOY_FOLDER%\images\client_%NEW_VERSION%.tar"
docker save %IMG_MYSQL% -o "%DEPLOY_FOLDER%\images\mysql_8_0.tar"
echo.

echo Copying certificates...
xcopy /E /I /Y Server\certs "%DEPLOY_FOLDER%\certs" >nul
xcopy /E /I /Y Client\certs "%DEPLOY_FOLDER%\certs" >nul
echo.

echo Copying docker-compose.deploy.yml...
copy /Y docker-compose.deploy.yml "%DEPLOY_FOLDER%\docker-compose.yml" >nul
echo.


REM ============================================================
echo Copying Kubernetes manifests...

if exist "K8s" (
    echo Adding K8s manifests to output...
    mkdir "%DEPLOY_FOLDER%\k8s" >nul 2>&1
    xcopy /E /I /Y "K8s" "%DEPLOY_FOLDER%\k8s" >nul
) else (
    echo WARNING: K8s folder not found — skipping Kubernetes export.
)
echo.



REM ============================================================
echo Writing README.RUN.txt...

if "%MODE%"=="deploy" (
    REM ------------ Kubernetes README --------------
    (
    echo Buddy2Go Kubernetes Deployment Package %NEW_VERSION%
    echo ====================================================
    echo.
    echo This package contains docker images and Kubernetes manifests.
    echo.
    echo ----------------------------------------------------
    echo 1. Load Docker Images into local Docker
    echo ----------------------------------------------------
    echo docker load -i images/server_%NEW_VERSION%.tar
    echo docker load -i images/client_%NEW_VERSION%.tar
    echo docker load -i images/mysql_8_0.tar
    echo.
    echo ----------------------------------------------------
    echo 2. Push images to your private registry \(if required\)
    echo ----------------------------------------------------
    echo docker tag buddy2go-server:%NEW_VERSION% registry/company/buddy2go-server:%NEW_VERSION%
    echo docker tag buddy2go-client:%NEW_VERSION% registry/company/buddy2go-client:%NEW_VERSION%
    echo.
    echo docker push registry/company/buddy2go-server:%NEW_VERSION%
    echo docker push registry/company/buddy2go-client:%NEW_VERSION%
    echo.
    echo ----------------------------------------------------
    echo 3. Apply Kubernetes manifests
    echo ----------------------------------------------------
    echo kubectl apply -f k8s/
    echo.
    echo ----------------------------------------------------
    echo 4. Verify Deployment
    echo ----------------------------------------------------
    echo kubectl get pods -n buddy2go
    echo kubectl get svc -n buddy2go
    echo kubectl get ingress -n buddy2go
    echo.
    echo Deployment version: %NEW_VERSION%
    ) > "%DEPLOY_FOLDER%\README.RUN.txt"
) else (
    REM ------------ LOCAL README -------------------
    (
    echo Buddy2Go LOCAL Package %NEW_VERSION%
    echo =============================================
    echo docker load -i images/server_%NEW_VERSION%.tar
    echo docker load -i images/client_%NEW_VERSION%.tar
    echo docker load -i images/mysql_8_0.tar
    echo.
    echo Start containers:
    echo    docker compose up -d
    ) > "%DEPLOY_FOLDER%\README.RUN.txt"
)

echo.


REM ============================================================
echo Creating TAR.GZ package...
tar -czvf "%TAR_NAME%" "%DEPLOY_FOLDER%" >nul

echo Copying to output folder: %TARGET_DIR%
copy /Y "%TAR_NAME%" "%OUTPUT_PACKAGE%" >nul
echo.


REM ============================================================
echo Cleaning leftover files...

if exist "%TAR_NAME%" (
    echo Removing project-root tar.gz...
    del /Q "%TAR_NAME%" >nul
)

if exist "%DEPLOY_FOLDER%" (
    echo Removing temporary folder...
    rd /s /q "%DEPLOY_FOLDER%"
)

echo Removing temporary build.appsettings.json...
del /Q Client\wwwroot\build.appsettings.json >nul
echo.

echo ===============================================
echo DONE — PACKAGE SAVED TO:
echo %OUTPUT_PACKAGE%
echo ===============================================
echo.

pause
exit /b 0
