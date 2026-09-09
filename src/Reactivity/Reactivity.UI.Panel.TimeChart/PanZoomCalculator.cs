using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Reactivity.UI.Panel.TimeChart
{
    class PanZoomCalculator
    {
        private Point currentPan;
        private Point currentZoom;

        public Point Pan
        {
            get { return currentPan; }
            set { currentPan = value; }
        }

        public Point Zoom
        {
            get { return currentZoom; }
            set { currentZoom = value; }
        }

        public PanZoomCalculator()
        {
            currentPan = new Point(0, 0);
            currentZoom = new Point(1, 1);
        }
    }
}
