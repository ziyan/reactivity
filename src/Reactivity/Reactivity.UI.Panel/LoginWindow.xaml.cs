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

namespace Reactivity.UI.Panel
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        private string UsernameBoxDefault = "";
        private string ServerBoxDefault = "http://localhost/client.svc";
        private System.ComponentModel.BackgroundWorker loginBackgroundWorker;
        public LoginWindow()
        {
            InitializeComponent();

            UsernameBox.Text = UsernameBoxDefault;            
            ServerBox.Text = ServerBoxDefault;
            progress.Visibility = Visibility.Collapsed;
            LoginButton.Visibility = Visibility.Visible;

            // Login background worker
            this.loginBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.loginBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(loginBackgroundWorker_DoWork);
            this.loginBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(loginBackgroundWorker_RunWorkerCompleted);

        }

        void loginBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.progress.Visibility = Visibility.Collapsed;
            LoginButton.Visibility = Visibility.Visible;
            this.PasswordBox.Password = "";
            this.ServerBox.IsEnabled = false;
            this.LoginButton.IsEnabled = true;
            this.UsernameBox.IsEnabled = true;
            this.PasswordBox.IsEnabled = true;
            this.ServerBox.IsEnabled = true;

            if (e.Result == null)
            {
                this.ServerBox.Background = Brushes.Pink;
                this.ServerBox.Focus();
            }
            else if ((bool)e.Result)
            {
                UserLoggedIn();
            }
            else
            {
                this.PasswordBox.Background = Brushes.Pink;                                
                this.PasswordBox.Focus();
            }
        }

        void loginBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {            
            try
            {
                string[] args = (string[])e.Argument;
                if (Reactivity.UI.Common.Client != null)
                {
                    if (Reactivity.UI.Common.Client.Uri.Trim().ToLower() != args[0].Trim().ToLower())
                    {
                        Reactivity.UI.Common.Client.Close();
                        Reactivity.UI.Common.Client = new Reactivity.UI.Client(args[0]);
                    }
                }
                else
                    Reactivity.UI.Common.Client = new Reactivity.UI.Client(args[0]);

                e.Result = Reactivity.UI.Common.Client.UserLogin(args[1], Util.Hash.ToString(args[2]));
            }
            catch
            {
                e.Result = null;
            }
        }

        private void UserLoggedIn()
        {
            this.Hide();
            new MainPanel().ShowDialog();
            Reactivity.UI.Common.Client.UserLogout();
            this.Show();
            this.Focus();
        }

        #region GUI
        private void UsernameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == UsernameBoxDefault) UsernameBox.Text = "";
            UsernameBox.SelectAll();
            UsernameBox.Background = Brushes.White;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox.SelectAll();
            PasswordBox.Background = Brushes.White;
        }

        private void UsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == "") UsernameBox.Text = UsernameBoxDefault;
        }

        private void ServerBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ServerBox.Text == ServerBoxDefault) ServerBox.Text = "";
            ServerBox.Background = Brushes.White;
        }

        private void ServerBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ServerBox.Text == "") ServerBox.Text = ServerBoxDefault;
        }
        #endregion

        public void Submit()
        {
            if (loginBackgroundWorker.IsBusy)
                return;            

            if(!ServerBox.Text.StartsWith("http://"))
            {
                ServerBox.Background = Brushes.Pink;
                ServerBox.Focus();                
                return;
            }

            if (!Util.Validator.IsUsername(UsernameBox.Text))
            {
                UsernameBox.Background = Brushes.Pink;
                UsernameBox.Focus();
                return;
            }

            ServerBox.Background = Brushes.White;
            PasswordBox.Background = Brushes.White;
            UsernameBox.Background = Brushes.White;

            this.ServerBox.IsEnabled = false;
            this.LoginButton.Visibility = Visibility.Collapsed;
            this.UsernameBox.IsEnabled = false;
            this.PasswordBox.IsEnabled = false;
            progress.Visibility = Visibility.Visible;            

            string[] args = new string[3];
            args[0] = this.ServerBox.Text;
            args[1] = this.UsernameBox.Text;
            args[2] = this.PasswordBox.Password;
            this.loginBackgroundWorker.RunWorkerAsync(args);
        }

        private void ServerBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                Submit();
        }

        private void UsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Submit();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Submit();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Submit();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(Common.Client != null)
                Common.Client.Close();
        }


    }
}
