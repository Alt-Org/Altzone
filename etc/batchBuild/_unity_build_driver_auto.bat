@echo off
rem
rem This is an example script how to CALL batch build for given build target (%1) from AN OTHER PROCESS.
rem
rem This is intended to be started programmatically!
rem Rebuild is not supported with this script!
rem
set BUILD_TARGET=%1
set UNITY_EXE_OVERRIDE=
set REBUILD=
set ENVFILE_NAME=_build_%BUILD_TARGET%.env
if exist %ENVFILE_NAME% (
	echo *
	echo * Current directory %cd%
	echo *
	cd ..\..
)
set BATCH_BUILD_DIR=.\etc\batchBuild
set ENVFILE_PATH=%BATCH_BUILD_DIR%\%ENVFILE_NAME%
%BATCH_BUILD_DIR%\unityBatchBuild.bat %ENVFILE_PATH% %REBUILD%
