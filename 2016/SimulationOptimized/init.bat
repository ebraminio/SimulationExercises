@echo off
rmdir /s /q build 2>NUL
mkdir build
cd build
cmake -G "Visual Studio 14 Win64" ../
cd..
