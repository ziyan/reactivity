using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Reactivity.Nodes.Computer.Service
{
    public partial class ComputerService : ServiceBase
    {
        private ComputerNode node = null;
        public ComputerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (node == null)
                node = new ComputerNode();
            node.Start();
        }

        protected override void OnContinue()
        {
            if (node == null)
                node = new ComputerNode();
            node.Start();
        }

        protected override void OnPause()
        {
            if (node != null)
                node.Stop();
            node = null;
        }

        protected override void OnStop()
        {
            if (node != null)
                node.Stop();
            node = null;
        }

        protected override void OnShutdown()
        {
            if (node != null)
                node.Stop();
            node = null;
        }
        
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (powerStatus == PowerBroadcastStatus.Suspend || powerStatus == PowerBroadcastStatus.QuerySuspend)
            {
                if (node != null)
                    node.Stop();
                node = null;
            }
            else if (powerStatus == PowerBroadcastStatus.ResumeSuspend ||
                powerStatus == PowerBroadcastStatus.ResumeCritical ||
                powerStatus == PowerBroadcastStatus.ResumeAutomatic || 
                powerStatus == PowerBroadcastStatus.QuerySuspendFailed)
            {
                if (node == null)
                    node = new ComputerNode();
                node.Start();
            }
            return true;
        }
    }
}
