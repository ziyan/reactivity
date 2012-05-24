using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using Reactivity.API;

namespace Reactivity.Server
{
    class RuleChainThread
    {
        private System.Threading.Thread thread;
        private bool safeToRun;
        private RuleChainAdapter adapter = new RuleChainAdapter();

        public RuleChainThread()
        {
        }

        public void Start()
        {
            safeToRun = true;
            Util.EventLog.WriteEntry(this, "Thread start");
            if (thread == null || !thread.IsAlive)
            {
                thread = new System.Threading.Thread(new System.Threading.ThreadStart(this.Run));
                thread.Start();
            }
        }

        public void Stop()
        {
            Util.EventLog.WriteEntry(this, "Thread stop");
            safeToRun = false;
        }

        public void Join()
        {
            if (thread != null && thread.IsAlive)
                thread.Join();
            Util.EventLog.WriteEntry(this, "Thread joined");
        }

        private void Run()
        {
            List<IRule> modules = RuleChain.Instance.Get();
            while (safeToRun)
            {
                Objects.Data data = RuleChain.Instance.DataDequeue();
                if (data == null) continue;
                // begin batch execution
                adapter.Begin(data);
                for (int i = 0; i < modules.Count; i++)
                {
                    if (!modules[i].Process(data, adapter))
                        break;
                }
                // end batch execution
                adapter.End();
            }
        }
    }
}
