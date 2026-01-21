@echo off
set PATH=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Microsoft\VisualStudio\NodeJs;%PATH%
cd /d "%~dp0"
call npm run dev
