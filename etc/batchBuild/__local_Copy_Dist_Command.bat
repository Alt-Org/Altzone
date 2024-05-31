@echo off
rem
rem This is an example script how to copy build output to distribution folder.
rem
rem You can start it directly here, where it is in version control.
rem
rem Alternatively you can copy (or link) this file to UNITY project root folder
rem    and start it from there - for your convenience.
rem
set ENV_FILE=__local_copy_dist_properties.env
if not exist %ENV_FILE% (
    echo *
    echo * Copy environment file %ENV_FILE% not found
    echo *
    goto :done
)
echo Current directory %cd%
FOR /F "eol=# tokens=*" %%i IN (%ENV_FILE%) DO (
	echo env set %%i
	SET %%i
)
set ROBOCOPY_OPTIONS=/S /E /V
set TARGET_DIR=%DIST_FOLDER_BASE%%BUILD_FOLDER_NAME%
rem try current folder
set SOURCE_DIR=.\build%BUILD_TARGET%
if not exist "%SOURCE_DIR%" (
    rem try from upper folders
    set SOURCE_DIR=..\..\build%BUILD_TARGET%
)
if not exist "%SOURCE_DIR%" (
    echo *
    echo * Build dir build%BUILD_TARGET% not found
    echo *
    goto :done
)
echo.
echo BUILD_FOLDER_NAME=%BUILD_FOLDER_NAME%
echo SOURCE_DIR=.\build%BUILD_TARGET%
echo TARGET_DIR=%DIST_FOLDER_BASE%%BUILD_FOLDER_NAME%
echo.
if "%OVERWRITE_FOLDER%" == "1" (
    if exist "%TARGET_DIR%" (
        echo *
        echo * Target dir will be totally overwritten
        echo *
        set ROBOCOPY_OPTIONS=%ROBOCOPY_OPTIONS% /PURGE
    )
)
if "%OVERWRITE_FOLDER%" == "0" (
    if exist "%TARGET_DIR%" (
        echo *
        echo * Target dir EXISTS already, can not copy over it
        echo *
        goto :done
    )
)
robocopy "%SOURCE_DIR%" "%TARGET_DIR%" *.* %ROBOCOPY_OPTIONS%
echo.
echo Build %BUILD_FOLDER_NAME% done
goto :done

:done
if "%1" == "" echo. && pause
