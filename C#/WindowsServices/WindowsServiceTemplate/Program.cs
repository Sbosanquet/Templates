using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceTemplate
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Console arguments</param>
        internal static void Main(String[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ServiceBase[] ServicesToRun = {
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
                    // Process the arguments passed the the console application and determine what to do
                    ProcessConsoleArguments(args, ServicesToRun);
                }
            }
            else
            {
                // Normal service execution
                ServiceBase.Run(ServicesToRun);
            }
        }

        #region Exception Handling

        /// <summary>
        /// Catches any unhandled exceptions within the application
        /// </summary>
        /// <param name="sender">The sender of he exception</param>
        /// <param name="e">The unhandled exception event argument</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Exception ex = (Exception)e.ExceptionObject;
            Console.WriteLine("Error : {0}", ex.GetBaseException().Message);
            Console.ForegroundColor = oldColor;
        }

        #endregion

        #region Console Argument Handling

        /// <summary>
        /// Run service in interactive mode
        /// </summary>
        /// <param name="servicesToRun">Available services associated with this application</param>
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
        /// Processes the command line arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="servicesToRun">Available services associated with this application</param>
        private static void ProcessConsoleArguments(string[] args, ServiceBase[] servicesToRun)
        {
            try
            {
                // Runs the service as a console app
                if (IsCmdLineArgPresent(args, "console"))
                {
                    // Simulate the services execution
                    RunInteractiveService(servicesToRun);
                    return;
                }

                bool hasCommands = false;
                bool error = false;

                // Install the service
                if (IsCmdLineArgPresent(args, "install"))
                {
                    try
                    {
                        InstallService();
                        hasCommands = true;
                    }
                    catch(System.Security.SecurityException)
                    {
                        error = true;
                    }
                }

                // Start the service
                if (IsCmdLineArgPresent(args, "start"))
                {
                    try
                    {
                        if (GetServiceInstalledStatus(servicesToRun))
                        {
                            StartService(servicesToRun);
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.CommandUninstallServiceMissingError);
                        }

                        hasCommands = true;
                    }
                    catch (System.Security.SecurityException)
                    {
                        error = true;
                    }
                }

                // Stop the service
                if (IsCmdLineArgPresent(args, "stop"))
                {
                    try
                    {
                        if (GetServiceInstalledStatus(servicesToRun))
                        {
                            StopService(servicesToRun);
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.CommandUninstallServiceMissingError);
                        }

                        hasCommands = true;
                    }
                    catch (System.Security.SecurityException)
                    {
                        error = true;
                    }
                }

                // Uninstall the service
                if (IsCmdLineArgPresent(args, "uninstall"))
                {
                    try
                    {
                        if (GetServiceInstalledStatus(servicesToRun))
                        {
                            StopService(servicesToRun);
                            UninstallService();
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.CommandUninstallServiceMissingError);
                        }
                        hasCommands = true;
                    }
                    catch (System.Security.SecurityException)
                    {
                        error = true;
                    }
                }

                // If we don't have commands we print usage message
                if (!hasCommands && !error)
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

        /// <summary>
        /// Helper to check to see if the specified command line argument is there
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="command">Command to process</param>
        private static bool IsCmdLineArgPresent(String[] args, String command)
        {
            if (args == null || args.Length == 0 || String.IsNullOrWhiteSpace(command)) return false;
            return args.Any(a => String.Equals(a, command, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Service Methods

        /// <summary>
        /// Attempts to uninstalls the service
        /// </summary>
        /// /// <exception cref="System.Security.SecurityException">Thrown when the process does not run with admin privileges</exception>
        private static void UninstallService()
        {
            if (IsProcessAdmin())
            {
                // TODO: Need to uninstall the event log

                ManagedInstallerClass.InstallHelper(new String[] { "/u", typeof(Program).Assembly.Location });
            }
            else
            {
                Console.WriteLine(Properties.Resources.CommandAdminElevationError);
                throw new System.Security.SecurityException(Properties.Resources.CommandAdminElevationException);
            }
        }

        /// <summary>
        /// Attempts to installs the service
        /// </summary>
        /// <exception cref="System.Security.SecurityException">Thrown when the process does not run with admin privileges</exception>
        private static void InstallService()
        {
            if (IsProcessAdmin())
            {
                ManagedInstallerClass.InstallHelper(new String[] { typeof(Program).Assembly.Location });
            }
            else
            {
                Console.WriteLine(Properties.Resources.CommandAdminElevationError);
                throw new System.Security.SecurityException(Properties.Resources.CommandAdminElevationException);
            }
        }

        /// <summary>
        /// Start the provided service(s)
        /// </summary>
        /// <param name="servicesToRun">Available services associated with this application</param>
        /// <exception cref="System.Security.SecurityException">Thrown when the process does not run with admin privileges</exception>
        private static void StartService(ServiceBase[] servicesToRun)
        {
            if (IsProcessAdmin())
            {
                foreach (var service in servicesToRun)
                {
                    ServiceController sc = new ServiceController(service.ServiceName);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.CommandAdminElevationError);
                throw new System.Security.SecurityException(Properties.Resources.CommandAdminElevationException);
            }
        }

        /// <summary>
        /// Stop the provided service(s)
        /// </summary>
        /// <param name="servicesToRun">Available services associated with this application</param>
        /// <exception cref="System.Security.SecurityException">Thrown when the process does not run with admin privileges</exception>
        private static void StopService(ServiceBase[] servicesToRun)
        {
            if (IsProcessAdmin())
            {
                foreach (var service in servicesToRun)
                {
                    ServiceController sc = new ServiceController(service.ServiceName);
                    if (sc.Status == ServiceControllerStatus.Running && sc.CanStop)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }
                }
            }
            else
            {
                Console.WriteLine(Properties.Resources.CommandAdminElevationError);
                throw new System.Security.SecurityException(Properties.Resources.CommandAdminElevationException);
            }
        }

        /// <summary>
        /// hecks to see if this process is running with administrative privleges
        /// </summary>
        /// <returns>true if the process is admin otherwise false</returns>
        private static bool IsProcessAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool admin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            return admin;
        }

        /// <summary>
        /// Checks to see if the service is installed
        /// </summary>
        /// <param name="servicesToRun"></param>
        /// <returns>true if the servce is installed otherwise false</returns>
        private static bool GetServiceInstalledStatus(ServiceBase[] servicesToRun)
        {
            bool serviceInstalled = true;

            try
            {
                foreach (var service in servicesToRun)
                {
                    ServiceController sc = new ServiceController(service.ServiceName);
                    if (sc.ServiceName.Length > 0)
                    {

                    }
                }
            }
            catch
            {
                serviceInstalled = false;
            }
            return serviceInstalled;
        }

        #endregion

    }
}
