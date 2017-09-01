echo off
perfmon /report
cd /d D:\ABN Support Toolkit\SysinternalsSuite\
procdump "runIC" "D:\BATS\Temp" -accepteula
exit