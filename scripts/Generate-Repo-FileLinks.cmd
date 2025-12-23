@echo off
setlocal ENABLEEXTENSIONS
REM File: Generate-Repo-FileLinks.cmd
REM Purpose: Launch the PowerShell script with ExecutionPolicy Bypass

set SCRIPT=%~dp0Generate-Repo-FileLinks.ps1
if not exist "%SCRIPT%" (
  echo Could not find "%SCRIPT%". Place this .cmd next to Generate-Repo-FileLinks.ps1
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%"
exit /b %ERRORLEVEL%
