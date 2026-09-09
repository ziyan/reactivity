using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Ghostscript;

namespace PdfSharp.Toolbox
{
  /// <summary>
  /// Adds preview images for all pages to 
  /// </summary>
  public class LetterheadManager
  {
    public LetterheadManager()
    {
      Initialize();
    }

    /// <summary>
    /// Opens the PDF file.
    /// </summary>
    void Initialize()
    {
      if (this.pdfFile != null)
      {
        FileStream file = new FileStream(this.pdfFile, FileMode.Open);
        int length = (int)file.Length;
        byte[] bytes = new byte[length];
        file.Read(bytes, 0, length);
        file.Close();
        if (this.letterhead != null)
          this.letterhead.Close();
        this.letterhead = new MemoryStream(bytes);
      }
    }

    public string PdfFile
    {
      get { return this.pdfFile; }
      set 
      {
        this.pdfFile = value;
        Initialize();
      }
    }
    string pdfFile;
    Stream letterhead;

    public void AddLetterhead(PdfDocument document, int startPage, int endPage, int formPageNumber)
    {
      if (document == null)
        throw new ArgumentNullException("document");
      int pageCount = document.PageCount;
      if (startPage < 1 || startPage > pageCount)
        throw new ArgumentOutOfRangeException("startPage");
      if (endPage < startPage || endPage > pageCount)
        throw new ArgumentOutOfRangeException("endPage");

      int pages = endPage - startPage + 1;
      int[] formPageNumbers = new int[pages];
      for (int idx = 0; idx < pages; idx++)
        formPageNumbers[idx] = formPageNumber;

      AddLetterhead(document, startPage, endPage, formPageNumbers);
    }

    public void AddLetterhead(PdfDocument document, int startPage, int endPage, int[] formPageNumbers)
    {
      if (document == null)
        throw new ArgumentNullException("document");
      int pageCount = document.PageCount;
      if (startPage < 1 || startPage > pageCount)
        throw new ArgumentOutOfRangeException("startPage");
      if (endPage < startPage || endPage > pageCount)
        throw new ArgumentOutOfRangeException("endPage");
      if (endPage - startPage + 1 != formPageNumbers.Length)
        throw new ArgumentException("The value of pageNubers.Length must be endPage - startPage + 1.", "pageNumbers");

      this.letterhead.Position = 0;
      XPdfForm form = XPdfForm.FromStream(this.letterhead);
      int formPageCount = form.PageCount;

      for (int idx = startPage - 1; idx < endPage - 1; idx++)
      {
        PdfPage page = document.Pages[idx];
        XGraphics gfx = XGraphics.FromPdfPage(page);
        int formPage = formPageNumbers[idx];
        Debug.Assert(formPage >= 1 && formPage <=formPageCount, "FormPage number out of range.");
        if (formPage >= 1 && formPage <= formPageCount)
        {
          form.PageNumber = formPage;
          gfx.DrawImage(form, 0, 0);
        }
        gfx.Dispose();
      }
    }

    //const string ImageKey = "/PreviewImage";
  }
}
