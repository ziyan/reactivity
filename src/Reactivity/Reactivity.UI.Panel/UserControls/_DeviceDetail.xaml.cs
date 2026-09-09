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
using Reactivity.Objects;
using Reactivity.Util;
using System.Collections.ObjectModel;

namespace Reactivity.UI.Panel.UserControls
{
    /// <summary>
    /// Interaction logic for _DeviceDetail.xaml
    /// </summary>
    public partial class _DeviceDetail : UserControl
    {
        private BackgroundWorker ServiceListBackgroundWorker;
        private BuildingView parent;
        private Device device;

        public _DeviceDetail(BuildingView parent, Device device)
        {
            this.device = device;
            this.parent = parent;
            InitializeComponent();

            deviceLabel.Text = device.Name;
            generalInfoText.Text = device.Description;
        }

        private void AddService()
        {
            if (device.Guid == null) return;

            Grid serviceGrid = new Grid();
            serviceGrid.HorizontalAlignment = HorizontalAlignment.Left;
            serviceGrid.VerticalAlignment = VerticalAlignment.Top;
            // Create and set up text box
            TextBlock textTemp = new TextBlock();
            textTemp.Text = device.Name + "\n" + device.Description;
            textTemp.FontSize = 12;
            textTemp.TextWrapping = TextWrapping.Wrap;

            Thickness textMargin = new Thickness(60, 0,0,0);
            textTemp.Margin = textMargin;
            textTemp.Foreground = Brushes.White;
            textTemp.HorizontalAlignment = HorizontalAlignment.Left;
            textTemp.VerticalAlignment = VerticalAlignment.Top;

            Image imageTemp = new Image();
            imageTemp.Width = 50;
            imageTemp.Height = 50;
            imageTemp.HorizontalAlignment = HorizontalAlignment.Left;
            imageTemp.VerticalAlignment = VerticalAlignment.Top;
            if (device.Type == Util.DeviceType.LuminositySensor)
                imageTemp.Source = luminosityIcon.Source;
            if (device.Type == Util.DeviceType.ACNode)
                imageTemp.Source = humidityIcon.Source;
            if (device.Type == Util.DeviceType.TemperatureSensor)
                imageTemp.Source = temperatureIcon.Source;

            Border innerBorder = new Border();
            innerBorder.Padding = new Thickness(10);
            innerBorder.BorderBrush = Brushes.White;
            Thickness outlineThickness = new Thickness(3);
            innerBorder.BorderThickness = outlineThickness;
            CornerRadius outlineCorner = new CornerRadius(5);
            innerBorder.CornerRadius = outlineCorner;

            Thickness outlineMargin = new Thickness(3);
            innerBorder.Margin = outlineMargin;

            // Add them into the grid
            serviceGrid.Children.Add(imageTemp);
            serviceGrid.Children.Add(textTemp);
            
            innerBorder.Child = serviceGrid;

            DetailContainer.Children.Add(innerBorder);

            imageTemp.MouseLeftButtonUp += new MouseButtonEventHandler(imageTemp_MouseDown);           
        }

        void imageTemp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            Service service = services[(Image)sender];

            foreach (LiveDataSource source in parent.LiveChart.Sources)
            {
                if (source is SubscriptionDataSource && ((SubscriptionDataSource)source).Service.Guid == service.Guid)
                {
                    parent.LiveChart.RemoveSource(source);
                    return;
                }
            }
            parent.LiveChart.AddSource(new Reactivity.UI.Panel.Chart.SubscriptionDataSource(service));
            */
        }

        //int rowCount = 0;
        //public bool AddService(ServiceType type, string desc, ServiceStatus status)
        //{
        //    RowDefinition detailRowTemp = new RowDefinition();

        //    DetailContainer.RowDefinitions.Add(detailRowTemp);

        //    // Create and set up text box
        //    TextBlock textTemp = new TextBlock();
        //    textTemp.Text = "Type: " + type + "\nDesc: " + desc;           
        //    textTemp.FontSize = 12;
        //    //textTemp.FontWeight = FontWeights.Bold;
        //    Thickness textMargin = new Thickness(0,8,8,8);
        //    textTemp.Margin = textMargin;
        //    textTemp.Foreground = Brushes.White;
        //    // Need to setup text wrap
        //    //textTemp.TextWrapping

        //    // Create and set up image
        //    Image imageTemp = new Image();
        //    imageTemp.Width = 50;
        //    imageTemp.Height = 50;
        //    switch (type)
        //    {
        //        case ServiceType.Luminosity:
        //            imageTemp.Source = luminosityIcon.Source;
        //            break;
        //        case ServiceType.Humidity:
        //            imageTemp.Source = humidityIcon.Source;
        //            break;
        //        case ServiceType.Temperature:
        //            imageTemp.Source = temperatureIcon.Source;
        //            break;
        //        //case ServiceType.Digital:
        //        //    imageTemp.Source = luminosityIcon.Source;
        //        //   break;
        //    }

        //    //<Border BorderBrush="White" Opacity="0.7" BorderThickness="2" CornerRadius="5" Background="#003262" Margin="8,5,8,5" x:Name="innerBorder" />        
        //    Border innerBorder = new Border();
        //    innerBorder.BorderBrush = Brushes.White;
        //    innerBorder.Opacity = 0.7;
        //    Thickness outlineThickness = new Thickness(3);
        //    innerBorder.BorderThickness = outlineThickness;
        //    CornerRadius outlineCorner = new CornerRadius(5);
        //    innerBorder.CornerRadius = outlineCorner;
        //    //Color darkBlue = new Color();
        //    //darkBlue.A = 0xFF;
        //    //darkBlue.R = 0x00;
        //    //darkBlue.G = 0x32;
        //    //darkBlue.B = 0x62;
        //    //SolidColorBrush darkBlueBrush = new SolidColorBrush(darkBlue);
        //    //innerBorder.Background = darkBlueBrush;
        //    Thickness outlineMargin = new Thickness(8,3,8,3);
        //    innerBorder.Margin = outlineMargin;
        //    //innerBorder.Background


        //    // Add them into the grid
        //    DetailContainer.Children.Add(textTemp);
        //    DetailContainer.Children.Add(imageTemp);
        //    DetailContainer.Children.Add(innerBorder);

        //    // Place them into the appropriate row
        //    Grid.SetColumnSpan(innerBorder, 2);
        //    Grid.SetRow(innerBorder, rowCount);       
        //    Grid.SetColumn(imageTemp, 0);
        //    Grid.SetRow(imageTemp, rowCount);
        //    Grid.SetColumn(textTemp, 1);
        //    Grid.SetRow(textTemp, rowCount);


        //    (DetailContainer.RowDefinitions.Last()).MinHeight = 70;
        //    if (rowCount > 0)
        //       this.Height += 70;

        //    rowCount++;

        //    return true;
        //}

        //public void resetList()
        //{
        //    DetailContainer.Children.Clear();
        //    DetailContainer.RowDefinitions.Clear();
        //    DetailContainer.Height = 160;
        //}

    }
}
