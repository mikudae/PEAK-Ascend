@echo off
setlocal
echo Starting Master instance...
cd /d "C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK"
start "" "PEAK.exe"
ping 127.0.0.1 -n 4 >nul
echo Starting Slave instance (windowed)...
cd /d "C:\Users\feldring\Downloads\PEAK\PEAK.v2.6.b-OFME\PEAK_Slave"
start "" "PEAK.exe" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
echo Both instances launched!
