@echo off
rem
rem This is an example script how to copy build results to distrobution folder.
rem
rem You can start it directly here, where it is in version control.
rem
rem Alternatively you can copy (or link) this file to UNITY project root folder
rem and start it from there - for your convenience.
rem
if not exist _local_build_properties.env (
	cd ..\..
)
echo Current directory %cd%

FOR /F "eol=#" %%i IN (_local_build_properties.env) DO (
	echo env set %%i
	SET %%i
)

set SOURCE_DIR=.\build%BUILD_TARGET%
set TARGET_DIR=%DIST_FOLDER_BASE%%BUILD_NUMBER%

echo.
echo SOURCE_DIR=.\build%BUILD_TARGET%
echo TARGET_DIR=%DIST_FOLDER_BASE%%BUILD_NUMBER%
if exist "%TARGET_DIR%" (
    echo *
    echo * Target dir EXISTS already, can not copy over it
    echo *
    goto :done
)

echo.
robocopy "%SOURCE_DIR%" "%TARGET_DIR%" *.* /S /E /V
echo.
echo Build %BUILD_NUMBER% copied
goto :done

:done
if "%1" == "" echo. && pause