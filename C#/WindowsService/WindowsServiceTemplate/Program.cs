using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceTemplate
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(String[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service(Properties.Settings.Default.ServiceName)
            };


            // In interactive mode ?
            if (Environment.UserInteractive)
            {
                // In debug mode ?
                if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
                {
                    // Simulate the services execution
                    RunInteractiveService(ServicesToRun);
                }
                else
                {
                    try
                    {                      
                        // Runs the service as a console app
                        if (IsCmdLineArgPresent(args, "console"))
                        {
                            // Simulate the services execution
                            RunInteractiveService(ServicesToRun);
                            return;
                        }

                        bool hasCommands = false;

                        // Install the service
                        if (IsCmdLineArgPresent(args, "install"))
                        {
                            InstallService();
                            hasCommands = true;
                        }

                        // Start the service
                        if (IsCmdLineArgPresent(args, "start"))
                        {
                            foreach (var service in ServicesToRun)
                            {
                                ServiceController sc = new ServiceController(service.ServiceName);
                                sc.Start();
                                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                            }
                            hasCommands = true;
                        }

                        // Stop the service
                        if (IsCmdLineArgPresent(args, "stop"))
                        {

                            foreach (var service in ServicesToRun)
                            {
                                ServiceController sc = new ServiceController(service.ServiceName);
                                sc.Stop();
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                            }
                            hasCommands = true;
                        }

                        // Uninstall the service
                        if (IsCmdLineArgPresent(args, "uninstall"))
                        {
                            UninstallService();
                            hasCommands = true;
                        }

                        // If we don't have commands we print usage message
                        if (!hasCommands)
                        {
                            Console.WriteLine("Usage : {0} [command] [command ...]", Environment.GetCommandLineArgs());
                            Console.WriteLine("Commands : ");
                            Console.WriteLine(" - install : Install the services");
                            Console.WriteLine(" - uninstall : Uninstall the services");
                            Console.WriteLine(" - start : Start the service");
                            Console.WriteLine(" - stop : Stop the services");
                            Console.WriteLine(" - console : run the service as a console application");
                        }
                    }
                    catch (Exception ex)
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error : {0}", ex.GetBaseException().Message);
                        Console.ForegroundColor = oldColor;
                    }
                }
            }
            else
            {
                // Normal service execution
                ServiceBase.Run(ServicesToRun);
            }
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        private static void UninstallService()
        {
            ManagedInstallerClass.InstallHelper(new String[] { "/u", typeof(Program).Assembly.Location });
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        private static void InstallService()
        {
            ManagedInstallerClass.InstallHelper(new String[] { typeof(Program).Assembly.Location });
        }

        /// <summary>
        /// Run service in interactive mode
        /// </summary>
        private static void RunInteractiveService(ServiceBase[] servicesToRun)
        {
            Console.WriteLine();
            Console.WriteLine("Start the services in interactive mode.");
            Console.WriteLine();

            // Get the method to invoke on each service to start it
            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);

            // Start services loop
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Starting {0} ... ", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.WriteLine("Started");
            }

            // Waiting the end
            Console.WriteLine();
            Console.WriteLine("Press a key to stop services and finish process...");
            Console.ReadKey();
            Console.WriteLine();

            // Get the method to invoke on each service to stop it
            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Stop loop
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Stopping {0} ... ", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Stopped");
            }

            Console.WriteLine();
            Console.WriteLine("All services are stopped.");

            // Waiting a key press to not return to VS directly
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.Write("=== Press a key to quit ===");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// Helper to check to see if the specified command line argument is there
        /// </summary>
        static bool IsCmdLineArgPresent(String[] args, String command)
        {
            if (args == null || args.Length == 0 || String.IsNullOrWhiteSpace(command)) return false;
            return args.Any(a => String.Equals(a, command, StringComparison.OrdinalIgnoreCase));
        }

        
    }
}
