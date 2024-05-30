@echo off
rem
rem This is an example script to start batch build for given build target (%1).
rem
rem You can start it directly here, where it is in version control.
rem
rem Alternatively you can copy this file to UNITY project root folder
rem and start it from there - for your convenience.
rem
set BUILD_TARGET=%1
set REBUILD=%2
set ENVFILE_NAME=_build_%BUILD_TARGET%.env
if exist %ENVFILE_NAME% (
	echo *
	echo * Current directory %cd%
	echo *
	cd ..\..
)
set BATCH_BUILD_DIR=.\etc\batchBuild
set ENVFILE_PATH=%BATCH_BUILD_DIR%\%ENVFILE_NAME%
call %BATCH_BUILD_DIR%\unityBatchBuild.bat %ENVFILE_PATH% %REBUILD%
echo.
pause