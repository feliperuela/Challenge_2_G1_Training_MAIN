@echo off
setlocal

REM === ajuste estes caminhos se precisar ===
set "VENV=C:\UnityProjects\CloudWalk\Challenge_2_G1_Training_MAIN\G1_Agent_Training_V6\venv"
set "WORKDIR=C:\UnityProjects\CloudWalk\Challenge_2_G1_Training_MAIN\G1_Agent_Training_V6"
set "LOGDIR=results"
set "PORT=6006"

pushd "%WORKDIR%"

REM inicia o TensorBoard usando o python do venv (dispensa ativar)
"%VENV%\Scripts\python.exe" -m tensorboard.main --logdir "%LOGDIR%" --host 127.0.0.1 --port %PORT% ^
  --reload_interval 5 --samples_per_plugin images=100

set EXITCODE=%ERRORLEVEL%

REM tenta abrir o navegador (espera 2s pra garantir que subiu)
timeout /t 2 /nobreak >nul
start "" "http://127.0.0.1:%PORT%"

echo.
echo TensorBoard exited with code: %EXITCODE%
echo Close this window to stop TensorBoard.
pause
popd
endlocal
