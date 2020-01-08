Application: 
Resource Logger Service

Description:
This application demonstrates the NETWAF.
It hosts a process that logs CPU and Memory usage to the Event Logs. 
It can be executed as a Desktop Application or as a Windows NT Service.
When executed as a Desktop Application the logs are displayed in addition to be logged to the Event Logs.
The application must be installed to create the EventLog source and register the Windows NT Service.

Installation:
Find the Developer Command Prompt, and then choose Run As Administrator.
Navigate to the folder that contains your project's output.

To install the application:
InstallUtil ResourceLoggerService.exe

To uninstall the application:
InstallUtil /u ResourceLoggerService.exe

