# FileMonitor
FileMonitorPInvoke is a open-source C# application that makes use of the Windows API functions, like FindFirstChangeNotification, FindNextChangeNotification, and ReadDirectoryChangesW, to track file changes in a specified directory. It allows you to monitor file additions, deletions, and modifications. The project is licensed under the permissive MIT License, allowing everyone to freely use, modify, and include the code in their projects, as long as they credit the original author. 
<p align="center">
  <img src="https://github.com/AlexRasch/FileMonitorPInvoke/assets/46262688/3062d0c5-5c95-4121-a022-30fc050dd0e7" width="550">
</p>

**Disclaimer:** 
While FileMonitorPInvoke provides a way to track file changes using Windows API functions, you may also consider using the built-in [FileSystemWatcher](https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher) class in C#. FileSystemWatcher offers a more feature-rich and managed solution with simplified event-driven file monitoring, which may better suit your needs. ¯\_(ツ)_/¯

## Inspiration
The application is loosely based on the example from Microsoft's documentation [Obtaining Directory Change Notifications](https://learn.microsoft.com/en-us/windows/win32/fileio/obtaining-directory-change-notifications), providing a reliable and simplified solution for file monitoring.

## Special Thanks
A heartfelt special thanks to [Mario](https://github.com/mariob) for his valuable inputs and generous support.
