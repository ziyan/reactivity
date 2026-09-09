// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Reactivity.UI.Panel\XYLineChart.cs                             **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Windows.Documents;
using System.Diagnostics;

namespace Reactivity.UI.Panel.Swordfish
{
	public partial class XYLineChart : UserControl
	{
		// ********************************************************************
		// Private Fields
		// ********************************************************************
		#region Private Fields

		// user data

		/// <summary>
		/// A list of lines to draw on the chart
		/// </summary>
		private List<ChartPrimitive> primitiveList;

		// internal settings

		private MatrixTransform shapeTransform;
		private Point minPoint;
		private Point maxPoint;
		private Point optimalGridLineSpacing;

		private PathGeometry chartClip;

		// Helper classes

		private ClosestPointPicker closestPointPicker;
		private PanZoomCalculator panZoomCalculator;

		// Appearance

		private Color gridLineColor = Colors.White;
        private Color gridLineLabelColor = Colors.White;

		private AdornerCursorCoordinateDrawer adorner;
		private AdornerLayer adornerLayer = null;

		#endregion Private Fields

		// ********************************************************************
		// Public Methods
		// ********************************************************************
		#region Public Methods

		/// <summary>
		/// Constructor. Initializes all the class fields.
		/// </summary>
		public XYLineChart()
		{
			// This assumes that you are navigating to this scene.
			// If you will normally instantiate it via code and display it
			// manually, you either have to call InitializeComponent by hand or
			// uncomment the following line.

			this.InitializeComponent();

			primitiveList = new List<ChartPrimitive>();

			// Set the Chart Geometry Clip region
			chartClip = new PathGeometry();
			chartClip.AddGeometry(new RectangleGeometry(new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight)));
			shapeTransform = new MatrixTransform();
			adorner = new AdornerCursorCoordinateDrawer(clippedPlotCanvas, shapeTransform);

			optimalGridLineSpacing = new Point(100, 75);

			panZoomCalculator = new PanZoomCalculator(new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight));
			panZoomCalculator.Window = new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
			panZoomCalculator.PanZoomChanged += new EventHandler<PanZoomArgs>(panZoomCalculator_PanZoomChanged);

			closestPointPicker = new ClosestPointPicker(new Size(13, 13));
			closestPointPicker.ClosestPointChanged += new EventHandler<ClosestPointArgs>(closestPointPicker_ClosestPointChanged);

			this.Cursor = Cursors.None;
			
