using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using Reactivity.Util;
using Reactivity.Objects;
using Reactivity.UI.Collections;

namespace Reactivity.UI.ControlPanel.Editors
{
    /// <summary>
    /// Interaction logic for DeviceEditDialog.xaml
    /// </summary>
    public partial class DeviceEditDialog : Window
    {

        private class Configuration
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private System.Text.RegularExpressions.Regex RegexDeviceTypeWithName = new System.Text.RegularExpressions.Regex(@"[a-zA-Z0-9 ]+\{([0-9abcdef]{8}\-[0-9abcdef]{4}\-[0-9abcdef]{4}\-[0-9abcdef]{4}\-[0-9abcdef]{12})\}");
        private System.Text.RegularExpressions.Regex RegexDeviceType = new System.Text.RegularExpressions.Regex(@"([0-9abcdef]{8}\-[0-9abcdef]{4}\-[0-9abcdef]{4}\-[0-9abcdef]{4}\-[0-9abcdef]{12})");

        private ObservableCollection<Configuration> configurations = new DispatchedObservableCollection<Configuration>();
        private bool modification = true;
        private Device device = null;
        private Util.DeviceProfileAdapter profile;
        private Util.DeviceConfigurationAdapter configuration;
        private Binding BuildingsBinding = null, FloorsBinding = null, ConfigurationListBinding = null;
        private ObservableCollection<Building> buildings = new DispatchedObservableCollection<Building>();
        private ObservableCollection<Floor> floors = new DispatchedObservableCollection<Floor>();
        private BackgroundWorker backgroundWorker, indexBackgroundWorker, resourceBackgroundWorker;

        public DeviceEditDialog(Device device)
        {
            InitializeComponent();

            this.DeviceType.Items.Add(new ComboBoxItem { Content = "Temperature Sensor {" + Util.DeviceType.TemperatureSensor.ToString() + "}" });
            this.DeviceType.Items.Add(new ComboBoxItem { Content = "Luminosity Sensor {" + Util.DeviceType.LuminositySensor.ToString() + "}" });
            this.DeviceType.Items.Add(new ComboBoxItem { Content = "AC Node {" + Util.DeviceType.ACNode.ToString() + "}" });
            this.DeviceType.Items.Add(new ComboBoxItem { Content = "Motion Sensor {" + Util.DeviceType.MotionSensor.ToString() + "}" });
            this.DeviceType.Items.Add(new ComboBoxItem { Content = "RFID Reader {" + Util.DeviceType.RFIDReader.ToString() + "}" });
            this.DeviceType.Items.Add(new ComboBoxItem { Content = "Computer Node {" + Util.DeviceType.ComputerNode.ToString() + "}" });
            this.DeviceType.Items.Add(new ComboBoxItem { Content = "Acceleration Sensor {" + Util.DeviceType.AccelerationSensor.ToString() + "}" });

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            this.indexBackgroundWorker = new BackgroundWorker();
            this.indexBackgroundWorker.DoWork += new DoWorkEventHandler(indexBackgroundWorker_DoWork);
            this.indexBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(indexBackgroundWorker_RunWorkerCompleted);

            this.resourceBackgroundWorker = new BackgroundWorker();
            this.resourceBackgroundWorker.DoWork += new DoWorkEventHandler(resourceBackgroundWorker_DoWork);
            this.resourceBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(resourceBackgroundWorker_RunWorkerCompleted);

            if (device != null)
                this.device = (Device)device.Clone();
            else
            {
                this.device = new Device { Guid = Guid.NewGuid(), 
                    Name = "<New Device>", Description = "", 
                    Type = Guid.Empty, Configuration = "",
                    Profile = "<profile>\n<building id=\"10000000-0000-0000-0000-000000000000\">\n<floor level=\"0\">\n<position x=\"0\" y=\"0\" z=\"0\" />\n</floor></building></profile>", 
                    Status = DeviceStatus.Unknown };
                modification = false;
            }
            //Initialize
            this.profile = Util.DeviceProfileAdapter.CreateAdapter(this.device.Profile);
            this.configuration = Util.DeviceConfigurationAdapter.CreateAdapter(this.device.Configuration);

            this.DeviceGuid.Text = this.device.Guid.ToString();
            this.DeviceGuid.IsReadOnly = modification;
            this.DeviceName.Text = this.device.Name;
            this.DeviceDescription.Text = this.device.Description;
            this.DeviceProfileX.Text = profile.X.ToString();
            this.DeviceProfileY.Text = profile.Y.ToString();
            this.DeviceProfileZ.Text = profile.Z.ToString();
            this.DeviceType.Text = this.device.Type.ToString();

            foreach (string key in configuration.Settings.Keys)
                configurations.Add(new Configuration { Key = key, Value = configuration.Settings[key] });

            // set up bindings
            FloorsBinding = new Binding();
            FloorsBinding.Source = floors;
            DeviceProfileFloor.SetBinding(ComboBox.ItemsSourceProperty, FloorsBinding);

            BuildingsBinding = new Binding();
            BuildingsBinding.Source = buildings;
            DeviceProfileBuilding.SetBinding(ComboBox.ItemsSourceProperty, BuildingsBinding);

            ConfigurationListBinding = new Binding();
            ConfigurationListBinding.Source = configurations;
            ConfigurationListView.SetBinding(ListView.ItemsSourceProperty, ConfigurationListBinding);

            // get index
            this.indexBackgroundWorker.RunWorkerAsync();
        }

        

