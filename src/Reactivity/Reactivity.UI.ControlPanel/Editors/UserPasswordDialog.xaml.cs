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
    /// Interaction logic for UserPasswordDialog.xaml
    /// </summary>
    public partial class UserPasswordDialog : Window
    {
        private BackgroundWorker backgroundWorker;
        private int user;

        public UserPasswordDialog(User user)
        {
            InitializeComponent();
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            
            //Initialize
            this.user = user.ID;
            this.UserID.Text = user.ID.ToString();
            this.UserName.Text = user.Name;
            this.UserDescription.Text = user.Description;
            this.UserUsername.Text = user.Username;
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
            object[] args = (object[])e.Argument;
            e.Result = Common.Client.UserSetPassword(Convert.ToInt32(args[0]), args[1].ToString());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if(NewPassword.Password == "")
            {
                MessageBox.Show("Password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (NewPassword.Password != NewPasswordConfirm.Password)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (backgroundWorker.IsBusy) return;
            this.IsEnabled = false;
            this.OKButton.Content = "Working";
            backgroundWorker.RunWorkerAsync(new object[] { user, Util.Hash.ToString(NewPassword.Password) });
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
