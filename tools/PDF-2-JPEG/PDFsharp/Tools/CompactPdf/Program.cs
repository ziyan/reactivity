using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace CompactPdf
{
  class Program
  {
    static void Main(string[] args)
    {
      if (!CheckArgs(args))
        return;

      string filename = args[0];

      filename = @"G:\!StLa\PDFsharp Problems\07-06-06 Stack Overflow\1178570264408.pdf";
      filename = @"G:\!StLa\QBX2006-6.00\Layout\Musterklinik-grau\Qualitätsbericht\Musterklinik-01.pdf";
      try
      {
        PdfDocument document = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);
        document.Options.CompressContentStreams = true;
        document.Options.NoCompression = false;

        // Convert all content stream in all pages to PdfContent objects
        PdfPages pages = document.Pages;
        for (int pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
          PdfPage page = pages[pageIndex];
          PdfContents contents = page.Contents;
          if (contents != null) // happens only in an invalid PDF file
          {
            PdfArray.ArrayElements contElements = contents.Elements;
            for (int contIndex = 0; contIndex < contElements.Count; contIndex++)
            {
              PdfReference iref = contElements[contIndex] as PdfReference;
              if (iref != null)
              {
                PdfDictionary dict = iref.Value as PdfDictionary;
                iref.Value = new PdfContent(dict);  // HACK: ctor must get public -> TODO: TransformToType<PdfContent>(dict);
              }
            }
          }
        }
        document.Internals.Catalog.Elements.Remove("/Outlines");  // delete more than 21000 objects
        document.Save(filename + ".pdf"); // HACK
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unexpected error: " + ex.ToString());
      }
    }

    static bool CheckArgs(string[] args)
    {
      //int count = args.Length;
      //if (count < 1 && count > 4)
      //  return Error();

      //if (count == 1)
      //{
      //  if (args[0] == "/?" || args[0] == "-?")
      //  {
      //    ShowHelp();
      //    return false;
      //  }
      //  return Error();
      //}

      //if (count < 3)
      //  return Error();

      //if (!File.Exists(args[0]))
      //{
      //  Console.WriteLine("File {0} does not exist.", args[0]);
      //  return false;
      //}

      //if (!File.Exists(args[1]))
      //{
      //  Console.WriteLine("File {0} does not exist.", args[1]);
      //  return false;
      //}

      //if (args.Length == 4 && args[3] != "-v")
      //  return Error();

      return true;
    }

    static bool Error()
    {
      Console.WriteLine("Invalid number of parameters. Try COMPPDF /?");
      return false;
    }

    static void ShowHelp()
    {
      //Console.WriteLine("Compares two PDF files by creating a new PDF file with facing pages.\n\n");
      //Console.WriteLine("COMPPDF filename1 filename2 destination [-v]\n");
      //Console.WriteLine("  sourcefile1  First PDF file to compare.");
      //Console.WriteLine("  sourcefile2  Second PDF file to compare.");
      //Console.WriteLine("  destfile     Destination PDF file.");
      //Console.WriteLine("  -v           View destination file.");
    }
  }
}