        void indexBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Util.ResourceAdapter && ((Util.ResourceAdapter)e.Result).IsValid)
            {
                Building[] buildings = ((Util.ResourceAdapter)e.Result).Buildings;
                this.buildings.Clear();
                if (buildings != null)
                    for (int i = 0; i < buildings.Length; i++)
                        this.buildings.Add(buildings[i]);
                if (profile.Building != Guid.Empty)
                    foreach (Building building in this.buildings)
                        if (building.Guid == profile.Building)
                            DeviceProfileBuilding.SelectedItem = building;
            }
        }

        void indexBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Common.Client.ResourceIndex;
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
                this.Close();
            else
            {
                MessageBox.Show("An error has occured, failed to execute request.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.IsEnabled = true;
                this.OKButton.Content = "OK";
                if (!modification)
                {
                    this.device.Guid = Guid.NewGuid();
                    this.DeviceGuid.Text = device.Guid.ToString();
                }
            }
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Device device = (Device)e.Argument;
            if (modification)
                e.Result = Common.Client.DeviceUpdate(device);
            else
                e.Result = Common.Client.DeviceCreate(device);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!modification)
            {
                try
                {
                    this.device.Guid = new Guid(DeviceGuid.Text);
                }
                catch
                {
                    MessageBox.Show("Please specify a valid GUID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            this.device.Name = DeviceName.Text;
            this.device.Description = DeviceDescription.Text;

            if (!(DeviceProfileBuilding.SelectedItem is Building))
            {
                MessageBox.Show("Please select a building.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            profile.Building = ((Building)DeviceProfileBuilding.SelectedItem).Guid;
            if (!(DeviceProfileFloor.SelectedItem is Floor))
            {
                MessageBox.Show("Please select a floor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            profile.Floor = ((Floor)DeviceProfileFloor.SelectedItem).Level;

            try
            {
                profile.X = Convert.ToDouble(DeviceProfileX.Text);
                profile.Y = Convert.ToDouble(DeviceProfileY.Text);
                profile.Z = Convert.ToDouble(DeviceProfileZ.Text);
            }
            catch
            {
                MessageBox.Show("Position X, Y, Z have to be decimals.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.device.Profile = profile.ToString();

            this.configuration.Settings.Clear();
            for (int i = 0; i < configurations.Count; i++)
                this.configuration.Settings[configurations[i].Key] = configurations[i].Value;
            this.device.Configuration = this.configuration.ToString();
            
            if (RegexDeviceTypeWithName.IsMatch(DeviceType.Text))
                this.device.Type = new Guid(RegexDeviceTypeWithName.Replace(DeviceType.Text, "$1"));
            else if (RegexDeviceType.IsMatch(DeviceType.Text))
                this.device.Type = new Guid(RegexDeviceType.Replace(DeviceType.Text, "$1"));
            else
            {
                MessageBox.Show("Device type format incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.device.Type == Guid.Empty)
            {
                MessageBox.Show("Device type format incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (backgroundWorker.IsBusy) return;
            this.IsEnabled = false;
            this.OKButton.Content = "Working";
            backgroundWorker.RunWorkerAsync(device);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if ((!(e.OriginalSource is TextBox) || !((TextBox)e.OriginalSource).AcceptsReturn) &&
                    !(e.OriginalSource is ComboBox))
                    OKButton_Click(sender, e);
            if (e.Key == Key.Escape)
                CancelButton_Click(sender, e);
        }

        private void DeviceProfileBuilding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceProfileBuilding.SelectedItem is Building)
            {
                Building building = (Building)DeviceProfileBuilding.SelectedItem;
                Floor[] floors = building.Floors;
                DeviceProfileFloor.SelectedItem = null;
                this.floors.Clear();
                if (floors != null)
                    for (int i = 0; i < floors.Length; i++)
                        this.floors.Add(floors[i]);
                foreach (Floor floor in this.floors)
                    if (floor.Level == profile.Floor)
                        DeviceProfileFloor.SelectedItem = floor;
                DeviceProfileFloor.IsEnabled = true;
            }
            else
            {
                DeviceProfileFloor.SelectedItem = null;
                DeviceProfileFloor.IsEnabled = false;
                floors.Clear();
            }
        }

        private void DeviceProfileFloor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceProfileViewBox.Child = null;
            if (DeviceProfileFloor.SelectedItem is Floor)
            {
                if(!resourceBackgroundWorker.IsBusy)
                    resourceBackgroundWorker.RunWorkerAsync(((Floor)DeviceProfileFloor.SelectedItem).Resource);
            }
        }

        void resourceBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = (System.IO.Stream)e.Result;
                source.EndInit();
                Image image = new Image();
                image.SizeChanged += new SizeChangedEventHandler(image_SizeChanged);
                image.Source = source;
                DeviceProfileViewBox.Child = image;
                
            }            
        }

        void image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewBox();
        }

        void resourceBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Common.Client.ResourceGetStream((Guid)e.Argument);
        }

        private void UpdateViewBox()
        {
            try
            {
                Point p = new Point(Convert.ToDouble(this.DeviceProfileX.Text), Convert.ToDouble(this.DeviceProfileY.Text));
                if (p.X > 1) p.X = 1;
                if (p.X < -1) p.X = -1;
                if (p.Y > 1) p.Y = 1;
                if (p.Y < -1) p.Y = -1;
                p.X = (p.X + 1) * ((Image)DeviceProfileViewBox.Child).ActualWidth / 2;
                p.Y = (p.Y + 1) * ((Image)DeviceProfileViewBox.Child).ActualHeight / 2;
                this.DeviceProfileViewBoxPointer.Margin =
                    new Thickness(p.X - DeviceProfileViewBoxPointer.Width / 2,
                        p.Y - DeviceProfileViewBoxPointer.Height / 2, 0, 0);
                this.DeviceProfileScrollViewer.ScrollToHorizontalOffset(p.X - DeviceProfileScrollViewer.ActualWidth / 2);
                this.DeviceProfileScrollViewer.ScrollToVerticalOffset(p.Y - DeviceProfileScrollViewer.ActualHeight / 2);                
            }
            catch { }
        }

        private void DeviceProfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateViewBox();
        }
        private void DeviceProfileScrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DeviceProfileViewBox.Child is Image)
            {
                Point p = e.GetPosition(DeviceProfileViewBox.Child);
                p.X = p.X / (((Image)DeviceProfileViewBox.Child).ActualWidth / 2) - 1;
                p.Y = p.Y / (((Image)DeviceProfileViewBox.Child).ActualHeight / 2) - 1;
                if (p.X > 1) p.X = 1;
                if (p.X < -1) p.X = -1;
                if (p.Y > 1) p.Y = 1;
                if (p.Y < -1) p.Y = -1;
                this.DeviceProfileX.Text = p.X.ToString();
                this.DeviceProfileY.Text = p.Y.ToString();
            }
        }

        private void DeviceProfileScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateViewBox();
        }

        private void ConfigurationAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationKey.Text == "")
            {
                MessageBox.Show("Key cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigurationKey.Focus();
                return;
            }
            Configuration newConfig = new Configuration { Key = ConfigurationKey.Text, Value = ConfigurationValue.Text };
            for (int i = 0; i < configurations.Count; i++)
                if (configurations[i].Key == newConfig.Key)
                {
                    MessageBox.Show("Found duplicate key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ConfigurationKey.Focus();
                    return;
                }
            configurations.Add(newConfig);
            ConfigurationListView.SelectedItem = newConfig;
        }

        private void ConfigurationRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationListView.SelectedItem is Configuration)
                configurations.Remove((Configuration)ConfigurationListView.SelectedItem);
        }

        private void ConfigurationSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(ConfigurationListView.SelectedItem is Configuration) || ConfigurationListView.SelectedItems.Count != 1)
                return;
            if (ConfigurationKey.Text == "")
            {
                MessageBox.Show("Key cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigurationKey.Focus();
                return;
            }
            Configuration config = (Configuration)ConfigurationListView.SelectedItem;
            if (config.Key != ConfigurationKey.Text)
            {
                for (int i = 0; i < configurations.Count; i++)
                    if (configurations[i].Key == ConfigurationKey.Text)
                    {
                        MessageBox.Show("Found duplicate key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ConfigurationKey.Focus();
                        return;
                    }
                config.Key = ConfigurationKey.Text;
            }
            config.Value = ConfigurationValue.Text;
            configurations.Remove(config);
            configurations.Insert(0, config);
            ConfigurationListView.SelectedItem = config;
        }

        private void ConfigurationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigurationRemoveButton.IsEnabled = (ConfigurationListView.SelectedItems.Count > 0);
            if (ConfigurationListView.SelectedItems.Count == 1 && ConfigurationListView.SelectedItem is Configuration)
            {
                ConfigurationKey.Text = ((Configuration)ConfigurationListView.SelectedItem).Key;
                ConfigurationValue.Text = ((Configuration)ConfigurationListView.SelectedItem).Value;
                ConfigurationSaveButton.IsEnabled = true;
                return;
            }
            ConfigurationKey.Text = "";
            ConfigurationValue.Text = "";
            ConfigurationSaveButton.IsEnabled = false;
        }
    }
}

