@echo off
rem
rem caddy run - same as caddy1 behaviour
rem caddy start
rem caddy reload
rem caddy stop
rem
set CADDY_EXE=.\caddy.exe
if exist "%CADDY_EXE%" (
	goto :go_caddy
)
set CADDY_EXE=.\caddy_windows_amd64.exe
if exist "%CADDY_EXE%" (
	goto :go_caddy
)
echo *
echo * Caddy executable %CADDY_EXE% not found
echo * Download it form official site and copy here
echo *
goto :eof

:go_caddy
if not exist "logs" (
    mkdir logs
)
set option=%1
if "%option%" == "" set option=run
if "%option%" == "s" set option=start
if "%option%" == "r" set option=reload
echo %CADDY_EXE% %option%
echo.
echo 	http://localhost:8080
echo.
%CADDY_EXE% %option%
