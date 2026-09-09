// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Reactivity.UI.Panel.Swordfish\TestPage.xaml.cs                           **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reactivity.UI.Panel.Swordfish
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class TestPage : Grid
    {        
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        ChartPrimitive[] lines = new ChartPrimitive[3];
        Random rand = new Random();
        double curX = 0;

        public TestPage()
        {
            InitializeComponent();
			AddTestLines();

            //timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();

		}

		public XYLineChart XYLineChart
		{
			get
			{
				return xyLineChart;
			}
		}

       
        void timer_Tick(object sender, EventArgs e)
        {
            curX += 0.1;
            if (lines.Length >= 20)
            {
                lines[0].RemoveLastPoint();
                lines[1].RemoveLastPoint();
            }
            lines[0].AddPoint(curX, rand.NextDouble());
            lines[1].AddPoint(curX, rand.NextDouble());

            textmsg.Text = lines.Length.ToString();

            xyLineChart.RedrawPlotLines();

        }


        /// <summary>
        /// Adds a set of lines to the chart for test purposes
        /// </summary>
        /// <param name="xyLineChart"></param>
        public void AddTestLines()
        {
            // Add test Lines to demonstrate the control

            xyLineChart.Primitives.Clear();
            

            double limit = 5;
            double increment = .1;

            // Create 3 normal lines

/*
            for (int lineNo = 0; lineNo < 2; ++lineNo)
            {
                ChartPrimitive line = new ChartPrimitive();

                // Label the lines
                line.Filled = true;
                line.Dashed = false;
                line.LineThickness = 1.5;
                line.ShowInLegend = true;
                line.HitTest = true;
                //line.AddPoint(0, 0);

                // Draw 3 sine curves
                /*
                for (double x = 0; x < limit + increment * .5; x += increment)
                {                    
                    line.AddPoint(x, rand.NextDouble());
                }
                line.AddPoint(limit, 0);
                 

                // Add the lines to the chart
                xyLineChart.Primitives.Add(line);
                lines[lineNo] = line;
                 
            }

            // Set the line colors to Red, Green, and Blue
            lines[0].Color = Color.FromArgb(90, 255, 0, 0);
            lines[1].Color = Color.FromArgb(90, 0, 180, 0);                    
*/
            for (int lineNo = 0; lineNo < 3; ++lineNo)
            {
                ChartPrimitive line = new ChartPrimitive();

                // Label the lines
                line.Label = "Test Line " + (lineNo + 1).ToString();
                line.ShowInLegend = true;
                line.HitTest = true;

                line.LineThickness = 1.5;
                // Draw 3 sine curves
                //for (double x = 0; x < limit + increment * .5; x += increment)
                //{
                //    line.AddPoint(x, Math.Cos(x * Math.PI - lineNo * Math.PI / 1.5));
                //}

                // Add the lines to the chart
                xyLineChart.Primitives.Add(line);
                lines[lineNo] = line;
            }
            // Set the line colors to Red, Green, and Blue
            lines[0].Color = Colors.Blue;
            lines[1].Color = Colors.Green;
            //lines[2].Color = Colors.Blue;

            xyLineChart.Title = "Live Sensor Data";            
            xyLineChart.XAxisLabel = "Time Since Measurement (seconds)";
            xyLineChart.YAxisLabel = "Sensor Units (unit)";

            xyLineChart.RedrawPlotLines();

            curX = limit;
        }


    }
}