@echo off
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild Service/Service.csproj
start Service/bin/Debug/Service.exe

cd Site
call npm link
start npm start

start http://localhost:8080

