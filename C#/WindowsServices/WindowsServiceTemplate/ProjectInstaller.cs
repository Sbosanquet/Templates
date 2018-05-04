using System.ComponentModel;
using System.ServiceProcess;

namespace WindowsServiceTemplate
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            // Sets the service process information.  In this case just the account the service will run as
            var process = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };

            //
            // Set up the service information using data from the application settings
            //
            var serviceAdmin = new ServiceInstaller
            {
                StartType = Properties.Settings.Default.ServiceStartupType,
                DelayedAutoStart = Properties.Settings.Default.ServiceDelayedStart,
                ServiceName = Properties.Settings.Default.ServiceName,
                DisplayName = Properties.Settings.Default.ServiceDisplayName,
                Description = Properties.Settings.Default.ServiceDescription
            };
            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
