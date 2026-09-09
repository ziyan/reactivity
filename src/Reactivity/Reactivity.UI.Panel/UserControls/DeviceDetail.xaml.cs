using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

using Reactivity.Objects;
using Reactivity.Util;

namespace Reactivity.UI.Panel.UserControls
{
    /// <summary>
    /// Interaction logic for DeviceDetail.xaml
    /// </summary>
    public partial class DeviceDetail : UserControl
    {
        private SubscriptionDataSource subscription = null;
        private SubscriptionDataSource ACRelaySubscription = null;
        private BackgroundWorker BuildingImageBackgroundWorker = null;
        private ColorPalette palette = new ColorPalette();
        private Device device;

        private bool acRelayValue = false;

        // Event to request that MainPanel add this device's subscription to main chart
        public event AddDeviceToChartEventHandler AddDeviceToChart;
        public delegate void AddDeviceToChartEventHandler(object sender, AddDeviceToChartEventArgs e);
        public class AddDeviceToChartEventArgs : EventArgs
        {
            public readonly Device device;
            public readonly SubscriptionDataSource subscriptionSource;
            public AddDeviceToChartEventArgs(Device device, SubscriptionDataSource subscriptionSource)
            {
                this.device = device;
                this.subscriptionSource = subscriptionSource;
            }
        }

        public DeviceDetail()
        {
            InitializeComponent();
            timeChart.ShowSourceList = false;
        }

        public DeviceDetail(Device deviceIn)
        {
            InitializeComponent();

            this.device = deviceIn;

            DeviceProfileAdapter deviceAdapter = DeviceProfileAdapter.CreateAdapter(deviceIn.Profile);
            ResourceAdapter buildingAdapater = Common.Client.ResourceIndex;

            if (deviceAdapter != null)
            {
                ACControlRow.Height = new GridLength(0);
                ComputerControlRow.Height = new GridLength(0);

                // Get building image
                BuildingImageBackgroundWorker = new BackgroundWorker();
                BuildingImageBackgroundWorker.WorkerSupportsCancellation = true;
                BuildingImageBackgroundWorker.DoWork += new DoWorkEventHandler(BuildingImageBackgroundWorker_DoWork);
                BuildingImageBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BuildingImageBackgroundWorker_RunWorkerCompleted);
                BuildingImageBackgroundWorker.RunWorkerAsync(deviceAdapter.Building);


                // Default subscription
                timeChart.Visibility = Visibility.Collapsed;
                loading.Visibility = Visibility.Visible;
                loading.Message = "Acquiring Subscription";
                loading.IsIndeterminant = true;

                short services = 0;

                if (deviceIn.Type == DeviceType.AccelerationSensor)
                {
                    services = (Int16)(ServiceType.AccelerationSensor_X & ServiceType.AccelerationSensor_Y & ServiceType.AccelerationSensor_Z);
                }
                else if (deviceIn.Type == DeviceType.ComputerNode)
                {
                    ComputerControlRow.Height = GridLength.Auto; // Show computer control row
                    services = (Int16)(ServiceType.ComputerNode_CPU & ServiceType.ComputerNode_Memory);
                }
                else if (deviceIn.Type == DeviceType.ACNode)
                {
                    ACControlRow.Height = GridLength.Auto; // Show AC control row
                    services = ServiceType.ACNode_Power;
                    
                    ACRelaySubscription = new SubscriptionDataSource(deviceIn, ServiceType.ACNode_Relay);
                    ACRelaySubscription.SubscriptionReady += new EventHandler(ACRelaySubscription_SubscriptionReady);
                    ACRelaySubscription.SubscriptionFailed += new EventHandler(ACRelaySubscription_SubscriptionFailed);
                }

                // Main subscription
                subscription = new SubscriptionDataSource(deviceIn, ServiceType.Default);
                subscription.SubscriptionReady += new EventHandler(subscription_SubscriptionReady);
                subscription.SubscriptionFailed += new EventHandler(subscription_SubscriptionFailed);

                // Set text
                deviceName.Text = (deviceIn.Name == "" ? "<None>" : deviceIn.Name);
                deviceDesc.Text = (deviceIn.Description == "" ? "<None>" : deviceIn.Description);
                deviceModel.Text = deviceIn.Type.ToString();

                // Building info
                Building buildingTemp = Common.Client.ResourceIndex.GetBuilding(deviceAdapter.Building);
                buildingName.Text = buildingTemp.Name;
                buildingLocation.Text = "Latitute: " + buildingTemp.Latitude.ToString() + "\nLongitude: " + buildingTemp.Longitude.ToString() + "\nAltitude: " + buildingTemp.Altitude.ToString(); 
                
                // Floor info
                floorLocation.Text = "Floor " + deviceAdapter.Floor.ToString() + "\nX: " + deviceAdapter.X.ToString() + "\nY: " + deviceAdapter.Y.ToString();
           
                // Chart
                timeChart.SetTitle(deviceIn.Name + " - Live Sensor Data");
                DeviceUnitsString units = new DeviceUnitsString();
                timeChart.SetStaticYLabel(units.FromType(deviceIn.Type));

                // Image of sensor type
                deviceModelImage.Source = new BitmapImage(new Uri(GetResourceFromSensorType(deviceIn.Type), UriKind.Relative));
                
            }
        }

