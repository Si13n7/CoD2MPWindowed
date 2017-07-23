::+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
::-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
::                                                           -+
::       _________.__  ____ ________        _________        +-
::      /   _____/|__|/_   |\_____  \   ____\______  \       -+
::      \_____  \ |  | |   |  _(__  <  /    \   /    /       +-
::      /        \|  | |   | /       \|   |  \ /    /        -+
::     /_______  /|__| |___|/______  /|___|  //____/         +-
::             \/                  \/      \/                -+
::                                                           +-
::                  D E V E L O P M E N T S                  -+
::                                                           +-
::+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
::-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
:: www.si13n7.com

@echo off
title File Transfer
set url=https://raw.githubusercontent.com/Si13n7/SilDev.CSharpLib/master/bin/SilDev.CSharpLib.dll
set dir=%~dp0..\..\SilDev.CSharpLib\bin
set path=%dir%\SilDev.CSharpLib.dll
if not exist "%dir%" (
    echo Do you want to create the following directory: "%dir%"?
    echo Press any key to continue OR close the window to cancel . . .
    pause >nul
    md "%dir%"
)
cd /d %WinDir%\System32
if exist bitsadmin.exe (
    call bitsadmin /transfer "File Transfer" "%url%" "%path%"
    echo.
    color 0a
    for %%i in (5 4 3 2 1) do ( 
        ping localhost -n 2 >nul
        echo %%i
        echo.
    )
) else (
    cd WindowsPowerShell\v1.0
        color 1f
    echo.
    echo Downloading . . .
    if exist "%path%" del /f /q "%path%"
    if not exist "%path%" (
        call powershell -Command "(New-Object Net.WebClient).DownloadFile('%url%', '%path%')"
    ) else (
        color 4f
        echo No write permissions to replace '%path%' file.
    )
)
exit
