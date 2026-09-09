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
using System.IO;
using Reactivity.Objects;
using System.ComponentModel;

namespace Reactivity.UI.Panel.UserControls
{
    /// <summary>
    /// Interaction logic for BuildingSwitcher.xaml
    /// </summary>
    public partial class BuildingSwitcher : UserControl
    {
        private List<Building> allBuildings = new List<Building>();
        public int currentBuildingIndex = 0;
        public bool Refreshing = false;
                
        public event BuildingSwitchedEventHandler OnBuildingSwitched;
        public delegate void BuildingSwitchedEventHandler(object sender, BuildingSwitchedEventArgs e);
        public class BuildingSwitchedEventArgs : EventArgs
        {
            public readonly Building building;
            public BuildingSwitchedEventArgs(Building building)
            {
                this.building = building;
            }           
        }

        public BuildingSwitcher()
        {
            InitializeComponent();                         
        }

        public BuildingSwitcher(Building building)
        {
            if (building != null)
            {
                if( allBuildings.Contains(building) )
                    currentBuildingIndex = allBuildings.IndexOf(building);
            }

        }

        public void ChangeSelectedBuilding(Building building)
        {
            if (building != null)
            {
                if (allBuildings.Contains(building))
                {
                    currentBuildingIndex = allBuildings.IndexOf(building);
                    getBuildingImageAsyc(allBuildings[currentBuildingIndex].Guid);

                    Refreshing = true;
                    buildingSelection.Text = allBuildings[currentBuildingIndex].Name;                    
                }
            }
        }

        private void Initialize(object sender, RoutedEventArgs e)
        {
            if (Common.Client.ResourceIndex.Buildings.Count() > 0)
            {
                //buildingName.Text = "Retrieving Buildings";

                // Populate building list
                currentBuildingIndex = 0;  // Chose the first building as the default
                foreach (Building building in Common.Client.ResourceIndex.Buildings)
                {
                    // Add building to list
                    allBuildings.Add(building);

                    // Add building into the dropdown
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = building.Name;
                    item.Background = Brushes.Transparent;
                    item.MouseEnter += new MouseEventHandler(item_MouseEnter);
                    buildingSelection.Items.Add(item);
                    
                }
                // Retrieve the first building's image and display it
                getBuildingImageAsyc(allBuildings[currentBuildingIndex].Guid);                
            }            
        }

        void item_MouseEnter(object sender, MouseEventArgs e)
        {
            // Search through buildings to find the one selected
            string selectedBuildingName = ((ComboBoxItem)sender).Content.ToString();
            for (int i = 0; i < allBuildings.Count; i++)
            {
                if (allBuildings[i].Name == selectedBuildingName)
                {
                    getBuildingImageAsyc(allBuildings[i].Guid);
                    break;
                }
            }
        }


        public Building getCurrentBuilding()
        {            
            return allBuildings[currentBuildingIndex];
        }

        private void BuildingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            else
            {
                //buildingName.Text = "Error Retrieving Image";
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(updateGUI));
            
        }

        private void BuildingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = null;
            if (e.Argument != null && Common.Client != null)
                e.Result = UI.Common.Client.ResourceGetCachedStream((Guid)e.Argument);
        }

        private void getBuildingImageAsyc(Guid buildingGuid)
        {
            BackgroundWorker BuildingBackgroundWorker = new BackgroundWorker();
            BuildingBackgroundWorker.DoWork += new DoWorkEventHandler(BuildingBackgroundWorker_DoWork);
            BuildingBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BuildingBackgroundWorker_RunWorkerCompleted);
            BuildingBackgroundWorker.RunWorkerAsync(buildingGuid);
        }      

        private void updateGUI()
        {
            if (Common.Client.ResourceIndex.Buildings.Count() > 0)
            {
                if (currentBuildingIndex >= 0 && currentBuildingIndex < Common.Client.ResourceIndex.Buildings.Count()-1)
                {
                    // Update text
                    //buildingName.Text = allBuildings[currentBuildingIndex].Name.ToString();                    
                }
            }
            else
            {                
                //buildingImage.Source = "../Resources/NoBulidingPhoto.png"; // TODO Make photo for this
                //previous.IsEnabled = next.IsEnabled = false;                 
                //buildingName.Text = "No Buildings";
            }
        }

        private void buildingSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                //((ComboBox)sender).SelectedIndex

                // Search through buildings to find the one selected
                string selectedBuildingName = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
                for (int i = 0; i < allBuildings.Count; i++)
                {
                    if (allBuildings[i].Name == selectedBuildingName)
                    {
                        currentBuildingIndex = i;
                        break;
                    }
                }

                getBuildingImageAsyc(allBuildings[currentBuildingIndex].Guid);

                // Notify of a change in building
                if (OnBuildingSwitched != null)
                    OnBuildingSwitched(this, new BuildingSwitchedEventArgs(allBuildings[currentBuildingIndex]));

                buildingSelection.Focusable = false;
            }

        }

    }

}
