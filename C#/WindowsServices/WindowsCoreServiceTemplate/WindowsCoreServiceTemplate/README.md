# Windows Core Service Template #

This basic template is based on a GitHub library by [Peter Kottas](https://github.com/PeterKottas/DotNetCore.WindowsService).  It allows a .NET core app to support for windows services allowing the app to run as either a console app or service on windows.  By using .NET core it also allows the program to run as a standard console application on linux.

Another handy blog for reference is [here](https://blog.csmac.nz/a-windows-service-on-dotnet/).

## Commands
The following command line arguments can be used:

* Run the service with action:install and it will install the service.
* Run the service with action:uninstall and it will uninstall the service.
* Run the service with action:start and it will start the service.
* Run the service with action:stop and it will stop the service.
* Run the service with username:YOUR_USERNAME, password:YOUR_PASSWORD and action:install which installs it for the given account.
* Run the service with built-in-account:(NetworkService|LocalService|LocalSystem) and action:install which installs it for the given built in account. Defaults to LocalSystem.
* Run the service with description:YOUR_DESCRIPTION and it setup description for the service.
* Run the service with display-name:YOUR_DISPLAY_NAME and it setup Display name for the service.
* Run the service with name:YOUR_NAME and it setup name for the service.
* Run the service with start-immediately:(true|false) to start service immediately after install. Defaults to true.