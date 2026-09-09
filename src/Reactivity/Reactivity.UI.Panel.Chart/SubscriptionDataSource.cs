using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using System.Windows.Media;

namespace Reactivity.UI.Panel.Chart
{
    public class SubscriptionDataSource : LiveDataSource, IDisposable
    {
        private Device device;
        private Subscription subscription;
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
            Name = device.Name;
            Thickness = 3;
            if (device.Type == Util.DeviceType.LuminositySensor)
                Brush = Brushes.White;
            if (device.Type == Util.DeviceType.ACNode)
                Brush = Brushes.Yellow;
            if (device.Type == Util.DeviceType.TemperatureSensor)
                Brush = Brushes.Red;
            if (UI.Common.Client == null) return;
            subscription = UI.Common.Client.Subscribe(device.Guid, new Reactivity.UI.SubscriptionCallback(this.Callback));
        }

        void Callback(Subscription subscription, Data data)
        {
            if (subscription.Status == SubscriptionStatus.Running && data != null)
                Push(data.Timestamp, Util.DataAdapter.GetGraphableValue(data));
        }
    }
}
