@echo off
setlocal enabledelayedexpansion

echo ===============================================
echo   Buddy2Go Deployment Package Builder
echo ===============================================

REM Jump over function definitions
goto :main


REM =================================================
REM FUNCTION: Keep only last 3 version tags (per image)
REM =================================================
:KeepLast3
REM %1 = repository name (e.g., buddy2go-server)
setlocal enabledelayedexpansion

echo Cleaning old versions for %1...

set count=0
for /f "tokens=1" %%t in ('
    docker images %1 --format "{{.Tag}}" ^| findstr /v "latest"
') do (
    set /a count+=1
    if !count! GTR 3 (
        echo Removing old tag: %1:%%t
        docker rmi %1:%%t >nul 2>&1
    )
)

endlocal
exit /b 0



REM =================================================
REM                      MAIN
REM =================================================
:main

REM ---------------- CONFIG ----------------
set VERSION_FILE=version.txt
set BUILD_OUTPUT_DIR=C:\Users\admin\source\Builds\Docker

if not exist "%VERSION_FILE%" (
    echo V0.1.1 > "%VERSION_FILE%"
)

set /p CURRENT_VERSION=<"%VERSION_FILE%"
echo Current version: %CURRENT_VERSION%

for /f "tokens=1-3 delims=." %%a in ("%CURRENT_VERSION:V=%") do (
    set MAJOR=%%a
    set MINOR=%%b
    set PATCH=%%c
)

set /a PATCH+=1
set NEW_VERSION=V%MAJOR%.%MINOR%.%PATCH%
echo New version: %NEW_VERSION%

echo %NEW_VERSION% > "%VERSION_FILE%"

set DEPLOY_FOLDER=Buddy2Go_Deployment_%NEW_VERSION%
set TAR_NAME=Buddy2Go_%NEW_VERSION%.tar.gz
set OUTPUT_PATH=%BUILD_OUTPUT_DIR%\%TAR_NAME%

set IMG_SERVER_VER=buddy2go-server:%NEW_VERSION%
set IMG_CLIENT_VER=buddy2go-client:%NEW_VERSION%
set IMG_SERVER_LATEST=buddy2go-server:latest
set IMG_CLIENT_LATEST=buddy2go-client:latest
set IMG_MYSQL=mysql:8.0

set MYSQL_CONTAINER=buddy2go-mysql
set MYSQL_USER=root
set MYSQL_PASS=__xh8xq9KmE4E.CwpQDG
set MYSQL_DB=Buddy2Go


REM ---------------- BUILD IMAGES ----------------
echo.
echo Building server image...
docker build -t %IMG_SERVER_LATEST% -f Server/Dockerfile .
if errorlevel 1 (
    echo FATAL ERROR: Failed to build server image.
    pause
    exit /b 1
)

echo.
echo Building client image...
docker build -t %IMG_CLIENT_LATEST% -f Client/Dockerfile .
if errorlevel 1 (
    echo FATAL ERROR: Failed to build client image.
    pause
    exit /b 1
)

REM ---------------- TAG VERSIONED IMAGES ----------------
echo.
echo Tagging images with version %NEW_VERSION%...

docker tag %IMG_SERVER_LATEST% %IMG_SERVER_VER%
if errorlevel 1 (
    echo FATAL ERROR: Failed to tag server image with %NEW_VERSION%.
    pause
    exit /b 1
)

docker tag %IMG_CLIENT_LATEST% %IMG_CLIENT_VER%
if errorlevel 1 (
    echo FATAL ERROR: Failed to tag client image with %NEW_VERSION%.
    pause
    exit /b 1
)

REM ----------- CLEANUP OLD VERSION TAGS (KEEP 3) ----------
echo.
call :KeepLast3 buddy2go-server
call :KeepLast3 buddy2go-client

REM ----------- ENSURE MYSQL IMAGE EXISTS ----------
echo.
echo Ensuring MySQL image %IMG_MYSQL% is available...
docker image inspect %IMG_MYSQL% >nul 2>&1
if errorlevel 1 (
    echo Pulling %IMG_MYSQL%...
    docker pull %IMG_MYSQL%
    if errorlevel 1 (
        echo FATAL ERROR: Failed to pull %IMG_MYSQL%.
        pause
        exit /b 1
    )
)


