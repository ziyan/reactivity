// ****************************************************************************
// Copyright Swordfish Computing Australia 2006                              **
// http://www.swordfish.com.au/                                              **
//                                                                           **
// Filename: Reactivity.UI.Panel\MouseCursorCoordinateDrawer.cs             **
// Authored by: John Stewien of Swordfish Computing                          **
// Date: April 2006                                                          **
//                                                                           **
// - Change Log -                                                            **
//*****************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows;
using System.Drawing.Imaging;

namespace Reactivity.UI.Panel.Swordfish
{
	/// <summary>
	/// This class put coordinates on the mouse cursor
	/// </summary>
	public class MouseCursorCoordinateDrawer
	{
		// ********************************************************************
		// Private Fields
		// ********************************************************************
		#region Private Fields

		/// <summary>
		/// The bitmap being used for the cursor
		/// </summary>
		private System.Drawing.Bitmap cursorBitmap;
		/// <summary>
		/// The last cursor created. Needs to be disposed.
		/// </summary>
		private Cursor lastCursor = null;
		/// <summary>
		/// The last coorinate painted on the cursor
		/// </summary>
		private System.Windows.Point lastCoordinate;
		/// <summary>
		/// The cursor position. Used for calculating the relative position of the text.
		/// </summary>
		private System.Windows.Point cursorPosition;
		/// <summary>
		/// The closest point
		/// </summary>
		private System.Windows.Point lockPoint;
		/// <summary>
		/// Flag indicating if the coordinates are locked to the closest point or or not.
		/// </summary>
		private bool locked;
		/// <summary>
		/// Number of significant figures for the x coordinate
		/// </summary>
		private string xFormat = "";
		/// <summary>
		/// Number of significant figures for the y coordinate
		/// </summary>
		private string yFormat = "";
		/// <summary>
		/// Flag to indicate that an exception was thrown when this tried to set
		/// the cursor, so do not try it again if false
		/// </summary>
		private bool hasPermissionToRun = true;

		#endregion Private Fields

		// ********************************************************************
		// Public Methods
		// ********************************************************************
		#region Public Methods

