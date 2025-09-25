@echo off
setlocal
set "VENV=C:\UnityProjects\CloudWalk\Challenge_2_G1_Training_MAIN\G1_Agent_Training_V6\venv"
set "WORKDIR=C:\UnityProjects\CloudWalk\Challenge_2_G1_Training_MAIN\G1_Agent_Training_V6"
set "CFG=config\g1_config_final.yaml"
set "RUNID=G1_Brain_004"

pushd "%WORKDIR%"
"%VENV%\Scripts\python.exe" -m mlagents.trainers.learn "%CFG%" --run-id=%RUNID% --resume
echo.
echo Finished with exit code: %ERRORLEVEL%
pause
popd
endlocal
