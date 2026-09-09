using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ImagesBmpOS2 : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawImage.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\images\Test (OS2).bmp");  

      double dx = gfx.PageSize.Width;
      double dy = gfx.PageSize.Height;

#if true
      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.RotateTransform(-60);
      gfx.TranslateTransform(-dx / 2, -dy / 2);
#endif

      gfx.DrawImage(image, (dx - image.Width) / 2, (dy - image.Height) / 2, image.Width, image.Height);
    }

    public override string Description
    {
      get {return "DrawImage";}
    }
  }
}
