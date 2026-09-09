using System;
using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

namespace ComparePDF
{
  /// <summary>
  /// Creates a PDF file for visual comparison of two PDF files.
  /// </summary>
  class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      if (!CheckArgs(args))
        return;

      string filename1 = args[0];
      string filename2 = args[1];

      try
      {
        // Create the output document
        PdfDocument document = new PdfDocument();
        document.Info.Subject = String.Format("Visual comparison of files {0} and {1}.", args[0], args[1]);
        document.Info.Creator = "comppdf";

        // Show consecutive pages facing
        document.PageLayout = PdfPageLayout.TwoPageLeft;

        XFont font = new XFont("Verdana", 10, XFontStyle.Bold);
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Far;
        XGraphics gfx;
        XRect box;

        // Open the external documents as XPdfForm objects. Such objects are
        // treated like images. By default the first page of the document is
        // referenced by a new XPdfForm.
        XPdfForm form1 = XPdfForm.FromFile(filename1);
        XPdfForm form2 = XPdfForm.FromFile(filename2);

        int count = Math.Max(form1.PageCount, form2.PageCount);
        for (int idx = 0; idx < count; idx++)
        {
          // Add two new pages to the output document
          PdfPage page1 = document.AddPage();
          PdfPage page2 = document.AddPage();

          if (form1.PageCount > idx)
          {
            gfx = XGraphics.FromPdfPage(page1);

            // Set page number (which is one-based)
            form1.PageNumber = idx + 1;

            // Draw the page identified by the page number like an image
            gfx.DrawImage(form1, new XRect(0, 0, form1.Width, form1.Height));

            // Write document file name and page number on each page
            box = page1.MediaBox.ToXRect();
            box.Inflate(0, -10);
            gfx.DrawString(String.Format("{0} • {1}", filename1, idx + 1),
              font, XBrushes.Red, box, format);
          }

          // Same as above for second page
          if (form2.PageCount > idx)
          {
            gfx = XGraphics.FromPdfPage(page2);

            form2.PageNumber = idx + 1;
            gfx.DrawImage(form2, new XRect(0, 0, form2.Width, form2.Height));

            box = page2.MediaBox.ToXRect();
            box.Inflate(0, -10);
            gfx.DrawString(String.Format("{0} • {1}", filename2, idx + 1),
              font, XBrushes.Red, box, format);
          }
        }

        // Save the document
        document.Save(args[2]);
        if (args.Length == 4 && args[3] == "-v")
          Process.Start(args[2]);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unexpected error: " + ex.ToString());
      }
    }

    static bool CheckArgs(string[] args)
    {
      int count = args.Length;
      if (count < 1 && count > 4)
        return Error();

      if (count == 1)
      {
        if (args[0] == "/?" || args[0] == "-?")
        {
          ShowHelp();
          return false;
        }
        return Error();
      }
      
      if (count < 3)
        return Error();

      if (!File.Exists(args[0]))
      {
        Console.WriteLine("File {0} does not exist.", args[0]);
        return false;
      }

      if (!File.Exists(args[1]))
      {
        Console.WriteLine("File {0} does not exist.", args[1]);
        return false;
      }

      if (args.Length == 4 && args[3] != "-v")
        return Error();

      return true;
    }

    static bool Error()
    {
      Console.WriteLine("Invalid number of parameters. Try COMPPDF /?");
      return false;
    }

    static void ShowHelp()
    {
      Console.WriteLine("Compares two PDF files by creating a new PDF file with facing pages.\n\n");
      Console.WriteLine("COMPPDF filename1 filename2 destination [-v]\n"); 
      Console.WriteLine("  sourcefile1  First PDF file to compare.");
      Console.WriteLine("  sourcefile2  Second PDF file to compare.");
      Console.WriteLine("  destfile     Destination PDF file.");
      Console.WriteLine("  -v           View destination file.");
    }
  }
}
