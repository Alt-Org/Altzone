@echo off
rem
rem LOG_WRITER_LOG must be set here because we need to expand environment variable %USERNAME%
rem
set LOG_WRITER_LOG=C:\Users\%USERNAME%\AppData\LocalLow\zoomierush\Zoomie Rush\editor_zoomie_rush_game.log
echo LOG_WRITER_LOG=%LOG_WRITER_LOG%
set ENV_FILE=%1
set REBUILD=%2
echo.
echo ~~~~~ BUILD start %TIME% with %ENV_FILE% %REBUILD% ~~~~~
echo *
echo * Current directory %cd%
echo *
if not exist %ENV_FILE% (
    echo *
    echo * Build environment file %ENV_FILE% not found
    echo *
    goto :done
)
FOR /F "eol=# tokens=*" %%i IN (%ENV_FILE%) DO (
	echo env set %%i
	SET %%i
)
rem --- all variable for build are collected now ---
if not "%UNITY_EXE_OVERRIDE%" == "" (
    rem UNITY_EXE_OVERRIDE can only set by the caller using environment variables!
    if not "%UNITY_EXE_OVERRIDE%" == "%UNITY_EXE%" (
        rem set latest UNITY executable (like if .env file is outdated)
        echo env set UNITY_EXE=%UNITY_EXE_OVERRIDE%
        set UNITY_EXE=%UNITY_EXE_OVERRIDE%
    )
)
if not exist "%UNITY_EXE%" (
    echo *
    echo * UNITY executable %UNITY_EXE% not found
    echo *
    goto :done
)
if exist %LOG_FILE% (
    echo. >%LOG_FILE%
)
if exist "%LOG_WRITER_LOG%" (
    del /Q "%LOG_WRITER_LOG%"
)
echo.
echo ~~~~~ BUILD execute %TIME% with %ENV_FILE% %REBUILD% ~~~~~
echo.
set BUILD_PARAMS1=-executeMethod %BUILD_METHOD% -quit -batchmode
set BUILD_PARAMS2=-projectPath .\ -buildTarget %BUILD_TARGET% -logFile "%LOG_FILE%"
set BUILD_PARAMS3=-envFile "%ENV_FILE%" %REBUILD%
echo set1 %BUILD_PARAMS1%
echo set2 %BUILD_PARAMS2%
echo set3 %BUILD_PARAMS3%
if not "%IS_TEST_RUN%" == "0" (
	echo.
	echo "%UNITY_EXE%" %BUILD_PARAMS1% %BUILD_PARAMS2% %BUILD_PARAMS3%
    echo *
    echo * This was for test: IS_TEST_RUN = %IS_TEST_RUN%
    echo *
    goto :done
)
echo - Start Build
@echo on
"%UNITY_EXE%" %BUILD_PARAMS1% %BUILD_PARAMS2% %BUILD_PARAMS3%
@set RESULT=%ERRORLEVEL%
@echo off
echo Build returns %RESULT%

if exist "%LOG_WRITER_LOG%" (
	echo copy %LOG_WRITER_LOG% to %LOG_FILE%.build.log
	copy /Y %LOG_WRITER_LOG% %LOG_FILE%.build.log >nul
)
echo.
echo ~~~~~ Revert build system changes to settings back to original state ~~~~~
@echo on
git checkout -f -- ProjectSettings\ProjectSettings.asset
git checkout -f -- Assets\Resources\GameAnalytics\Settings.asset
@echo off
echo.

if not "%RESULT%" == "0" (
    echo *
    echo * Build FAILED with %RESULT%, check log file %LOG_FILE% for errors :-^(
    echo *
    goto :done
)
if exist "build%BUILD_TARGET%" (
    echo *
    echo * Build output folder build%BUILD_TARGET% created, build done SUCCESSFULLY :-^)
    echo *
)
if not "%POST_PROCESS%" == "1" (
    goto :done
)
:post_test
if not exist %LOG_FILE% (
    echo *
    echo * Log file %LOG_FILE% not found, SKIP post processing
    echo *
    goto :done
)
echo.
echo ~~~~~ BUILD post processing %TIME% with %ENV_FILE% ~~~~~
echo copy %LOG_FILE% to %LOG_FILE_POST%
copy /Y %LOG_FILE% %LOG_FILE_POST% >nul
set POST_PARAMS1=-executeMethod %POST_METHOD% -quit -batchmode
set POST_PARAMS2=%BUILD_PARAMS2%
set POST_PARAMS3=%BUILD_PARAMS3%
echo set1 %POST_PARAMS1%
echo set2 %POST_PARAMS2%
echo set3 %POST_PARAMS3%
echo - Start Post Process
@echo on
"%UNITY_EXE%" %POST_PARAMS1% %POST_PARAMS2% %POST_PARAMS3%
@set RESULT=%ERRORLEVEL%
@echo off
echo Post processing returns %RESULT%

if exist "%LOG_WRITER_LOG%" (
	echo copy %LOG_WRITER_LOG% to %LOG_FILE%.post.log
	copy /Y %LOG_WRITER_LOG% %LOG_FILE%.post.log >nul
)

if "%RESULT%" == "0" (
	echo *
	echo * BUILD log file %LOG_FILE_POST% post processing done SUCCESSFULLY :-^)
	echo *
    goto :done
)
if "%RESULT%" == "10" (
	echo *
	echo * BUILD log file %LOG_FILE_POST% post processing skipped - no data ^>:[
	echo *
    goto :done
)
echo *
echo * Build post processing returns %RESULT%, check log file %LOG_FILE_POST% for errors :-^(
echo *

:done
echo.
echo ~~~~~ BUILD done %TIME% with %ENV_FILE% ~~~~~
