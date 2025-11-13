@echo off
setlocal enabledelayedexpansion

echo ===============================================
echo   Buddy2Go Deployment Package Builder
echo ===============================================

REM -----------------------------------------------
REM 1. CONFIGURATION
REM -----------------------------------------------

set DEPLOY_FOLDER=Buddy2Go_Deployment
set TAR_NAME=Buddy2Go_Deployment.tar.gz

REM Your Docker image names
set IMG_SERVER=buddy2go-server
set IMG_CLIENT=buddy2go-client
set IMG_MYSQL=mysql:8.0

REM Optional MySQL dump
set MYSQL_CONTAINER=buddy2go-mysql
set MYSQL_USER=root
set MYSQL_PASS=__xh8xq9KmE4E.CwpQDG
set MYSQL_DB=Buddy2Go


echo.
echo Cleaning old deployment folder...
if exist "%DEPLOY_FOLDER%" rd /s /q "%DEPLOY_FOLDER%"

echo Creating deployment folder...
mkdir "%DEPLOY_FOLDER%"
mkdir "%DEPLOY_FOLDER%\images"
mkdir "%DEPLOY_FOLDER%\certs"
mkdir "%DEPLOY_FOLDER%\backup"


REM -----------------------------------------------
REM 2. EXPORT DOCKER IMAGES
REM -----------------------------------------------

echo.
echo Exporting Docker images...

docker save %IMG_SERVER% -o "%DEPLOY_FOLDER%\images\server.tar"
docker save %IMG_CLIENT% -o "%DEPLOY_FOLDER%\images\client.tar"
docker save %IMG_MYSQL% -o "%DEPLOY_FOLDER%\images\mysql.tar"

echo Images exported.


REM -----------------------------------------------
REM 3. COPY CONFIG FILES
REM -----------------------------------------------

echo.
echo Copying docker-compose.yml...
copy /Y docker-compose.deploy.yml "%DEPLOY_FOLDER%\docker-compose.yml"

echo.
echo Copying certificates...
xcopy /E /I /Y Server\certs "%DEPLOY_FOLDER%\certs"
xcopy /E /I /Y Client\certs "%DEPLOY_FOLDER%\certs"


REM -----------------------------------------------
REM 4. OPTIONAL: DATABASE BACKUP
REM -----------------------------------------------

echo.
echo Creating MySQL backup (optional)...

echo Running: mysqldump inside container %MYSQL_CONTAINER%
docker exec %MYSQL_CONTAINER% mysqldump -u %MYSQL_USER% -p%MYSQL_PASS% %MYSQL_DB% > "%DEPLOY_FOLDER%\backup\Buddy2Go.sql"

echo Database backup saved.


REM -----------------------------------------------
REM 5. CREATE README FILE
REM -----------------------------------------------

(
echo INSTALLATION INSTRUCTIONS
echo -------------------------
echo.
echo 1. Ensure Docker Desktop is installed.
echo 2. Extract this archive anywhere.
echo 3. Import images:
echo      docker load -i images\server.tar
echo      docker load -i images\client.tar
echo      docker load -i images\mysql.tar
echo.
echo 4. Start system:
echo      docker compose up -d
echo.
) > "%DEPLOY_FOLDER%\README.RUN.txt"


REM -----------------------------------------------
REM 6. PACKAGE EVERYTHING INTO tar.gz
REM -----------------------------------------------

echo.
echo Creating TAR.GZ package...

REM Check if tar exists (Windows 10+ has built-in bsdtar)
where tar >nul 2>&1
if errorlevel 1 (
    echo ERROR: tar.exe not found. Please install Git for Windows or bsdtar.
    exit /b 1
)

tar -czvf "%TAR_NAME%" "%DEPLOY_FOLDER%"

echo.
echo ===============================================
echo Deployment Package Created:
echo %TAR_NAME%
echo ===============================================

pause
