@echo off

:: 获取当前目录
set denconPath=%~dp0

::获取操作系统版本及系统类型

set OsVersion=0
set OsProcessor=0
set driverRootPath=0
set driverSubPath=0

:: 判断操作系统版本:

:: ver|findstr /r /i " [版本 5.1.*]" > NUL && goto WindowsXP
ver|findstr /r /i " [版本 6.1.*]" > NUL && goto Windows7
ver|findstr /r /i " [版本 6.3.*]" > NUL && goto Windows8
ver|findstr /r /i " [版本 10.*]" > NUL && goto Windows10 
goto UnknownVersion

:WindowsXP
set OsVersion="WindowsXP"
goto GetProcessor

:Windows7
set OsVersion="Windows7"
set driverPath=win7
goto GetProcessor


:Windows8
set OsVersion="Windows8.1"
set driverPath=win81
goto GetProcessor

:Windows10 
set OsVersion="Windows10"
set driverPath=win81
goto GetProcessor

:UnknownVersion
set OsVersion="UnknownVersion"
set driverPath=0
goto GetProcessor

:: 判断系统类型:

:GetProcessor
if /i "%processor_architecture%" equ "x86" (
set OsProcessor="X86"
set driverSubPath=x86
) else (
if /i "%processor_architecture%" equ "amd64" (
set OsProcessor="X64"
set driverSubPath=x64
) else (
set OsProcessor="UnknownProcessor"
)
)

echo %OsVersion% %OsProcessor%

:: 拼接命令字符串:

set denconfullPath=%denconPath%%driverSubPath%\devcon.exe
set driverfullPath=%denconPath%%driverPath%\%driverSubPath%\cyusb3.inf

echo %denconPath%
echo %denconfullPath%
echo %driverfullPath%

:: 检查当前用户权限
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"  

echo "return" %errorlevel%


echo "%denconfullPath%" /r install "%driverfullPath%" "USB\VID_04B4&PID_00F3"
echo "%denconfullPath%" remove "USB\VID_04B4&PID_00F3"
echo "%denconfullPath%" rescan


:: 执行usb安装命令 所有的执行字符串都要加"",防止路径中有空格或者其他不能正确识别的字符出现

"%denconfullPath%" /r install "%driverfullPath%" "USB\VID_04B4&PID_00F3"
"%denconfullPath%" remove "USB\VID_04B4&PID_00F3"
"%denconfullPath%" rescan

exit