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
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Reactivity.UI.ControlPanel
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private System.ComponentModel.BackgroundWorker loginBackgroundWorker;
        public LoginWindow()
        {
            InitializeComponent();
            this.loginBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.loginBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(loginBackgroundWorker_DoWork);
            this.loginBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(loginBackgroundWorker_RunWorkerCompleted);
        }

        void loginBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            
            this.passwordBox.Password = "";
            this.loginButton.Content = "Login";
            this.loginButton.IsEnabled = true;
            this.usernameBox.IsEnabled = true;
            this.passwordBox.IsEnabled = true;
            this.serverBox.IsEnabled = true;
            if (e.Result == null)
            {
                this.serverBox.Background = Brushes.Pink;
                this.serverBox.Focus();
            }
            else if ((bool)e.Result)
            {
                this.Hide();
                new MainPanel().ShowDialog();
                Common.Client.UserLogout();
                this.Show();
                this.Focus();
            }
            else
            {
                this.passwordBox.Background = Brushes.Pink;
                this.passwordBox.Focus();
            }
        }

        void loginBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                string[] args = (string[])e.Argument;
                if (Common.Client != null)
                {
                    if (Common.Client.Uri.Trim().ToLower() != args[0].Trim().ToLower())
                    {
                        Common.Client.Close();
                        Common.Client = new Reactivity.UI.Client(args[0]);
                    }
                }
                else
                    Common.Client = new Reactivity.UI.Client(args[0]);

                e.Result = false;
                if (Common.Client.UserLogin(args[1], Util.Hash.ToString(args[2])))
                    if (Common.Client.UserHasAdminPermission)
                    {
                        e.Result = true;
                    }
                    else
                    {
                        Common.Client.UserLogout();
                    }
            }
            catch
            {
                e.Result = null;
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.serverBox.Text.StartsWith("http://"))
            {
                this.serverBox.Background = Brushes.Pink;
                this.serverBox.Focus();
                return;
            }
            if (this.usernameBox.Text == "" || !Util.Validator.IsUsername(this.usernameBox.Text))
            {
                this.usernameBox.Background = Brushes.Pink;
                this.usernameBox.Focus();
                return;
            }
            this.loginButton.IsEnabled = false;
            this.usernameBox.IsEnabled = false;
            this.passwordBox.IsEnabled = false;
            this.serverBox.IsEnabled = false;
            this.loginButton.Content = "Please Wait ...";

            string[] args = new string[3];
            args[0] = this.serverBox.Text;
            args[1] = this.usernameBox.Text;
            args[2] = this.passwordBox.Password;
            this.loginBackgroundWorker.RunWorkerAsync(args);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            this.passwordBox.Background = Brushes.White;
            this.usernameBox.Background = Brushes.White;
            this.serverBox.Background = Brushes.White;
            if(e.Key == Key.Enter)
                loginButton_Click(sender, e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Common.Client != null)
            {
                Common.Client.Close();
                Common.Client = null;
            }
        }

        private void loginWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
