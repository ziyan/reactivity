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
using System.Windows.Threading;
using System.Xml;
using System.ComponentModel;
using Reactivity.Objects;
using Reactivity.Clients;
using System.Collections.ObjectModel;

namespace Reactivity.UI.ControlPanel
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : Window
    {
        private BackgroundWorker RuleListWorker, UserListWorker, DeviceListWorker, RemovalWorkder;
        private Binding RuleListBinding = null, UserListBinding = null, DeviceListBinding = null, LogListBinding = null;

        public MainPanel()
        {
            InitializeComponent();
            RuleListWorker = new BackgroundWorker();
            RuleListWorker.DoWork += new DoWorkEventHandler(RuleListWorker_DoWork);
            RuleListWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RuleListWorker_RunWorkerCompleted);

            UserListWorker = new BackgroundWorker();
            UserListWorker.DoWork += new DoWorkEventHandler(UserListWorker_DoWork);
            UserListWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UserListWorker_RunWorkerCompleted);

            DeviceListWorker = new BackgroundWorker();
            DeviceListWorker.DoWork += new DoWorkEventHandler(DeviceListWorker_DoWork);
            DeviceListWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DeviceListWorker_RunWorkerCompleted);

            RemovalWorkder = new BackgroundWorker();
            RemovalWorkder.DoWork += new DoWorkEventHandler(RemovalWorkder_DoWork);
            RemovalWorkder.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RemovalWorkder_RunWorkerCompleted);

            LogListBinding = new Binding();
            LogListBinding.Source = Common.Client.Logs;
            LogListView.SetBinding(ListView.ItemsSourceProperty, LogListBinding);
        }

        void RemovalWorkder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void RemovalWorkder_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is Device)
                e.Result = Common.Client.DeviceRemove(((Device)e.Argument).Guid);
            if (e.Argument is Rule)
                e.Result = Common.Client.RuleRemove(((Rule)e.Argument).ID);
            if (e.Argument is User)
                e.Result = Common.Client.UserRemove(((User)e.Argument).ID);
        }

        void DeviceListWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DeviceListBinding = new Binding();
            DeviceListBinding.Source = e.Result;
            DeviceListView.SetBinding(ListView.ItemsSourceProperty, DeviceListBinding);
            ProgressBar.Visibility = Visibility.Hidden;
        }

        void DeviceListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Common.Client.Devices;
        }

        void UserListWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UserListBinding = new Binding();
            UserListBinding.Source = e.Result;
            UserListView.SetBinding(ListView.ItemsSourceProperty, UserListBinding);
            ProgressBar.Visibility = Visibility.Hidden;
        }

        void UserListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Common.Client.Users;
        }

        void RuleListWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RuleListBinding = new Binding();
            RuleListBinding.Source = e.Result;
            RuleListView.SetBinding(ListView.ItemsSourceProperty, RuleListBinding);
            ProgressBar.Visibility = Visibility.Hidden;
        }

        void RuleListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Common.Client.Rules;
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem)
            {
                string header = ((TabItem)MainTabControl.SelectedItem).Header.ToString();
                if (header == "Devices" && DeviceListBinding == null && !DeviceListWorker.IsBusy)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    DeviceListWorker.RunWorkerAsync();
                }
                if (header == "Rules" && RuleListBinding == null && !RuleListWorker.IsBusy)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    RuleListWorker.RunWorkerAsync();
                }
                if (header == "Users" && UserListBinding == null && !UserListWorker.IsBusy)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    UserListWorker.RunWorkerAsync();
                }
            }
        }

        private void NewDeviceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Editors.DeviceEditDialog(null).ShowDialog();
        }

        private void NewUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Editors.UserEditDialog(null).ShowDialog();
        }

        private void UserEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem is User)
                new Editors.UserEditDialog((User)UserListView.SelectedItem).ShowDialog();
        }

        private void UserRemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemovalWorkder.RunWorkerAsync(UserListView.SelectedItem);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
                this.Close();
        }

        private void NewRuleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Editors.RuleEditDialog(null).ShowDialog();
        }

        private void RuleEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (RuleListView.SelectedItem is Rule)
                new Editors.RuleEditDialog((Rule)RuleListView.SelectedItem).ShowDialog();
        }

        private void RuleRemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemovalWorkder.RunWorkerAsync(RuleListView.SelectedItem);
        }

        private void DeviceEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Editors.DeviceEditDialog((Device)DeviceListView.SelectedItem).ShowDialog();
        }

        private void DeviceRemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemovalWorkder.RunWorkerAsync(DeviceListView.SelectedItem);
        }

        private void UserSetPasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem is User)
                new Editors.UserPasswordDialog((User)UserListView.SelectedItem).ShowDialog();
        }

        private void LogClearMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Common.Client.LogsClear();
        }
    }
}
