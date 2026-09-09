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
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.UI.Panel.UserControls;
using System.Windows.Media.Animation;

namespace Reactivity.UI.Panel
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainPanel : Window
    {
        
        private BuildingView buildingView = null;
        ResourceAdapter adapter = null;
        Building currentBuilding = null;

        public bool useEffects = true;
        private static readonly Duration liveChartSlideEffectDuration = new Duration(new TimeSpan(0, 0, 0, 0, 600));

        public MainPanel()
        {
            InitializeComponent();
            //RootGrid.Children.Clear();
            adapter = Common.Client.ResourceIndex;

            upperGUIContainer.Visibility = Visibility.Hidden;

            // Setup event handlers
            buildingSwitcher.OnBuildingSwitched += new BuildingSwitcher.BuildingSwitchedEventHandler(buildingSwitcher_OnBuildingSwitched);            
            liveChart.TabClicked += new EventHandler(liveChart_TabClicked);            

        }

        private enum liveChartState { Hidden, FullView };
        private liveChartState currentChartState = liveChartState.Hidden;
        void liveChart_TabClicked(object sender, EventArgs e)
        {
            
            if (useEffects)
            {
                /*
                Storyboard liveChartStory = new Storyboard();
                DoubleAnimation slide = new DoubleAnimation();
                Storyboard.SetTargetName(slide, "liveChart");
                Storyboard.SetTargetProperty(slide, new PropertyPath(UserControls.LiveChart.RenderTransformProperty));

                if (currentChartState == liveChartState.Hidden)
                {
                    // Pull out chart with effect
                    
                    currentChartState = liveChartState.FullView;
                }
                else
                {
                    // Pull in chart with effect                    
                    currentChartState = liveChartState.Hidden;
                }

                liveChartStory.Children.Add(slide);
                this.BeginStoryboard(liveChartStory);
                
            }
            else
            {
                 */
                if (currentChartState == liveChartState.Hidden)
                {
                    // Pull out chart
                    liveChart.RenderTransform = new TranslateTransform(0, 0);
                    currentChartState = liveChartState.FullView;
                }
                else
                {
                    // Pull in chart
                    liveChart.RenderTransform = new TranslateTransform(-liveChart.ActualWidth + 20, 0);
                    currentChartState = liveChartState.Hidden;
                }
            }
        }

        #region EventHandlers
        void buildingView_OnDeviceClicked(object sender, BuildingView.DeviceClickedEventArgs e)
        {            
            if (e.device != null)
            {
                DeviceDetail deviceDetail = new DeviceDetail(e.device);
                deviceDetail.CloseRequest += new EventHandler(deviceDetail_CloseRequest);
                deviceDetail.AddDeviceToChart += new DeviceDetail.AddDeviceToChartEventHandler(deviceDetail_AddDeviceToChart);
                buildingViewContainer.Children.Add(deviceDetail);
                upperGUIContainer.Visibility = Visibility.Collapsed;
                
                /*liveChart.AddDevice(e.device);*/
            }                
        }

        void deviceDetail_AddDeviceToChart(object sender, DeviceDetail.AddDeviceToChartEventArgs e)
        {            
            liveChart.AddSubsciptionSource(e.device, e.subscriptionSource);
        }

        void deviceDetail_CloseRequest(object sender, EventArgs e)
        {
            buildingViewContainer.Children.Remove((DeviceDetail)sender);
            upperGUIContainer.Visibility = Visibility.Visible;
        }

        void buildingSwitcher_OnBuildingSwitched(object sender, BuildingSwitcher.BuildingSwitchedEventArgs e)
        {
            if (e.building != null)
                currentBuilding = ((Building)e.building);
            else
                return;

            // This event was triggered only as the result of refreshing text, ignore
            if (buildingSwitcher.Refreshing == true)
            {                
                buildingSwitcher.Refreshing = false;  // One refresh has occurred, allow new building switches to happen
                return;
            }

            // Remove the old building viewer
            buildingViewContainer.Children.Clear();

            // Clear the chart
            liveChart.timeChart.ClearAllSeries();
            
            // Create a new building viewer and show it
            buildingView = new BuildingView(currentBuilding);
            buildingViewContainer.Children.Add(buildingView);
            buildingView.OnDeviceClicked += new BuildingView.DeviceClickedEventHandler(buildingView_OnDeviceClicked);
            
        }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Change the highlighted floor plan
            if (buildingView != null && (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Return || e.Key == Key.Enter))
            {
                if (e.Key == Key.Down) buildingView.ChangeLevelDown();
                if (e.Key == Key.Up) buildingView.ChangeLevelUp();
                if (e.Key == Key.Return || e.Key == Key.Enter) buildingView.SelectLevel();
            }

            // Escape to exit fullscreen
            if (e.Key == Key.Escape)
            {
                if (Topmost)
                {
                    Topmost = false;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    /*
                    if (Reactivity.UI.Common.Client != null && Reactivity.UI.Common.Client.UserIsLoggedIn)
                    {
                        buildingView.FadeOut();
                        Reactivity.UI.Common.Client.UserLogout();
                        RootGrid.Children.Remove(buildingView);
                        buildingView = null;
                    }
                     */
                }
            }
        }        

        private void Window_StateChanged(object sender, EventArgs e)
        {
            // If maximized, go to fullscreen mode
            if (Topmost != true && this.WindowState == WindowState.Maximized)
            {
                Topmost = true;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Reactivity.UI.Common.Client != null)
            {
                Reactivity.UI.Common.Client.Close();
                Reactivity.UI.Common.Client = null;
            }
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            upperGUIContainer.Visibility = Visibility.Collapsed;
            buildingViewContainer.Visibility = Visibility.Collapsed;
            buildingSelect.Visibility = Visibility.Visible;

            buildingSwitcherLarge.OnBuildingSwitched += new BuildingSwitcher.BuildingSwitchedEventHandler(buildingSwitcherLarge_OnBuildingSwitched);
        }

        void buildingSwitcherLarge_OnBuildingSwitched(object sender, BuildingSwitcher.BuildingSwitchedEventArgs e)
        {
            buildingSelect.Visibility = Visibility.Collapsed;
            buildingViewContainer.Visibility = Visibility.Visible;
            // Other GUI elements will be faded in when floor images are done loading
            LoadBuilding(e.building);
        }

        private void LoadBuilding(Building building)
        {
            if (building == null) return;            

            // Set the current building to the first one in the list
            buildingViewContainer.Children.Clear();
            currentBuilding = building;

            // Changes the smaller building switcher's selected building since it has already been selected            
            buildingSwitcher.ChangeSelectedBuilding(currentBuilding);
            buildingSwitcher.Refreshing = false;  // Let new events be handled

            // Create a building viewer and set up events for device clicks
            buildingView = new BuildingView(currentBuilding);
            buildingViewContainer.Children.Add(buildingView);
            buildingView.OnDeviceClicked += new BuildingView.DeviceClickedEventHandler(buildingView_OnDeviceClicked);
            buildingView.LoadComplete += new EventHandler(buildingView_LoadComplete);
            buildingView.Focus();
            buildingView.FadeIn();
        }

        void buildingView_LoadComplete(object sender, EventArgs e)
        {
            // When a building is done loading, fade the upper gui back in (chart and building switcher)
            this.BeginStoryboard((Storyboard)this.Resources["fadeInUpperGUI"]);
            liveChart.timeChart.ClearAllSeries();
            upperGUIContainer.Visibility = Visibility.Visible;
            buildingViewContainer.Visibility = Visibility.Visible;
        }

    }

}
