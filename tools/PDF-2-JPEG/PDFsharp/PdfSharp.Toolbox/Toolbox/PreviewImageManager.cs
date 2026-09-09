using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Ghostscript;

namespace PdfSharp.Toolbox
{
  /// <summary>
  /// Adds preview images for all pages to a PDF file.
  /// Similar to PDF page thumb nail concept, but does not base on it.
  /// </summary>
  public class PreviewImageManager
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewImageManager"/> class with a PDF file.
    /// </summary>
    /// <param name="pdfFile">The PDF file.</param>
    public PreviewImageManager(string pdfFile)
    {
      this.pdfFile = pdfFile;
      Initialize();
    }

    /// <summary>
    /// Opens the PDF file.
    /// </summary>
    void Initialize()
    {
      if (this.document == null)
        this.document = PdfReader.Open(this.pdfFile, PdfDocumentOpenMode.Modify);
    }

    /// <summary>
    /// Gets or sets the PDF file.
    /// </summary>
    /// <value>The PDF file.</value>
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

    /// <summary>
    /// Gets the page count of the PDF file.
    /// </summary>
    /// <value>The page count.</value>
    public int PageCount
    {
      get
      {
        if (this.document != null)
          return this.document.PageCount;
        return 0;
      }
    }

    /// <summary>
    /// Creates preview images for all pages of the document.
    /// </summary>
    /// <param name="resolution">The resolution of the preview images.</param>
    public void CreateImages(int resolution)
    {
      CreateImages(1, this.document.PageCount, resolution);
    }

    /// <summary>
    /// Creates preview images for the specified page range of the document.
    /// </summary>
    /// <param name="startPage">The start page.</param>
    /// <param name="endPage">The end page.</param>
    /// <param name="resolution">The resolution of the preview images.</param>
    public void CreateImages(int startPage, int endPage, int resolution)
    {
      int pageCount = this.document.PageCount;
      if (startPage < 1 || startPage > pageCount)
        throw new ArgumentOutOfRangeException("startPage", startPage, "Value out of range for this document.");
      if (endPage < 1 || endPage > pageCount || endPage < startPage)
        throw new ArgumentOutOfRangeException("endPage", endPage, "Value out of range for this document.");

      GS gs = new GS();
      //Image[] images = gs.PdfToPng(this.pdfFile, startPage, endPage, resolution);
      string[] files = gs.PdfToPngFiles(this.pdfFile, startPage, endPage, resolution);

      for (int pageNumber = startPage, idx = 0; pageNumber <= endPage; pageNumber++, idx++)
      {
        SetImage(pageNumber, files[idx]);
        File.Delete(files[idx]);
      }
      this.document.Save(this.pdfFile);
    }

    /// <summary>
    /// Gets the image associated with this page. 
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>Image, or null if no image exists.</returns>
    public Image GetImage(int pageNumber)
    {
      Image image = null;
      PdfPage page = this.document.Pages[pageNumber - 1];
      PdfCustomValue cust = page.CustomValues[ImageKey];
      if (cust != null)
      {
        MemoryStream stream = new MemoryStream(cust.Value);
        image = Image.FromStream(stream);
        stream.Close();
      }
      return image;
    }

    /// <summary>
    /// Removes all preview images from the PDF file.
    /// </summary>
    public void RemoveAllImages()
    {
      int pageCount = this.document.PageCount;
      for (int idx = 0; idx < pageCount; idx++)
        RemoveImage(idx);
      this.document.Save(this.pdfFile);
    }

    void SetImage(int pageNumber, string filename)
    {
      PdfPage page = this.document.Pages[pageNumber - 1];

      FileStream stream = new FileStream(filename, FileMode.Open);
      int length = (int)stream.Length;
      byte[] bytes = new byte[length];
      stream.Seek(0, SeekOrigin.Begin);
      stream.Read(bytes, 0, length);
      stream.Close();

      PdfCustomValue cust = new PdfCustomValue();
      cust.Value = bytes;
      page.CustomValues[ImageKey] = cust;
    }

    void RemoveImage(int pageNumber)
    {
      PdfPage page = this.document.Pages[pageNumber - 1];
      page.CustomValues[ImageKey] = null;
    }

    PdfDocument document;
    const string ImageKey = "/PreviewImage";
  }
}
