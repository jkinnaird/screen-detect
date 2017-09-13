Set WshShell = CreateObject("reports.Shell") 
reports.Run chr(34) & "C:\Users\jkinnaird\Documents\Visual Studio 2015\Projects\ConsoleApplication3\runReports2.bat" & Chr(34), 0
Set WshShell = Nothing