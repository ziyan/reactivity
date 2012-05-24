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
using System.ComponentModel;
using Reactivity.Objects;

namespace Reactivity.UI.ControlPanel.Editors
{
    /// <summary>
    /// Interaction logic for UserEditDialog.xaml
    /// </summary>
    public partial class UserEditDialog : Window
    {
        private bool modification = true;
        private User user = null;
        private BackgroundWorker backgroundWorker;

        public UserEditDialog(User user)
        {
            InitializeComponent();
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            if (user != null)
                this.user = (User)user.Clone();
            else
            {
                this.user = new User { ID = 0, Username = "", Name = "<New User>", Description = "" };
                modification = false;
            }
            //Initialize
            this.UserID.Text = this.user.ID.ToString();
            this.UserName.Text = this.user.Name;
            this.UserUsername.IsReadOnly = modification;
            this.UserDescription.Text = this.user.Description;
            this.UserUsername.Text = this.user.Username;
            this.UserPermissionAdmin.IsChecked = (this.user.Permission & User.PERMISSION_ADMIN) > 0;
            this.UserPermissionSubscribe.IsChecked = (this.user.Permission & User.PERMISSION_SUBSCRIBE) > 0;
            this.UserPermissionControl.IsChecked = (this.user.Permission & User.PERMISSION_CONTROL) > 0;
            this.UserPermissionStats.IsChecked = (this.user.Permission & User.PERMISSION_STATS) > 0;
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
            }
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            User user = (User)e.Argument;
            if (modification)
                e.Result = Common.Client.UserUpdate(user);
            else
                e.Result = Common.Client.UserCreate(user);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.user.Name = UserName.Text;
            this.user.Description = UserDescription.Text;
            if (!modification)
            {
                if (!Util.Validator.IsUsername(UserUsername.Text))
                {
                    MessageBox.Show("Username is in invalid format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.user.Username = UserUsername.Text;
            }
            this.user.Permission = (UserPermissionAdmin.IsChecked == true ? User.PERMISSION_ADMIN : 0)
                | (UserPermissionSubscribe.IsChecked == true ? User.PERMISSION_SUBSCRIBE : 0)
                | (UserPermissionControl.IsChecked == true ? User.PERMISSION_CONTROL : 0)
                | (UserPermissionStats.IsChecked == true ? User.PERMISSION_STATS : 0);
            if (backgroundWorker.IsBusy) return;
            this.IsEnabled = false;
            this.OKButton.Content = "Working";
            backgroundWorker.RunWorkerAsync(user);
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
    }
}
