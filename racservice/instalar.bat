@ECHO OFF
echo Instalando Rac Service...
echo ---------------------------------------------------
sc create racservice binPath=C:\robot_rac\racservice.exe start=auto
sc start racservice
echo ---------------------------------------------------
pause
echo Done.