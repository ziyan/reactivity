using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Globalization;

namespace Reactivity.UI.Panel.TimeChart
{
    public class ColoredPoint
    {
        public Point pointData;
        public Color pointColor;
        public string name;

        public ColoredPoint()
        {
            name = String.Empty;
            pointColor = Colors.White;
            pointData = new Point(0, 0);
        }

        public ColoredPoint Clone()
        {
            ColoredPoint point = new ColoredPoint();
            point.name = this.name;
            point.pointColor = this.pointColor;
            point.pointData = new Point(this.pointData.X, this.pointData.Y);

            return point;
        }
    }

    public class AdornerCursor2: Adorner
    {
          // ********************************************************************
        // Private Fields
        // ********************************************************************
        #region Private Fields

        private Point mousePoint;
        /// <summary>
        /// The transform of the element being adorned
        /// </summary>
        private MatrixTransform elementTransform;

        private Point mouseDownPoint;
        private Point mouseUpPoint;
        private bool isDrawingZoomVisual = false;
        private bool isDrawingPanningVisual = false;
        private Size canvasSize;
        private bool isInPanMode = false;

        private ImageSource panCursorImage;
        private List<ColoredPoint> lockPoints = new List<ColoredPoint>();

        #endregion Private Fields

        // ********************************************************************
        // Public Methods
        // ********************************************************************
        #region Public Methods

        /// <summary>
        /// Constructor. Initializes class fields.
        /// </summary>
        public AdornerCursor2(UIElement adornedElement, MatrixTransform shapeTransform)
            : base(adornedElement)
        {
            this.elementTransform = shapeTransform;
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// Draws a mouse cursor on the adorened element
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            GeneralTransform inverse = elementTransform.Inverse;
            if (inverse == null)
                return;

            Brush blackBrush = new SolidColorBrush(Colors.White);
            blackBrush.Freeze();

            float radius = 5;

            Pen blackPen = new Pen(blackBrush, .7);
            blackPen.Freeze();

            if (isInPanMode)
            {
                //drawing the pan icon symbol
                if (panCursorImage != null)
                    drawingContext.DrawImage(panCursorImage, new Rect(mousePoint, new Size(16, 16)));
            }
            else
            {
                // Draw the normal zooming symbol
                drawingContext.DrawLine(blackPen, new Point(mousePoint.X, mousePoint.Y - radius), new Point(mousePoint.X, mousePoint.Y + radius));
                drawingContext.DrawLine(blackPen, new Point(mousePoint.X - radius, mousePoint.Y), new Point(mousePoint.X + radius, mousePoint.Y));
            }

            if (lockPoints.Count >0)
            {
                foreach(ColoredPoint pt in lockPoints)
                    if (pt.pointData.Y >= 0 && pt.pointData.Y <= canvasSize.Height)
                    {
                        Brush cursorBrush = new SolidColorBrush(pt.pointColor);
                        Pen cursorPen = new Pen(cursorBrush, 0.7);
                        cursorBrush.Freeze();
                        cursorPen.Freeze();

                        drawingContext.DrawEllipse(cursorBrush, cursorPen, pt.pointData, 4, 4);
                    }
            }

            if (isDrawingZoomVisual)
            {
                Rect rect = new Rect();

                rect.X = Math.Min(mouseDownPoint.X, mousePoint.X);
                rect.Y = Math.Min(mouseDownPoint.Y, mousePoint.Y);
                rect.Width = Math.Abs(mouseDownPoint.X - mousePoint.X);
                rect.Height = Math.Abs(mouseDownPoint.Y - mousePoint.Y);

                Brush zoomingBrush = new SolidColorBrush(Colors.LightBlue);

                drawingContext.PushOpacity(0.3);
                drawingContext.DrawRectangle(zoomingBrush, blackPen, rect);
                drawingContext.Pop();
            }

        }

        #endregion Public Methods

        // ********************************************************************
        // Properties
        // ********************************************************************
        #region Properties

        /// <summary>
        /// Gets/Sets the current mouse position
        /// </summary>
        public Point MousePoint
        {
            get
            {
                return mousePoint;
            }
            set
            {
                if (mousePoint != value)
                {
                    mousePoint = value;
                    AdornerLayer parent = AdornerLayer.GetAdornerLayer(AdornedElement);
                    parent.Update();
                }
            }
        }

        public Point MouseDownPoint
        {
            get { return mouseDownPoint; }
            set
            {
                if (mouseDownPoint != value)
                {
                    mouseDownPoint = value;
                    AdornerLayer parent = AdornerLayer.GetAdornerLayer(AdornedElement);
                    parent.Update();
                }
            }
        }

        public Point MouseUpPoint
        {
            get { return mouseUpPoint; }
            set
            {
                if (mouseUpPoint != value)
                {
                    mouseUpPoint = value;
                    AdornerLayer parent = AdornerLayer.GetAdornerLayer(AdornedElement);
                    parent.Update();
                }
            }
        }

        public bool IsDrawingZoomVisual
        {
            get { return isDrawingZoomVisual; }
            set
            {
                if (isDrawingZoomVisual != value)
                {
                    isDrawingZoomVisual = value;
                    AdornerLayer parent = AdornerLayer.GetAdornerLayer(AdornedElement);
                    parent.Update();
                }
            }
        }

        public bool IsDrawingPanningVisual
        {
            get { return isDrawingPanningVisual; }
            set
            {
                if (isDrawingPanningVisual != value)
                {
                    isDrawingPanningVisual = value;
                    AdornerLayer parent = AdornerLayer.GetAdornerLayer(AdornedElement);
                    parent.Update();
                }
            }
        }

        public Size CanvasSize
        {
            get { return canvasSize; }
            set
            {
                canvasSize = value;
            }
        }

        public void ClearLockPoints()
        {
            lockPoints.Clear();
        }

        public void AddLockPoint(ColoredPoint pt)
        {
            lockPoints.Add(pt);
        }

        public ImageSource PanCursorImage
        {
            get { return panCursorImage; }
            set { panCursorImage = value; }
        }

        public bool IsInPanMode
        {
            get { return isInPanMode; }
            set { isInPanMode = value; }
        }

        #endregion Properties
    }
}
