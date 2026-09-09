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
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using Reactivity.Objects;
using Reactivity.UI.Panel.Swordfish;
using Reactivity.Util;


namespace Reactivity.UI.Panel.UserControls
{
    /// <summary>
    /// Interaction logic for BuildingView.xaml
    /// </summary>
    public partial class BuildingView : UserControl
    {
        private Building building = null;
        private Dictionary<int, Image> floorImages = new Dictionary<int, Image>();
        private Dictionary<int, Viewport2DVisual3D> floor3DVisuals = new Dictionary<int, Viewport2DVisual3D>();
        private Dictionary<Button, Device> devices = new Dictionary<Button, Device>();
        private Dictionary<Device, Tools3D.InteractiveSphere> spheres = new Dictionary<Device, Reactivity.UI.Tools3D.InteractiveSphere>();

        private static Random random = new Random();        // For randomizing fluctuations

        private List<SolidColorBrush> brushPallette = new List<SolidColorBrush>();

        private BackgroundWorker DeviceListBackgroundWorker;

        private int selectedFloorIndex = 0;

        private static readonly double floorSelectedOpacity = 0.9;
        private static readonly double floorDeselectedOpacity = 0.25;
        private static readonly double floorZSpacing = 0.2;

        private static readonly bool useEffects = true;
        private static readonly Duration floorSwitchEffectDuration = new Duration(new TimeSpan(0, 0, 0, 0, 400));

        private Point mouseDownPoint;
        private Point3D initCameraPos;
        private static readonly double cameraRadius = 3.0;
        private double cameraAngle = 45 *  Math.PI / 180;  // Initial camera angle is at 45 degrees relative to the XY plane        
        private double cameraOffset = cameraRadius * Math.Tan(20 * Math.PI / 180);  // Offset from the floor's Z level. Determines angle and which the XY plane is viewed
        private double cameraFOV = 50;

        private enum viewerState { FullView, SingleFloor, SingleFloorZoomed };
        private viewerState state = viewerState.FullView;

        #region Events
        public event DeviceClickedEventHandler OnDeviceClicked;
        public delegate void DeviceClickedEventHandler(object sender, DeviceClickedEventArgs e);
        public class DeviceClickedEventArgs : EventArgs
        {
            public readonly Device device;
            public DeviceClickedEventArgs(Device device)
            {
                this.device = device;
            }
        }

        public event EventHandler LoadComplete;
        #endregion

        public BuildingView(Building buildingIn)
        {            
            building = buildingIn;
            InitializeComponent();          
            initPallette();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            LoadProgress.Message = "Loading Floor Images for " + building.Name;
            changeCameraPositionWithEffects(cameraAngle, cameraRadius, 0);     
            foreach (Floor floor in building.Floors)
            {
                BackgroundWorker FloorImagesBackgroundWorker = new BackgroundWorker();
                FloorImagesBackgroundWorker.DoWork += new DoWorkEventHandler(FloorImagesBackgroundWorker_DoWork);
                FloorImagesBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FloorImagesBackgroundWorker_RunWorkerCompleted);
                FloorImagesBackgroundWorker.RunWorkerAsync(floor.Resource);           
            }            
        }

