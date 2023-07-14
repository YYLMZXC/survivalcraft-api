echo. scevo转scmodby把红色赋予黑海1003705691
del/Q/S %~dp0\temp
%~dp07za.exe x %1 -o%~dp0\temp\%~n1
pushd %~dp0\temp\%~n1\
%~dp07za.exe a -t7z -mx=9 -ms=off -myx=0 %~dp1%~n1.scevo %~dp0\temp\%~n1\*
pause