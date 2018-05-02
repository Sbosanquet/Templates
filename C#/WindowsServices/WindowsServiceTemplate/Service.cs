﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace WindowsServiceTemplate
{
    public partial class Service : ServiceBase
    {
        //private System.Diagnostics.EventLog eventLog1;

        public Service(string Name)
            : this()
        {
            this.ServiceName = Name;
        }

        public Service()
        {
            InitializeComponent();

            //eventLog1 = new EventLog();
            //if (!EventLog.SourceExists("MySource"))
            //{
            //    EventLog.CreateEventSource("MySource", "MyNewLog");
            //}
            //eventLog1.Source = "MySource";
            //eventLog1.Log = "MyNewLog";

        }

        protected override void OnStart(string[] args)
        {
            //eventLog1.WriteEntry("In OnStart");
        }

        protected override void OnStop()
        {
            //eventLog1.WriteEntry("In OnStop");
        }
    }
}