@echo off
REM ·��
set EMULATOR_PATH=D:\leidian\LDPlayer9\dnplayer.exe
set ADB_PATH=D:\leidian\LDPlayer9\adb.exe
set MAIN_PATH=D:\vsproject\mxdat\mxdat\bin\Debug\net8.0\mxdat.exe

REM ����ģ����
start "" "%EMULATOR_PATH%"

REM �ȴ�ģ����������������Ҫ�����ȴ�ʱ�䣩
timeout /t 30

REM ʹ�� ADB ���ӵ�ģ����������Ӧ�ú� WireGuard
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

REM ���� MITMproxy ����������
REM �������� MITMproxy ���õ�������
start cmd /k mitmweb -m wireguard --no-http2 -s extract_multipart.py

REM ��ʾ�û����в������ȴ��������
echo ������������������ɺ����������...
pause

REM �ر�ģ����
taskkill /f /t /im "dnplayer.exe"

python getSessionkey.py
timeout /t 2

start "" "%MAIN_PATH%"