REM ----------- PREPARE DEPLOY FOLDER ----------
echo.
echo Cleaning old deployment folder...
if exist "%DEPLOY_FOLDER%" rd /s /q "%DEPLOY_FOLDER%"

echo Creating deployment folder: %DEPLOY_FOLDER%
mkdir "%DEPLOY_FOLDER%"
mkdir "%DEPLOY_FOLDER%\images"
mkdir "%DEPLOY_FOLDER%\certs"
mkdir "%DEPLOY_FOLDER%\backup"


REM ----------- EXPORT IMAGES (BOTH TAGS) ----------
echo.
echo Exporting Docker images...

docker save %IMG_SERVER_LATEST% %IMG_SERVER_VER% -o "%DEPLOY_FOLDER%\images\server_%NEW_VERSION%.tar"
if errorlevel 1 (
    echo FATAL ERROR: Failed to export server image tar.
    pause
    exit /b 1
)

docker save %IMG_CLIENT_LATEST% %IMG_CLIENT_VER% -o "%DEPLOY_FOLDER%\images\client_%NEW_VERSION%.tar"
if errorlevel 1 (
    echo FATAL ERROR: Failed to export client image tar.
    pause
    exit /b 1
)

docker save %IMG_MYSQL% -o "%DEPLOY_FOLDER%\images\mysql_8_0.tar"
if errorlevel 1 (
    echo FATAL ERROR: Failed to export MySQL image tar.
    pause
    exit /b 1
)

echo Images exported successfully.


REM ----------- COPY CONFIG & CERTS ----------
echo.
echo Copying docker-compose.deploy.yml to deployment folder...
copy /Y docker-compose.deploy.yml "%DEPLOY_FOLDER%\docker-compose.yml" >nul

echo.
echo Copying certificates...
xcopy /E /I /Y Server\certs "%DEPLOY_FOLDER%\certs" >nul
xcopy /E /I /Y Client\certs "%DEPLOY_FOLDER%\certs" >nul


REM ----------- OPTIONAL DB BACKUP ----------
echo.
echo Checking MySQL container for backup...

docker ps --format "{{.Names}}" | findstr /i "^%MYSQL_CONTAINER%$" >nul
if errorlevel 1 (
    echo WARNING: MySQL container "%MYSQL_CONTAINER%" not running, skipping backup.
) else (
    docker exec %MYSQL_CONTAINER% mysqldump -u %MYSQL_USER% -p%MYSQL_PASS% %MYSQL_DB% > "%DEPLOY_FOLDER%\backup\Buddy2Go.sql"
    if errorlevel 1 (
        echo WARNING: mysqldump failed, backup may be incomplete.
    ) else (
        echo Database backup complete.
    )
)


REM ----------- README.RUN.txt ----------
echo.
echo Writing README.RUN.txt...

(
echo Buddy2Go Deployment Package %NEW_VERSION%
echo ---------------------------------------------
echo.
echo 1. Load Docker images:
echo      docker load -i images/server_%NEW_VERSION%.tar
echo      docker load -i images/client_%NEW_VERSION%.tar
echo      docker load -i images/mysql_8_0.tar
echo.
echo 2. Start the system:
echo      docker compose up -d
echo.
) > "%DEPLOY_FOLDER%\README.RUN.txt"


REM ----------- CREATE TAR.GZ ----------
echo.
echo Creating TAR.GZ package: %TAR_NAME% ...

tar -czvf "%TAR_NAME%" "%DEPLOY_FOLDER%"


REM ----------- COPY TO BUILD OUTPUT ----------
echo.
echo Copying TAR.GZ to: %BUILD_OUTPUT_DIR%

if not exist "%BUILD_OUTPUT_DIR%" mkdir "%BUILD_OUTPUT_DIR%"
copy /Y "%TAR_NAME%" "%OUTPUT_PATH%" >nul

echo.
echo ===============================================
echo Deployment Package Created:
echo %OUTPUT_PATH%
echo Version: %NEW_VERSION%
echo ===============================================

pause
