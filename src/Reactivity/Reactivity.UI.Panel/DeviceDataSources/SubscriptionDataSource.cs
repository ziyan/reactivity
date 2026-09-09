using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Reactivity.Objects;
using Reactivity.Util;
using System.ComponentModel;

namespace Reactivity.UI.Panel
{
    public class SubscriptionDataSource : LiveDataSource, IDisposable
    {
        public Subscription subscription;        
        public event EventHandler SubscriptionReady;
        public event EventHandler SubscriptionFailed;

        private BackgroundWorker getSubsWorker;
        private Device device;
        public Device Device
        {
            get { return device; }
        }

        public void Dispose()
        {
            if (UI.Common.Client == null) return;
            if (subscription == null) return;
            UI.Common.Client.Unsubscribe(subscription.Guid);
        }

        public SubscriptionDataSource(Device device)
        {            
            this.device = device;
            if (UI.Common.Client == null) return;
            getSubsWorker = new BackgroundWorker();
            getSubsWorker.WorkerSupportsCancellation = true;
            getSubsWorker.DoWork += new DoWorkEventHandler(getSubsWorker_DoWork);
            getSubsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getSubsWorker_RunWorkerCompleted);
            getSubsWorker.RunWorkerAsync();
        }

        public SubscriptionDataSource(Device device, short serviceType)
        {
            this.device = device;
            this.serviceType = serviceType;
            if (UI.Common.Client == null) return;
            getSubsWorker = new BackgroundWorker();
            getSubsWorker.WorkerSupportsCancellation = true;
            getSubsWorker.DoWork += new DoWorkEventHandler(getSubsWorker_DoWork);
            getSubsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getSubsWorker_RunWorkerCompleted);
            getSubsWorker.RunWorkerAsync();
        }

        public void CancelSubscribe()
        {
            if( getSubsWorker.IsBusy )
                getSubsWorker.CancelAsync();
        }

        void getSubsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                if (SubscriptionFailed != null)
                    SubscriptionFailed(this, null);
            }
            else
            {
                if (SubscriptionReady != null)
                    SubscriptionReady(this, null);
            }
        }

        void getSubsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = null;
            subscription = Common.Client.Subscribe(device.Guid, serviceType, new SubscriptionCallback(this.Callback));
            if (subscription != null)
                e.Result = true;
        }

        void Callback(Subscription subscription, Data data)
        {
            if (subscription.Status == SubscriptionStatus.Running && data != null)
                Push(data.Timestamp, Util.DataAdapter.GetGraphableValue(data));
        }

    }
}
