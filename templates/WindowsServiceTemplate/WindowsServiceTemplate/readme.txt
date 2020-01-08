NETWAFService
Templates includes the necessary classes for building a Windows Service application.


Files:
++++++

Program.cs
Calls the ApplicationHost passing the type of the process that is being hosted.

MainProcess.cs
Defines the process that is being hosted, sets properties used during the installation and by the windows service.

MainProcessInstaller.cs
Defines the installer class for the MainProcess. Custom installation can be added to this file.

DemoProcess.cs
Defines a demo process that uses a timer and logs information about system resources. 
This process is managed from the MainProcess class.



Installing the Application:
+++++++++++++++++++++++++++

Before running the application the first time it needs to be installed for creating the Log Source, 
the Performance Counters and registering the Windows Service.

After compiling your project install the application from a command prompt running as Administrator.

Install the application executing 
installutil your-application.exe

Uninstall the application executing
installutil /u your-application.exe

The full path for installutil.exe and your-application.exe may be needed depending from where ere the command is being called 
and if installutil.exe is in the system path. 
your-application.exe will be in the bin\Debug or bin\Release folder depending on the active configuration.



Executing the application as a desktop application:
+++++++++++++++++++++++++++++++++++++++++++++++++++

When the application is executed from Visual Studio or any other interactive way the application executes as a desktop application.



Executing the application as a windows service application:
+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

Open Control Pane/Administrative Tools/Services and find your application by the service display name that was set in the property ServiceDisplayName in the MainProcess class.
The service can be started/stopped from that place.


Start development:
++++++++++++++++++

Replace the DemoProcess with you own process/processes and manage the start and stop from the MainProcess. 
Follow all the TODOs in the class MainProcess to customize the application.
