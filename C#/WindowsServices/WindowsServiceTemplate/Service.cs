using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceTemplate
{
    public partial class Service : ServiceBase
    {
        #region Helper Declarations

        private enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        #endregion

        private readonly EventLog serviceEventLog;

        public  Service(string Name)
        {
            InitializeComponent();

            this.ServiceName = Name;

            if (!EventLog.SourceExists(Properties.Resources.EventLogServiceSource))
            {
                EventLog.CreateEventSource(Properties.Resources.EventLogServiceSource, Properties.Resources.EventLogName);
            }

            serviceEventLog = new EventLog
            {
                Source = Properties.Resources.EventLogServiceSource
            };
        }

        #region Service Event Handlers

        /// <summary>
        /// Overrides the start event handler. Method is called when the service starts.
        /// </summary>
        /// <param name="args">Startup arguments</param>
        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

#if DEBUG
            WriteEventLogEntry("In OnStart");
#endif

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// Overrides the stop event handler. Method is called when the service stops.
        /// </summary>
        protected override void OnStop()
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

#if DEBUG
            WriteEventLogEntry("In OnStop");
#endif

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// Overrides the pause event handler. Method is called when the service is resumed after being paused. 
        /// It will only be called if the service is create with the PauseAndResume flag has been set.  By
        /// default it isn't.
        /// </summary>
        protected override void OnPause()
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_PAUSE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

#if DEBUG
            WriteEventLogEntry("In OnPause");
#endif

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_PAUSED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            base.OnPause();
        }

        /// <summary>
        /// Overrides the continue event handler. Method is called when the service is resumed after being paused. 
        /// It will only be called if the service is create with the PauseAndResume flag has been set.  By
        /// default it isn't.
        /// </summary>
        protected override void OnContinue()
        {
            // Update the service state to Start Pending.  
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_CONTINUE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

#if DEBUG
            WriteEventLogEntry("In OnContinue.");
#endif

            // Update the service state to Running.  
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// Overrides the power event handler. Method is called when a PC power event is detected, e.g. suspend
        /// </summary>
        /// <param name="powerStatus">The broadcasted power status</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
#if DEBUG
            WriteEventLogEntry("In OnPowerEvent");
#endif

            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// Overrides the shutdown event handler. Method is called when PC is shutting down
        /// </summary>
        protected override void OnShutdown()
        {
#if DEBUG
            WriteEventLogEntry("In OnContinue.");
#endif

            base.OnShutdown();
        }

        #endregion

        #region Event Writing

        /// <summary>
        /// Writes an entry into the event log
        /// </summary>
        /// <param name="Text">Text to write into the event log</param>
        private void WriteEventLogEntry(string Text)
        {
            if (EventLog.SourceExists(Properties.Resources.EventLogServiceSource))
            {
                serviceEventLog.WriteEntry(Text);
            }
        }

        #endregion

    }
}
