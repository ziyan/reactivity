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

namespace Reactivity.UI.Panel.UserControls
{
    /// <summary>
    /// Interaction logic for LoadingWidget.xaml
    /// </summary>
    public partial class LoadingWidget : UserControl
    {
        public LoadingWidget()
        {
            InitializeComponent();
        }
        public bool IsIndeterminant
        {
            get { return loadingBar.IsIndeterminate; }
            set { loadingBar.IsIndeterminate = value; }
        }
        public string Message
        {
            get { return message.Text; }
            set { message.Text = value; }
        }
        public double Progress
        {
            get { return loadingBar.Value; }
            set { loadingBar.Value = value; }
        }
        public Visibility MessageVisibility
        {
            get { return message.Visibility; }
            set { message.Visibility = value; }
        }
        public Visibility LoadingVisibility
        {
            get { return loadingBar.Visibility; }
            set { loadingBar.Visibility = value; }
        }
    }
}
