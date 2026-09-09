using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawImage.
  /// </summary>
  public class ImagesFormXObject : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\..\PDFs\SomeLayout.pdf");

      double dx = gfx.PageSize.Width;
      double dy = gfx.PageSize.Height;

#if true
      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.RotateTransform(-25);
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