		/// <summary>
		/// Constructor. Initializes class fields.
		/// </summary>
		public MouseCursorCoordinateDrawer()
		{
			cursorBitmap = new System.Drawing.Bitmap(48, 48, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		}

		/// <summary>
		/// Set the coordinates on the cursor in an untransformed manner
		/// </summary>
		/// <param name="mousePos"></param>
		/// <param name="uiElement"></param>
		public void SetCoordinatesOnCursor(System.Windows.Point mousePos, System.Windows.FrameworkElement uiElement)
		{
			SetCoordinatesOnCursor(mousePos, uiElement, new ScaleTransform(1, 1));
		}

		/// <summary>
		/// Sets the coordinates on the cursor using the transform passed in
		/// </summary>
		/// <param name="mousePos"></param>
		/// <param name="uiElement"></param>
		/// <param name="transform"></param>
		public void SetCoordinatesOnCursor(System.Windows.Point mousePos, System.Windows.FrameworkElement uiElement, GeneralTransform transform)
		{
			if (transform != null && hasPermissionToRun)
			{
				try
				{
					cursorPosition = mousePos;
					lastCoordinate = transform.Transform(mousePos);
					SetStringFormat(transform);
					SetCoordinatesOnCursor(uiElement);
				}
				catch (Exception)
				{
					hasPermissionToRun = false;
				}
			}
		}

		#endregion Public Methods
	
		// ********************************************************************
		// Private Methods
		// ********************************************************************
		#region Private Methods

		/// <summary>
		/// Works out the number of decimal places required to show the different between
		/// 2 pixels. E.g if pixels are .1 apart then use 2 places etc
		/// </summary>
		private void SetStringFormat(GeneralTransform transform)
		{
			Rect rect = new Rect(0, 0, 1, 1);
			rect = transform.TransformBounds(rect);

			int xFigures = (int)(Math.Ceiling(-Math.Log10(rect.Width))+.1);
			int yFigures = (int)(Math.Ceiling(-Math.Log10(rect.Height))+.1);
			xFormat = "#0.";
			yFormat = "#0.";
			for (int i = 0; i < xFigures; ++i)
			{
				xFormat += "#";
			}

			for (int i = 0; i < yFigures; ++i)
			{
				yFormat += "#";
			}
		}

		/// <summary>
		/// Changes the cursor for this control to show the coordinates
		/// </summary>
		/// <param name="mousePos"></param>
		private void SetCoordinatesOnCursor(System.Windows.FrameworkElement uiElement)
		{
			System.Windows.Point coordinate = locked ? lockPoint : lastCoordinate;
			Cursor newCursor = null;
			System.Drawing.Font cursorFont = new System.Drawing.Font("Arial", 8f);

			try
			{
				// Lets get the string to be printed
				string coordinateText = coordinate.X.ToString(xFormat) + "," + coordinate.Y.ToString(yFormat);
				// Calculate the rectangle required to draw the string
				System.Drawing.SizeF textSize = GetTextSize(coordinateText, cursorFont);

				// ok, so here's the minimum 1/4 size of the bitmap we need, as the
				// Hotspot for the cursor will be in the centre of the bitmap.
				int minWidth = 8 + (int)Math.Ceiling(textSize.Width);
				int minHeight = 8 + (int)Math.Ceiling(textSize.Height);

				// If the bitmap needs to be resized, then resize it, else just clear it
				if (cursorBitmap.Width < minWidth * 2 || cursorBitmap.Height < minHeight * 2)
				{
					System.Drawing.Bitmap oldBitmap = cursorBitmap;
					cursorBitmap = new System.Drawing.Bitmap(Math.Max(cursorBitmap.Width, minWidth * 2), Math.Max(cursorBitmap.Height, minHeight * 2));
					oldBitmap.Dispose();
				}

				// Get the centre of the bitmap which will be the Hotspot
				System.Drawing.Point centre = new System.Drawing.Point(cursorBitmap.Width / 2, cursorBitmap.Height / 2);
				/// Calculate the text rectangle
				System.Drawing.Rectangle textRectangle = new System.Drawing.Rectangle(centre.X + 8, centre.Y + 8, minWidth - 8, minHeight - 8);

				int diff = (int)cursorPosition.X + textRectangle.Right / 2 - 3 - (int)uiElement.ActualWidth;

				if (diff > 0)
				{
					textRectangle.Location = new System.Drawing.Point(textRectangle.Left - diff, textRectangle.Top);
				}

				// Draw the target symbol, and the coordinate text on the bitmap
				using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(cursorBitmap))
				{
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					// This line causes a crash on laptops when you render a string
					// g.CompositingMode = CompositingMode.SourceCopy;
					g.Clear(System.Drawing.Color.Transparent);

					float targetX = centre.X;
					float targetY = centre.Y;

					float radius = 30;

					if (!locked)
					{
						System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0, 0), 1.4f);
						g.DrawEllipse(blackPen, targetX - radius * .5f, targetY - radius * .5f, radius, radius);
						g.DrawLine(blackPen, targetX - radius * .8f, targetY, targetX - 2f, targetY);
						g.DrawLine(blackPen, targetX + 2f, targetY, targetX + radius * .8f, targetY);
						g.DrawLine(blackPen, targetX, targetY - radius * .8f, targetX, targetY - 2f);
						g.DrawLine(blackPen, targetX, targetY + 2f, targetX, targetY + radius * .8f);
					}
					else
					{
						System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 0, 0, 0), 3f);
						System.Drawing.Pen yellowPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, 255, 255, 0), 2f);
						g.DrawEllipse(blackPen, targetX - radius * .5f, targetY - radius * .5f, radius, radius);
						g.DrawEllipse(yellowPen, targetX - radius * .5f, targetY - radius * .5f, radius, radius);
					}

					if (!locked)
						g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(127, 255, 255, 255)), textRectangle);
					else
						g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(170, 255, 255, 0)), textRectangle);

					// Setup the text format for drawing the subnotes
					using (System.Drawing.StringFormat stringFormat = new System.Drawing.StringFormat())
					{
						stringFormat.Trimming = System.Drawing.StringTrimming.None;
						stringFormat.FormatFlags = System.Drawing.StringFormatFlags.NoClip | System.Drawing.StringFormatFlags.NoWrap;
						stringFormat.Alignment = System.Drawing.StringAlignment.Near;

						// Draw the string left aligned
						g.DrawString(
								coordinateText,
								cursorFont,
								new System.Drawing.SolidBrush(System.Drawing.Color.Black),
								textRectangle,
								stringFormat);
					}
				}

				// Now copy the bitmap to the cursor
				newCursor = WPFCursorFromBitmap.CreateCursor(cursorBitmap);

			}
			catch (Exception)
			{
			}
			finally
			{
				// After the new cursor has been set, the unmanaged resources can be
				// cleaned up that were being used by the old cursor
				if (newCursor != null)
				{
					uiElement.Cursor = newCursor;
				}
				if (lastCursor != null)
				{
					lastCursor.Dispose();
				}
				lastCursor = newCursor;

				// Save the new values for cleaning up on the next pass
			}
		}

		/// <summary>
		/// Gets the rectangle taken up by the rendered text.
		/// </summary>
		/// <param name="text">The text to render</param>
		/// <param name="font">The font to render the text in</param>
		/// <returns>The rectangle taken up by rendering the text</returns>
		private static System.Drawing.RectangleF GetTextRectangle(string text, System.Drawing.Font font)
		{
			System.Drawing.RectangleF retRectangle = new System.Drawing.RectangleF();

			// Create a small bitmap just to get a device context
			System.Drawing.Bitmap tmpBitmap = new System.Drawing.Bitmap(1, 1);

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tmpBitmap))
			using (System.Drawing.StringFormat stringFormat = new System.Drawing.StringFormat())
			{
				stringFormat.Alignment = System.Drawing.StringAlignment.Near;
				stringFormat.Trimming = System.Drawing.StringTrimming.None;
				stringFormat.FormatFlags = System.Drawing.StringFormatFlags.NoClip | System.Drawing.StringFormatFlags.NoWrap;
				// Do a small rectangle. The size will be ignored with the flags being used
				System.Drawing.RectangleF rectangle = new System.Drawing.RectangleF(0, 0, 1, 1);
				// Set the stringFormat for measuring the first character
				System.Drawing.CharacterRange[] characterRanges ={ new System.Drawing.CharacterRange(0, text.Length) };
				stringFormat.SetMeasurableCharacterRanges(characterRanges);

				System.Drawing.Region[] stringRegions = g.MeasureCharacterRanges(text, font, rectangle, stringFormat);

				// Draw rectangle for first measured range.
				retRectangle = stringRegions[0].GetBounds(g);
			}
			return retRectangle;
		}
		/// <summary>
		/// Gets the width and height of the rendered text.
		/// </summary>
		/// <param name="text">The text to render</param>
		/// <param name="font">The font to render the text in</param>
		/// <returns>The size of the rendered text</returns>
		private static System.Drawing.SizeF GetTextSize(string text, System.Drawing.Font font)
		{
			System.Drawing.RectangleF rectangle = GetTextRectangle(text, font);
			System.Drawing.SizeF size = new System.Drawing.SizeF(rectangle.Right, rectangle.Bottom);
			return size;
		}

		#endregion Private Methods

		// ********************************************************************
		// Properties
		// ********************************************************************
		#region Properties

		/// <summary>
		/// Gets/Sets if the coordinates are locked to the LockPoint or not
		/// </summary>
		public bool Locked
		{
			get
			{
				return locked;
			}
			set
			{
				locked = value;
			}
		}

		/// <summary>
		/// Gets/Sets the coordinate for the cursor to show when it is "Locked"
		/// </summary>
		public System.Windows.Point LockPoint
		{
			get
			{
				return lockPoint;
			}
			set
			{
				lockPoint = value;
			}
		}

		#endregion Properties
	}
}