        void FloorImagesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if( e.Argument != null )
                e.Result = UI.Common.Client.ResourceGetCachedStream((Guid)e.Argument);            
        }

        int curFloorIndex = 0;
        void FloorImagesBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if( e.Result != null)
            {
                // Get the image from a stream
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = (Stream)e.Result;
                source.EndInit();

                // Create a new image and store it in a dictionary
                Image floorImage = new Image();
                floorImage.Source = source;
                if (curFloorIndex == 0)
                    floorImage.Opacity = floorSelectedOpacity;
                else
                    floorImage.Opacity = floorDeselectedOpacity;
                floorImages.Add(curFloorIndex, floorImage); // Keep a list of floor index to floor image                

                Viewport2DVisual3D floorVisual = createFloorMesh(curFloorIndex);
                floor3DVisuals.Add(curFloorIndex, floorVisual);

                // Add the actual floor to the 3D Viewport
                AddFloor(floorImage, floorVisual, curFloorIndex);
                curFloorIndex++;

                // Show progress
                LoadProgress.Progress = 100.0 * ((double)(curFloorIndex+1) / building.Floors.Count());

                // If this is the last floor, do some final GUI things
                if (curFloorIndex == building.Floors.Count())
                {
                    LoadProgress.Visibility = Visibility.Collapsed;
                    ChangeLevel(0);
                    LoadAllDevices();
                    if (LoadComplete != null) LoadComplete(this, null);
                    //EnableImageRollovers(true);
                }
            }
        }

        private void EnableImageRollovers(bool enable)
        {
            if (enable)
            {
                foreach (Image image in floorImages.Values)
                {
                    image.MouseEnter += new MouseEventHandler(floorImage_MouseEnter);
                    image.MouseLeave += new MouseEventHandler(floorImage_MouseLeave);
                    image.MouseLeftButtonUp += new MouseButtonEventHandler(floorImage_MouseLeftButtonUp);
                }
            }
            else
            {
                foreach (Image image in floorImages.Values)
                {
                    image.MouseEnter -= new MouseEventHandler(floorImage_MouseEnter);
                    image.MouseLeave -= new MouseEventHandler(floorImage_MouseLeave);
                    image.MouseLeftButtonUp -= new MouseButtonEventHandler(floorImage_MouseLeftButtonUp);
                }
            }
        }

        private void LoadAllDevices()
        {
            LoadProgress.Visibility = Visibility.Visible;
            LoadProgress.Message = "Loading Devices";
            DeviceListBackgroundWorker = new BackgroundWorker();
            DeviceListBackgroundWorker.WorkerSupportsCancellation = true;
            DeviceListBackgroundWorker.DoWork += new DoWorkEventHandler(DeviceListBackgroundWorker_DoWork);
            DeviceListBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DeviceListBackgroundWorker_RunWorkerCompleted);
            DeviceListBackgroundWorker.RunWorkerAsync();
        }

        void DeviceListBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null) return;
            double counter = 0;
            foreach (Device device in (ObservableCollection<Device>)e.Result)
            {
                LoadProgress.Progress = 100.0 * (counter / ((ObservableCollection<Device>)e.Result).Count);
                AddDevice(device);
                counter++;
            }
            LoadProgress.Visibility = Visibility.Collapsed;
        }

        void DeviceListBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = null;
            if(Common.Client!=null)
                e.Result = Common.Client.Devices;
        }

        #region Device/Sphere Manipulation
        private void AddDevice(Device device)
        {
            DeviceProfileAdapter adapter = DeviceProfileAdapter.CreateAdapter(device.Profile);            
            if (!adapter.IsValid) return;
            if (adapter.Building != building.Guid) return;  // Device must belong to this building

            // Create the sphere
            Button button = new Button();
            if (device.Type == DeviceType.ACNode)
                button.Foreground = Brushes.Orange;
            devices[button] = device;

            AddSphere(button, device.Type, adapter.X, adapter.Y, (adapter.Floor - 1) * floorZSpacing + floorZSpacing * 0.3);
        }

        private void RemoveDevice(Device device)
        {
            RemoveSphere(device);
        }

        private void RemoveAllDevices()
        {
            devices.Clear();
            RemoveAllSpheres();
        }
        

        private void AddSphere(Button button, Guid type, double x, double y, double z)
        {
            Tools3D.InteractiveSphere sphere = new Reactivity.UI.Tools3D.InteractiveSphere();
            sphere.IsBackVisible = true;
            sphere.Transform = new System.Windows.Media.Media3D.TranslateTransform3D(x, y, z);
            sphere.Visual = button;
            if (type == DeviceType.TemperatureSensor)
                button.Template = (ControlTemplate)Resources["TemperatureButton"];
            else if (type == DeviceType.LuminositySensor)
                button.Template = (ControlTemplate)Resources["LuminosityButton"];
            else if (type == DeviceType.ACNode)
                button.Template = (ControlTemplate)Resources["ACButton"];
            else if (type == DeviceType.MotionSensor)
                button.Template = (ControlTemplate)Resources["MotionButton"];
            else if (type == DeviceType.RFIDReader)
                button.Template = (ControlTemplate)Resources["RFIDButton"];
            else if (type == DeviceType.ComputerNode)
                button.Template = (ControlTemplate)Resources["ComputerButton"];
            else if (type == DeviceType.AccelerationSensor)
                button.Template = (ControlTemplate)Resources["AccelerationButton"];
            else
                button.Template = (ControlTemplate)Resources["TemperatureButton"];

            /*
            <Rectangle x:Name="rec" Width="100" Height="100" Fill="#b55230" />
            <ControlTemplate.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter TargetName="rec" Property="Fill" Value="#f3fe3d" />
                </Trigger>
            </ControlTemplate.Triggers>
           
            Rectangle rec = new Rectangle();
            rec.Width = 100;
            rec.Height = 100;
            rec.Fill = Brushes.Orange;

            ControlTemplate sphereTemplate = new ControlTemplate(typeof(Button));
            sphereTemplate.RegisterName(
            Trigger mouseover = new Trigger();
            mouseover.Property = Button.IsMouseOverProperty;
            mouseover.Value = true;
            Setter colorSetter = new Setter(
            sphereTemplate.Triggers;
             */

            // Actions
            button.Click += new RoutedEventHandler(DeviceButton_Click);
            button.MouseDoubleClick += new MouseButtonEventHandler(DeviceButton_MouseDoubleClick);

            port3d.Children.Add(sphere);
            //spheres.Add(curFloorIndex ? , sphere);
        }

        private void RemoveSphere(Device device)
        {
            port3d.Children.Remove(spheres[device]);
        }

        private void RemoveAllSpheres()
        {
            foreach (Tools3D.InteractiveSphere sphere in spheres.Values)
                port3d.Children.Remove(sphere);
        }

        #endregion

        #region Floor Manipulation
        private void AddFloor(Image imageIn, Viewport2DVisual3D mesh, int level)
        {
            // Assign the image to the 3D Visual
            // Disconnect old visual just in case there is one
            mesh.Visual = imageIn;

            // Add the floor into the viewport
            port3d.Children.Add(mesh);
            
        }

        private Viewport2DVisual3D createFloorMesh(int level)
        {
            #region ReferenceXAML
            /*
             * Reference XAML
            <Viewport2DVisual3D>
                <Viewport2DVisual3D.Geometry>
                    <MeshGeometry3D Positions="-1,1,0 -1,-1,0 1,-1,0 1,1,0"
                            TextureCoordinates="0,0 0,1 1,1 1,0" TriangleIndices="0 1 2 0 2 3"/>
                </Viewport2DVisual3D.Geometry>
                <Viewport2DVisual3D.Material>
                    <DiffuseMaterial Viewport2DVisual3D.IsVisualHostMaterial="True" Brush="White"/>
                </Viewport2DVisual3D.Material>
                <StackPanel>
                    <Image Source="../Resources/clouds_wheat_cc.jpg" Opacity="0.5"/>
                </StackPanel>
                <Viewport2DVisual3D.Visual>
                    <Image Source="../Resources/clouds_wheat_cc.jpg" Opacity="0.5"/>
                </Viewport2DVisual3D.Visual>             
            </Viewport2DVisual3D>

             * Old (interactive) code
            <Tools3D:InteractiveVisual3D Geometry="{StaticResource PlaneMesh2}" IsBackVisible="True" >
                <Tools3D:InteractiveVisual3D.Visual>
                    <StackPanel>
                        <Image x:Name="Level2" Source="/Resources/009-2b.png" 
                               Opacity="0.2"></Image>
                    </StackPanel>
                </Tools3D:InteractiveVisual3D.Visual>
            </Tools3D:InteractiveVisual3D>
                    */
            #endregion

            double floorZCoord = level * floorZSpacing;

            Viewport2DVisual3D floor = new Viewport2DVisual3D();

            // Create a plane mesh
            MeshGeometry3D planeMesh = new MeshGeometry3D();
            planeMesh.Positions = new Point3DCollection();
            planeMesh.Positions.Add(new Point3D(-1, 1, floorZCoord));
            planeMesh.Positions.Add(new Point3D(-1, -1, floorZCoord));
            planeMesh.Positions.Add(new Point3D(1, -1, floorZCoord));
            planeMesh.Positions.Add(new Point3D(1, 1, floorZCoord));

            planeMesh.TextureCoordinates.Add(new Point(0, 0));
            planeMesh.TextureCoordinates.Add(new Point(0, 1));
            planeMesh.TextureCoordinates.Add(new Point(1, 1));
            planeMesh.TextureCoordinates.Add(new Point(1, 0));

            planeMesh.TriangleIndices.Add(0);
            planeMesh.TriangleIndices.Add(1);
            planeMesh.TriangleIndices.Add(2);
            planeMesh.TriangleIndices.Add(0);
            planeMesh.TriangleIndices.Add(2);
            planeMesh.TriangleIndices.Add(3);

            floor.Geometry = planeMesh;

            // Create a white material which serves as a virtual host to the image
            floor.Material = new DiffuseMaterial(Brushes.White);
            Viewport2DVisual3D.SetIsVisualHostMaterial(floor.Material, true);

            return floor;
        }

        private void RemoveFloor(int floorIndex)
        {
            port3d.Children.Remove(floor3DVisuals[floorIndex]);            
        }

        private void RemoveAllFloors()
        {
            foreach (Viewport2DVisual3D visual in floor3DVisuals.Values)
                port3d.Children.Remove(visual);
        }
        #endregion

        void floorImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                //floorImages.Values.Contains((Image)sender));
                int newSelectedFloor = floorImages.Values.ToList().IndexOf((Image)sender);
                state = viewerState.SingleFloor;
                fullView.Visibility = Visibility.Visible;  // Allow user to see Full View button to get back to regular building view
                ChangeLevel(newSelectedFloor);
            }
            else if (e.ClickCount > 2)
            {
                /*
                DeviceProfileAdapter adapter = DeviceProfileAdapter.CreateAdapter(devices[(Button)sender].Profile);
                if (!adapter.IsValid) return;

                zoom = zoomStates.ZoomedOnDevice;
                Point zoomPoint = new Point(adapter.X, adapter.Y);
                camera.Position = new Point3D(camera.Position.X + zoomPoint.X, camera.Position.Y + zoomPoint.Y, camera.Position.Z);
                camera.FieldOfView = 20;
                 */
            }
        }


        private void fullView_Click(object sender, RoutedEventArgs e)
        {
            state = viewerState.FullView;
            fullView.Visibility = Visibility.Collapsed;
            ShowAllFloors();
            
        }

        private void ShowAllFloors()
        {
            RemoveAllFloors();
            changeCameraPosition(cameraAngle, cameraRadius);
            int i = 0;
            foreach (Floor floor in building.Floors)
            {
                AddFloor(floorImages[i], floor3DVisuals[i], i);
                i++;
            }
            // Re-add spheres
            foreach (Button button in devices.Keys)
                button.Visibility = Visibility.Visible;
        }


        void floorImage_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = floorDeselectedOpacity;
            floorImages[selectedFloorIndex].Opacity = floorSelectedOpacity;
        }

        void floorImage_MouseEnter(object sender, MouseEventArgs e)
        {
            floorImages[selectedFloorIndex].Opacity = floorDeselectedOpacity;
            ((Image)sender).Opacity = floorSelectedOpacity;
        }

        void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            // Notify MainPanel that a device was clicked, and give information about the chart source
            if (OnDeviceClicked != null)            
                OnDeviceClicked(this, new DeviceClickedEventArgs(devices[(Button)sender]));            
        }

        private void DeviceButton_MouseDoubleClick(object sender, RoutedEventArgs e)
        {            
            DeviceProfileAdapter adapter = DeviceProfileAdapter.CreateAdapter(devices[(Button)sender].Profile);
            if (!adapter.IsValid) return;

            state = viewerState.SingleFloorZoomed;
            Point zoomPoint = new Point(adapter.X, adapter.Y);
            camera.Position = new Point3D(camera.Position.X + zoomPoint.X, camera.Position.Y + zoomPoint.Y, camera.Position.Z);
            camera.FieldOfView = 20;
        }

        private void initPallette()
        {                        
            byte[] ColorGroups = new byte[] {   146, 206, 0,    // Lime green
                                                143, 51, 201,   // Violet
                                                181, 82, 48,    // Red
                                                229, 174, 20,   // Orange
                                                47, 170, 255,   // Bright blue
                                                243, 255, 61    // Yellow
                                            };
            Color colorTemp = new Color();
            colorTemp.A = 255;  // Set alpha channel of color to fully opaque
            for (int i = 0; i < (ColorGroups.Length / 3); i++)
            {
                colorTemp.R = ColorGroups[i * 3];
                colorTemp.G = ColorGroups[i * 3 + 1];
                colorTemp.B = ColorGroups[i * 3 + 2];
                SolidColorBrush brushTemp = new SolidColorBrush(colorTemp);
                brushPallette.Add(brushTemp);
            }
        }

        public void FadeIn()
        {
            this.BeginStoryboard((Storyboard)this.Resources["FadeIn"]);
            BuildingViewGrid.Visibility = Visibility.Visible;
        }

        public void FadeOut()
        {
            System.Windows.Media.Animation.Storyboard fadeOutStoryboard = (System.Windows.Media.Animation.Storyboard)this.Resources["FadeOut"];
            fadeOutStoryboard.Completed += new EventHandler(fadeOutStoryboard_Completed);
            this.BeginStoryboard(fadeOutStoryboard);
        }        

        void fadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            BuildingViewGrid.Visibility = Visibility.Collapsed;
        }

        #region Floor Related
        public int GetNumFloors()
        {
            return building.Floors.Count();
        }
       
        public void ChangeLevelDown()
        {
            if( selectedFloorIndex > 0 )
                ChangeLevel(--selectedFloorIndex);
        }

        public void ChangeLevelUp()
        {
            if( selectedFloorIndex < GetNumFloors() - 1 )
                ChangeLevel(++selectedFloorIndex);
        }

        public void SelectLevel()
        {
            if (state == viewerState.FullView)
            {
                state = viewerState.SingleFloor;
            }
            else if (state == viewerState.SingleFloor)
            {
                state = viewerState.FullView;
                ShowAllFloors();
            }
            ChangeLevel(selectedFloorIndex);
        }
        
        public void ChangeLevel(int level)
        {
            selectedFloorIndex = level;

            if (state == viewerState.FullView)
            {
                // Change camera position to make the selected level within view
                camera.FieldOfView = cameraFOV;
                changeCameraPositionWithEffects(cameraAngle, cameraRadius, selectedFloorIndex);

                // Lower opacity on all floor but the one selected
                foreach (Image floorImage in floorImages.Values)
                    floorImage.Opacity = floorDeselectedOpacity;
                floorImages[level].Opacity = floorSelectedOpacity;  

                // Remove the Full View button
                fullView.Visibility = Visibility.Collapsed;
            }
            else
            {
                // With Effects
                if (useEffects)
                {
                    // Disable the rollover effect
                    //EnableImageRollovers(false);

                    // Smoothly transistion camera view to the selected floor
                    //changeCameraPositionWithEffects(cameraAngle, cameraRadius, selectedFloorIndex);
                    changeCameraPosition(cameraAngle, cameraRadius);

                    // Completely fade out all floors but the one selected
                    foreach (Image floorImage in floorImages.Values)
                        floorImage.Opacity = floorDeselectedOpacity;
                    floorImages[level].Opacity = floorSelectedOpacity;

                    // Delete all floors and hide spheres (devices) not on the current floor
                    RemoveAllFloors();
                    for( int i = 0; i < devices.Count(); i++)
                    { 
                        DeviceProfileAdapter adapter = DeviceProfileAdapter.CreateAdapter(devices.Values.ToList()[i].Profile);
                        if (adapter.Floor - 1 != level)
                            devices.Keys.ToList()[i].Visibility = Visibility.Collapsed;
                    }
                        
                    // Create floor and devices specific to selected floor
                    AddFloor(floorImages[selectedFloorIndex], floor3DVisuals[selectedFloorIndex], selectedFloorIndex);

                    // Re-enable rollovers
                    //EnableImageRollovers(true);

                    // Show the Full View button
                    fullView.Visibility = Visibility.Visible;
                }
                else
                {
                    // Change camera position to make the selected level within view
                    //camera.FieldOfView = cameraFOV;
                    //changeCameraPositionWithEffects(cameraAngle, cameraRadius, selectedFloorIndex);
                }
            }
        }
        #endregion

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                mouseDownPoint = e.GetPosition(this);
                initCameraPos = camera.Position;
            }

        }
        
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {           
            if( state != viewerState.SingleFloorZoomed )  // Only rotate if not zoomed in on a device
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    Point mouseUpPoint = e.GetPosition(this);
                    double deltaX = mouseDownPoint.X - mouseUpPoint.X;
                    double deltaAngle = deltaX / this.ActualWidth * Math.PI;
                    
                    double initAngle = Math.Atan2(initCameraPos.Y, initCameraPos.X);

                    changeCameraPosition(initAngle + deltaAngle, cameraRadius);
                   
                    // Store the latest camera angle globally (used for resetting the view)
                    cameraAngle = Math.Atan2(camera.Position.Y, camera.Position.X);
                }
            }
        }
        
        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Set zoom state, zoom out, and go back to default camera position for this floor
            state = viewerState.FullView;
            camera.FieldOfView = 50;
            //changeCameraPositionWithEffects(cameraAngle, 3.0);
            changeCameraPosition(cameraAngle, 3.0);
        }

        // Changes the camera's position based on an XY plane angle, and keeps the Z coordinate the same
        // This constrains the camera's path to a circular region directly above the current floor
        private void changeCameraPosition(double XYAngle, double radius)
        {
            double newX = radius * Math.Cos(XYAngle);
            double newY = radius * Math.Sin(XYAngle);
            
            camera.Position = new Point3D(newX, newY, camera.Position.Z);
            camera.LookDirection = new Vector3D(-1 * newX, -1 * newY, -1 * (floorZSpacing + cameraOffset));
        }        
        // Same as changeCameraPosition but uses effects to change camera Position and LookDirection
        private void changeCameraPositionWithEffects(double XYAngle, double radius, int level)
        {
            double newX = radius * Math.Cos(XYAngle);
            double newY = radius * Math.Sin(XYAngle);

            Storyboard moveCamera = (Storyboard)this.Resources["CameraSwitch"];
            moveCamera.Children.Clear();

            Point3DAnimation moveCameraAnim = new Point3DAnimation(new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z), new Point3D(newX, newY, level * floorZSpacing + cameraOffset), floorSwitchEffectDuration);
            Storyboard.SetTargetName(moveCameraAnim, "camera");
            Storyboard.SetTargetProperty(moveCameraAnim, new PropertyPath(PerspectiveCamera.PositionProperty));
            moveCamera.Children.Add(moveCameraAnim);

            Vector3DAnimation moveCameraLookAnim = new Vector3DAnimation(new Vector3D(camera.LookDirection.X, camera.LookDirection.Y, camera.LookDirection.Z), new Vector3D(-1 * newX, -1 * newY, -1 * (floorZSpacing + cameraOffset)), floorSwitchEffectDuration);
            Storyboard.SetTargetName(moveCameraLookAnim, "camera");
            Storyboard.SetTargetProperty(moveCameraLookAnim, new PropertyPath(PerspectiveCamera.LookDirectionProperty));
            moveCamera.Children.Add(moveCameraLookAnim);

            this.BeginStoryboard(moveCamera);            
            moveCamera.Completed += new EventHandler(moveCamera_Completed);            

        }

        
        void moveCamera_Completed(object sender, EventArgs e)
        {
        }
    }
}
