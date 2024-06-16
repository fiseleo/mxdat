@echo off
REM 路径
set EMULATOR_PATH=D:\leidian\LDPlayer9\dnplayer.exe
set ADB_PATH=D:\leidian\LDPlayer9\adb.exe
set MAIN_PATH=D:\vsproject\mxdat\mxdat\bin\Debug\net8.0\mxdat.exe

REM 启动模拟器
start "" "%EMULATOR_PATH%"

REM 等待模拟器启动（根据需要调整等待时间）
timeout /t 30

REM 使用 ADB 连接到模拟器并启动应用和 WireGuard
"%ADB_PATH%" devices
timeout /t 2
"%ADB_PATH%" devices
timeout /t 5
"%ADB_PATH%" root
"%ADB_PATH%" devices
timeout /t 2
"%ADB_PATH%" shell am start -n com.wireguard.android/com.wireguard.android.activity.MainActivity
timeout /t 2
"%ADB_PATH%" devices
timeout /t 2
"%ADB_PATH%" shell am start -n com.nexon.bluearchive/com.nexon.bluearchive.MxUnityPlayerActivity

REM 启动 MITMproxy 并进行拦截
REM 请根据你的 MITMproxy 配置调整命令
start cmd /k mitmweb -m wireguard --no-http2 -s extract_multipart.py

REM 提示用户进行操作，等待拦截完成
echo 请进行所需操作并在完成后按任意键继续...
pause

REM 关闭模拟器
taskkill /f /t /im "dnplayer.exe"

python getSessionkey.py
timeout /t 2

start "" "%MAIN_PATH%"