			// Set up all the message handlers for the clipped plot canvas
			AttachEventsToCanvas(this);
			clippedPlotCanvas.IsVisibleChanged += new DependencyPropertyChangedEventHandler(clippedPlotCanvas_IsVisibleChanged);
			clippedPlotCanvas.SizeChanged += new SizeChangedEventHandler(clippedPlotCanvas_SizeChanged);
		}

		/// <summary>
		/// Attaches mouse handling events to the canvas passed in. The canvas passed in should be the top level canvas.
		/// </summary>
		/// <param name="eventCanvas"></param>
		protected void AttachEventsToCanvas(UIElement eventCanvas)
		{
			eventCanvas.LostMouseCapture += new MouseEventHandler(clippedPlotCanvas_LostMouseCapture);
			eventCanvas.MouseMove += new System.Windows.Input.MouseEventHandler(clippedPlotCanvas_MouseMove);
			eventCanvas.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(clippedPlotCanvas_MouseLeftButtonDown);
			eventCanvas.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(clippedPlotCanvas_MouseLeftButtonUp);
			eventCanvas.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(clippedPlotCanvas_MouseRightButtonDown);
			eventCanvas.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(clippedPlotCanvas_MouseRightButtonUp);
			eventCanvas.MouseEnter += new MouseEventHandler(clippedPlotCanvas_MouseEnter);
			eventCanvas.MouseLeave += new MouseEventHandler(clippedPlotCanvas_MouseLeave);
		}

		/// <summary>
		/// Reset everything from the current collection of Chart Primitives
		/// </summary>
		public void RedrawPlotLines()
		{
			closestPointPicker.Points.Clear();
			//legendBox.Children.Clear();
			foreach (ChartPrimitive primitive in primitiveList)
			{
				if (primitive.ShowInLegend)
				{
					//legendBox.Children.Insert(0, new ColorLabel(primitive.Label, primitive.Color));
				}
				if (primitive.HitTest)
				{
					closestPointPicker.Points.AddRange(primitive.Points);
				}
			}
			InvalidateMeasure();
			SetChartTransform(clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
			RenderPlotLines(clippedPlotCanvas);
		}

		#endregion Public Methods

		// ********************************************************************
		// Protected Methods
		// ********************************************************************
		#region Protected Methods

		/// <summary>
		/// Resize the plot by changing the transform and drawing the grid lines
		/// </summary>
		protected void ResizePlot()
		{
			SetChartTransform(clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
			// Don't need to re-render the plot lines, just change the transform
			//RenderPlotLines(clippedPlotCanvas);

			// Still need to redraw the grid lines
			InvalidateMeasure();
			// Grid lines are now added in the measure override to stop flickering
			//DrawGridlinesAndLabels(textCanvas, new Size(textCanvas.ActualWidth, textCanvas.ActualHeight), minPoint, maxPoint);
		}

		/// <summary>
		/// Set the chart transform
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		protected void SetChartTransform(double width, double height)
		{
			Rect plotArea = ChartUtilities.GetPlotRectangle(primitiveList, 0.01f);

			minPoint = plotArea.Location;
			minPoint.Offset(-plotArea.Width * panZoomCalculator.Pan.X, plotArea.Height * panZoomCalculator.Pan.Y);
			minPoint.Offset(0.5 * plotArea.Width * (1 - 1 / panZoomCalculator.Zoom.X), 0.5 * plotArea.Height * (1 - 1 / panZoomCalculator.Zoom.Y));

			maxPoint = minPoint;
			maxPoint.Offset(plotArea.Width / panZoomCalculator.Zoom.X, plotArea.Height / panZoomCalculator.Zoom.Y);

			Point plotScale = new Point();
			plotScale.X = (width / plotArea.Width) * panZoomCalculator.Zoom.X;
			plotScale.Y = (height / plotArea.Height) * panZoomCalculator.Zoom.Y;

			Matrix shapeMatrix = Matrix.Identity;
			shapeMatrix.Translate(-minPoint.X, -minPoint.Y);
			shapeMatrix.Scale(plotScale.X, plotScale.Y);
			shapeTransform.Matrix = shapeMatrix;
		}

		/// <summary>
		/// Render all the plot lines from the collection of Chart Primitives
		/// </summary>
		/// <param name="canvas"></param>
		protected void RenderPlotLines(Canvas canvas)
		{
			// Draw the Chart Plot Points

			// Fill in the background
			canvas.Children.Clear();
            /*
            for(int i = primitiveList.Count-1; i >= 0; i--)
            {
              if (primitiveList[i].Points.Count > 0)
                {
                    Path path = new Path();
                    PathGeometry pathGeometry = new PathGeometry();
                    pathGeometry.Transform = shapeTransform;
                    
                    if (primitiveList[i].Filled)
                    {
                        pathGeometry.AddGeometry(primitiveList[i].PathGeometry);
                        path.Stroke = null;
                        if (primitiveList[i].Dashed)
                        {
                            path.Fill = ChartUtilities.CreateHatch50(primitiveList[i].Color, new Size(2, 2));
                        }
                        else
                            path.Fill = new SolidColorBrush(primitiveList[i].Color);
                    }
                    else
                    {
                        pathGeometry.AddGeometry(primitiveList[i].PathGeometry);
                        path.Stroke = new SolidColorBrush(primitiveList[i].Color);
                        path.StrokeThickness = primitiveList[i].LineThickness;
                        path.Fill = null;
                        if (primitiveList[i].Dashed)
                            path.StrokeDashArray = new DoubleCollection(new double[] { 2, 2 });
                    }
                    path.Data = pathGeometry;
                    path.Clip = chartClip;
                    canvas.Children.Add(path);
                }
            }
            */
            
			foreach (ChartPrimitive primitive in primitiveList)
			{
				if (primitive.Points.Count > 0)
				{
					Path path = new Path();
					PathGeometry pathGeometry = new PathGeometry();
					pathGeometry.Transform = shapeTransform;

					if (primitive.Filled)
					{
						pathGeometry.AddGeometry(primitive.PathGeometry);
						path.Stroke = null;
						if (primitive.Dashed)
						{
							path.Fill = ChartUtilities.CreateHatch50(primitive.Color, new Size(2, 2));
						}
						else
							path.Fill = new SolidColorBrush(primitive.Color);
					}
					else
					{
						pathGeometry.AddGeometry(primitive.PathGeometry);
						path.Stroke = new SolidColorBrush(primitive.Color);
						path.StrokeThickness = primitive.LineThickness;
						path.Fill = null;
						if (primitive.Dashed)
							path.StrokeDashArray = new DoubleCollection(new double[] { 2, 2 });
					}
					path.Data = pathGeometry;
					path.Clip = chartClip;
					canvas.Children.Add(path);
				}
			}
             
		}

		/// <summary>
		/// Add grid lines here
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size constraint)
		{
			DrawGridlinesAndLabels(textCanvas, new Size(textCanvas.ActualWidth, textCanvas.ActualHeight), minPoint, maxPoint);
			return base.MeasureOverride(constraint);
		}

		/// <summary>
		/// Draw all the gridlines and labels for the gridlines
		/// </summary>
		/// <param name="textCanvas"></param>
		/// <param name="size"></param>
		/// <param name="minXY"></param>
		/// <param name="maxXY"></param>
		protected void DrawGridlinesAndLabels(Canvas textCanvas, Size size, Point minXY, Point maxXY)
		{
			// Clear the text canvas
			textCanvas.Children.Clear();

			// Create brush for writing text
			Brush axisBrush = new SolidColorBrush(gridLineColor);
			Brush axisScaleBrush = new SolidColorBrush(gridLineLabelColor);

			// Need to pick appropriate scale increment.
			// Go for a 2Exx, 5Exx, or 1Exx type scale
			double scaleX = 0.0;
			double scaleY = 0.0;

			// Work out all the limits

			if (maxXY.X != minXY.X)
				scaleX = (double)size.Width / (double)(maxXY.X - minXY.X);
			if (maxXY.Y != minXY.Y)
				scaleY = (double)size.Height / (double)(maxXY.Y - minXY.Y);
			double optimalSpacingX = optimalGridLineSpacing.X / scaleX;

			double spacingX = ChartUtilities.ClosestValueInListTimesBaseToInteger(optimalSpacingX, new double[] { 1, 3, 6 }, 12.0);

			if (spacingX < 2.0)
				spacingX = ChartUtilities.Closest_1_2_5_Pow10(optimalSpacingX);

			double optimalSpacingY = optimalGridLineSpacing.Y / scaleY;
			double spacingY = ChartUtilities.Closest_1_2_5_Pow10(optimalSpacingY);

			int startXmult = (int)Math.Ceiling(minXY.X / spacingX);
			int endXmult = (int)Math.Floor(maxXY.X / spacingX);
			int startYmult = (int)Math.Ceiling(minXY.Y / spacingY);
			int endYmult = (int)Math.Floor(maxXY.Y / spacingY);

			double maxXLabelHeight = 0;

			PathFigure pathFigure = new PathFigure();

			// Draw all the vertical gridlines

			for (int lineNo = startXmult; lineNo <= endXmult; ++lineNo)
			{
				double xValue = lineNo * spacingX;
				double xPos = (xValue - minXY.X) * scaleX;

				Point startPoint = new Point(xPos, size.Height);
				Point endPoint = new Point(xPos, 0);

				pathFigure.Segments.Add(new LineSegment(startPoint, false));
				pathFigure.Segments.Add(new LineSegment(endPoint, true));

				TextBlock text = new TextBlock();
				text.Text = xValue.ToString();
				text.Foreground = axisScaleBrush;
				text.Measure(size);

				text.SetValue(Canvas.LeftProperty, xPos - text.DesiredSize.Width * .5);
				text.SetValue(Canvas.TopProperty, size.Height + 1);
				textCanvas.Children.Add(text);
				maxXLabelHeight = Math.Max(maxXLabelHeight, text.DesiredSize.Height);
			}

			xGridlineLabels.Height = maxXLabelHeight + 2;

			// Set string format for vertical text
			double maxYLabelHeight = 0;

			// Draw all the horizontal gridlines

			for (int lineNo = startYmult; lineNo <= endYmult; ++lineNo)
			{
				double yValue = lineNo * spacingY;
				double yPos = (-yValue + minXY.Y) * scaleY + size.Height;

				Point startPoint = new Point(0, yPos);
				Point endPoint = new Point(size.Width, yPos);

				pathFigure.Segments.Add(new LineSegment(startPoint, false));
				pathFigure.Segments.Add(new LineSegment(endPoint, true));

				TextBlock text = new TextBlock();
				text.Text = yValue.ToString();
                text.Foreground = axisScaleBrush;
				text.LayoutTransform = new RotateTransform(-90);
				text.Measure(size);

				text.SetValue(Canvas.LeftProperty, -text.DesiredSize.Width - 1);
				text.SetValue(Canvas.TopProperty, yPos - text.DesiredSize.Height * .5);
				textCanvas.Children.Add(text);
				maxYLabelHeight = Math.Max(maxYLabelHeight, text.DesiredSize.Width);
			}
			yGridLineLabels.Height = maxYLabelHeight + 2;

			Path path = new Path();
			path.Stroke = axisBrush;
			PathGeometry pathGeometry = new PathGeometry(new PathFigure[] { pathFigure });

			pathGeometry.Transform = (Transform)textCanvas.RenderTransform.Inverse;
			path.Data = pathGeometry;

			textCanvas.Children.Add(path);
		}

		#endregion Protected Methods

		// ********************************************************************
		// Properties
		// ********************************************************************
		#region Properties

		/// <summary>
		/// Gets/Sets the title of the chart
		/// </summary>
		public string Title
		{
			get
			{
				return titleBox.Text;
			}
			set
			{
				titleBox.Text = value;
			}
		}

		/// <summary>
		/// Gets/Sets the X axis label
		/// </summary>
		public string XAxisLabel
		{
			get
			{
				return xAxisLabel.Text;
			}
			set
			{
				xAxisLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets/Sets the Y axis label
		/// </summary>
		public string YAxisLabel
		{
			get
			{
				return yAxisLabel.Text;
			}
			set
			{
				yAxisLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets the collection of chart primitives
		/// </summary>
		public List<ChartPrimitive> Primitives
		{
			get
			{
				return primitiveList;
			}
		}

		#endregion Properties

		// ********************************************************************
		// Misc Event Handlers
		// ********************************************************************
		#region Misc Event Handlers

		/// <summary>
		/// Handles when the closest point to the mouse cursor changes. Hides
		/// or shows the closest point, and changes the mouse cursor accordingly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void closestPointPicker_ClosestPointChanged(object sender, ClosestPointArgs e)
		{
			adorner.Locked = closestPointPicker.Locked;
			adorner.LockPoint = closestPointPicker.ClosestPoint;
		}

		/// <summary>
		/// Handles when the pan or zoom changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void panZoomCalculator_PanZoomChanged(object sender, PanZoomArgs e)
		{
			ResizePlot();
		}


		#endregion Misc Event Handlers

		// ********************************************************************
		// clippedPlotCanvas Event Handlers
		// ********************************************************************
		#region clippedPlotCanvas Event Handlers

		/// <summary>
		/// Adds an adorner when the plot canvas is visible
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void clippedPlotCanvas_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible && adornerLayer == null)
			{
				adornerLayer = AdornerLayer.GetAdornerLayer(clippedPlotCanvas);
				adornerLayer.Add(adorner);
				adorner.Visibility = this.IsMouseOver ? Visibility.Visible : Visibility.Hidden;
			}
			else if (adornerLayer != null)
			{
				adornerLayer.Remove(adorner);
				adornerLayer = null;
			}
		}

		/// <summary>
		/// Handles when the plot size changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			chartClip.Clear();
			chartClip.AddGeometry(new RectangleGeometry(new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight)));
			panZoomCalculator.Window = new Rect(0, 0, clippedPlotCanvas.ActualWidth, clippedPlotCanvas.ActualHeight);
			ResizePlot();
		}

		/// <summary>
		/// Handles when the plot loses focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
		{
			panZoomCalculator.StopPanning();
			panZoomCalculator.StopZooming();
			this.Cursor = Cursors.None;
		}

		/// <summary>
		/// Handles when the mouse moves over the plot
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Point mousePos = e.GetPosition(clippedPlotCanvas);

			if (!panZoomCalculator.IsPanning && !panZoomCalculator.IsZooming)
			{
				adorner.MousePoint = mousePos;
				closestPointPicker.MouseMoved(mousePos, shapeTransform.Inverse);
			}
			else
			{
				panZoomCalculator.MouseMoved(clippedPlotCanvas.RenderTransform.Inverse.Transform(mousePos));
			}
		}

		/// <summary>
		/// Handles when the user clicks on the plot
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ClickCount < 2)
			{
				if (((UIElement)sender).CaptureMouse())
				{
					this.Cursor = Cursors.ScrollAll;
					adorner.Visibility = Visibility.Hidden;
					panZoomCalculator.StartPan(clippedPlotCanvas.RenderTransform.Inverse.Transform(e.GetPosition(clippedPlotCanvas)));
				}
			}
			else
			{
				Mouse.Capture(null);
				panZoomCalculator.Reset();
			}
		}

		/// <summary>
		/// Handles when the user releases the mouse button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			panZoomCalculator.StopPanning();
			if (!panZoomCalculator.IsZooming)
			{
				Mouse.Capture(null);
				this.Cursor = Cursors.None;
				if (this.IsMouseOver)
				{
					adorner.Visibility = Visibility.Visible;
				}
			}
		}

		/// <summary>
		///  Handles when the user clicks on the plot
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ClickCount < 2)
			{
				if (((UIElement)sender).CaptureMouse())
				{
					this.Cursor = Cursors.ScrollAll;
					adorner.Visibility = Visibility.Visible;
					panZoomCalculator.StartZoom(clippedPlotCanvas.RenderTransform.Inverse.Transform(e.GetPosition(clippedPlotCanvas)));
				}
			}
		}

		/// <summary>
		/// Handles when the user releases the mouse
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void clippedPlotCanvas_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			panZoomCalculator.StopZooming();
			if (!panZoomCalculator.IsPanning)
			{
				Mouse.Capture(null);
				this.Cursor = Cursors.None;
			}
			else
			{
				adorner.Visibility = Visibility.Hidden;
				this.Cursor = Cursors.ScrollAll;
			}
		}

		/// <summary>
		/// Handles when the mouse leaves the plot. Removes the nearest point indicator.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void clippedPlotCanvas_MouseLeave(object sender, MouseEventArgs e)
		{
			adorner.Locked = false;
			adorner.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Handles when the mouse enters the plot. Puts back the nearest point indicator.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void clippedPlotCanvas_MouseEnter(object sender, MouseEventArgs e)
		{
			adorner.Visibility = Visibility.Visible;
			adorner.Locked = closestPointPicker.Locked;
		}

		#endregion clippedPlotCanvas Event Handlers
	}
}
