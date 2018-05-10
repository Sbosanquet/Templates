using PeterKottas.DotNetCore.WindowsService;
using System;

namespace WindowsCoreServiceTemplate
{
    internal static class Program
    {
        /*
         * Service template based on https://github.com/PeterKottas/DotNetCore.WindowsService
         *  Run the service without arguments and it runs like console app.
         *  Run the service with action:install and it will install the service.
         *  Run the service with action:uninstall and it will uninstall the service.
         *  Run the service with action:start and it will start the service.
         *  Run the service with action:stop and it will stop the service.
         *  Run the service with username:YOUR_USERNAME, password:YOUR_PASSWORD and action:install which installs it for the given account.
         *  Run the service with built-in-account:(NetworkService|LocalService|LocalSystem) and action:install which installs it for the given built in account. Defaults to LocalSystem.
         *  Run the service with description:YOUR_DESCRIPTION and it setup description for the service.
         *  Run the service with display-name:YOUR_DISPLAY_NAME and it setup Display name for the service.
         *  Run the service with name:YOUR_NAME and it setup name for the service.
         *  Run the service with start-immediately:(true|false) to start service immediately after install. Defaults to true.
        */

        internal static void Main(string[] args)
        {
            ServiceRunner<Service>.Run(config =>
            {
                var name = config.GetDefaultName();
                config.SetName("MyAppService");
                config.SetDescription("An example application");
                config.SetDisplayName("MyApp As A Service");
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((serviceArguments, serviceController) => new Service());

                    serviceConfig.OnStart((service, serviceArguments) =>
                    {
                        Console.WriteLine("Service {0} started", name);
                        if (!(service is null))
                        {
                            service.Start();
                        }
                        else
                        {
                            Console.WriteLine("Service cannot be started as it doesn't exist!!");
                        }
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", name);
                        if (!(service is null))
                        {
                            service.Stop();
                        }
                    });

                    serviceConfig.OnInstall(service =>
                    {
                        Console.WriteLine("Service {0} installed", name);
                    });

                    serviceConfig.OnUnInstall(service =>
                    {
                        Console.WriteLine("Service {0} uninstalled", name);
                    });

                    serviceConfig.OnPause(service =>
                    {
                        Console.WriteLine("Service {0} paused", name);
                        if (!(service is null))
                        {
                            service.Pause();
                        }
                    });

                    serviceConfig.OnContinue(service =>
                    {
                        Console.WriteLine("Service {0} continued", name);
                        if (!(service is null))
                        {
                            service.Continue();
                        }
                    });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });

        }
    }
}
