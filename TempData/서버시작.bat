@echo off
cd /d D:\GitProject\Ratkin
echo 서버 시작 중... (http://localhost:8080/TempData/shield_calc.html)
start "" "http://localhost:8080/TempData/shield_calc.html"
python -m http.server 8080
pause
