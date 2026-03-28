@echo off

set /p fileList=Enter a comma-separated list of files or a directory folder: 
set /p exportDir=Enter the export directory (leave blank for .\converted): 
set /p keepEye=Keep eye bones? (y/n):
set /p log=Output NMD info to log file in export directory? (y/n):
set /p openExport=Open export directory when finished? (y/n):

    if /i "%keepEye%"=="y" (
	if /i "%log%"=="y" (
            if /i "%openExport%"=="y" (
            	start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --keepeye --log --openexportdir
    	    ) else (
                start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --keepeye --log
    	    )
    	) else (
            if /i "%openExport%"=="y" (
            	start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --keepeye --openexportdir
    	    ) else (
                start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --keepeye
    	    )
    	)
    ) else (
        if /i "%log%"=="y" (
            if /i "%openExport%"=="y" (
            	start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --log --openexportdir
    	    ) else (
                start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --log
    	    )  
    	) else (
 	    if /i "%openExport%"=="y" (
            	start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%" --keepeye --openexportdir
    	    ) else (
                start "" ".\NMDSuite.exe" /parsefiles "%fileList%" --exportdir "%exportDir%"
    	    )
    	)
    )
exit