        private void ComputerRowShowHide(bool enable)
        {
            computerControlLabel.Visibility = (enable ? Visibility.Visible : Visibility.Collapsed);
            computerControlButtons.Visibility = (enable ? Visibility.Visible : Visibility.Collapsed);
        }

        
        void ACRelaySubscription_SubscriptionFailed(object sender, EventArgs e)
        {
            ACStatus.Text = "Failed to subscribe to AC Relay";
            ACToggleButton.Visibility = Visibility.Collapsed;
        }

        void ACRelaySubscription_SubscriptionReady(object sender, EventArgs e)
        {
            if (ACRelaySubscription.currentACRelayStatus == true)
            {
                ACStatus.Text = "AC Relay Status: ON ";
                acRelayValue = true;
            }
            else
            {
                ACStatus.Text = "AC Relay Status: OFF ";
                acRelayValue = false;
            }
        }        

        void subscription_SubscriptionFailed(object sender, EventArgs e)
        {
            loading.LoadingVisibility = Visibility.Collapsed;
            loading.Message = "Failed to acquire subscription";
        }

        void subscription_SubscriptionReady(object sender, EventArgs e)
        {
            // Hide the loading bar and show chart
            timeChart.Visibility = Visibility.Visible;
            loading.Visibility = Visibility.Collapsed;
            addToMainChart.Visibility = Visibility.Visible;

            subscription.dataSeries.StrokeColor = palette.colorPalette[0];
            timeChart.AddSeries(subscription.dataSeries);
            deviceName.Text += " (" + subscription.subscription.Status.ToString() + ")";
        }

        void BuildingImageBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                // Get the image from a stream
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = (Stream)e.Result;
                source.EndInit();

                buildingImage.Source = source;
            }
        }

        void BuildingImageBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = null;
            if (e.Argument != null)
                e.Result = UI.Common.Client.ResourceGetCachedStream((Guid)e.Argument);     
        }

        public event EventHandler CloseRequest;
        private void close_Click(object sender, RoutedEventArgs e)
        {
            // Fire event
            CloseRequest(this, null);
            // Cancel worker threads
            subscription.CancelSubscribe();
            if (BuildingImageBackgroundWorker.IsBusy)
                BuildingImageBackgroundWorker.CancelAsync();
            
        }

        private string GetResourceFromSensorType(Guid deviceTypeGuid)
        {
            string relativePath = "../Resources/SensorIcons/";
            if (deviceTypeGuid == DeviceType.TemperatureSensor)
                return relativePath + "temperature.png";
            else if (deviceTypeGuid == DeviceType.LuminositySensor)
                return relativePath + "luminosity.png";
            else if (deviceTypeGuid == DeviceType.ACNode)
                return relativePath + "ac-node.png";
            else if (deviceTypeGuid == DeviceType.MotionSensor)
                return relativePath + "motion.png";
            else if (deviceTypeGuid == DeviceType.RFIDReader)
                return relativePath + "rfid.png";
            else if (deviceTypeGuid == DeviceType.ComputerNode)
                return relativePath + "computer-node.png";
            else if (deviceTypeGuid == DeviceType.AccelerationSensor)
                return relativePath + "acceleration.png";
            else
                return relativePath + "";
        }

        private void addToMainChart_Click(object sender, RoutedEventArgs e)
        {
            if (AddDeviceToChart != null)
                AddDeviceToChart(this, new AddDeviceToChartEventArgs(device, subscription));
            CloseRequest(this, null);
        }

        private void ACToggleButton_Click(object sender, RoutedEventArgs e)
        {
            acRelayValue = !acRelayValue;
            Data data = new Data { Device = this.device.Guid, Service = ServiceType.ACNode_Relay, Type = DataType.Bool };
            Util.DataAdapter.Encode(acRelayValue, data);
            Common.Client.DataSend(data);

            if (acRelayValue == true) ACStatus.Text = "AC Relay Status: ON ";
            else ACStatus.Text = "AC Relay Status: OFF";
        }

        private void logoff_Click(object sender, RoutedEventArgs e)
        {
            Data data = new Data { Device = this.device.Guid, Service = ServiceType.Default, Type = DataType.Short };
            Util.DataAdapter.Encode(1, data);
            Common.Client.DataSend(data);
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            Data data = new Data { Device = this.device.Guid, Service = ServiceType.Default, Type = DataType.Short };
            Util.DataAdapter.Encode(2, data);
            Common.Client.DataSend(data);
        }

        private void suspend_Click(object sender, RoutedEventArgs e)
        {
            Data data = new Data { Device = this.device.Guid, Service = ServiceType.Default, Type = DataType.Short };
            Util.DataAdapter.Encode(3, data);
            Common.Client.DataSend(data);
        }

        private void hibernate_Click(object sender, RoutedEventArgs e)
        {
            Data data = new Data { Device = this.device.Guid, Service = ServiceType.Default, Type = DataType.Short };
            Util.DataAdapter.Encode(4, data);
            Common.Client.DataSend(data);
        }

        private void shutdown_Click(object sender, RoutedEventArgs e)
        {
            Data data = new Data { Device = this.device.Guid, Service = ServiceType.Default, Type = DataType.Short };
            Util.DataAdapter.Encode(6, data);
            Common.Client.DataSend(data);
        }

    }
}
