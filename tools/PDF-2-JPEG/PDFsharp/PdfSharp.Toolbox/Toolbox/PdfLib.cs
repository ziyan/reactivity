using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;

namespace PdfSharp.Toolbox
{
  public class PdfLib
  {
    public static void ZzzPsgiolePfrp(PdfDictionary.PdfStream stream)
    {
      if (stream.TryUnfilter())
      {
        CSequence seq = ContentReader.ReadContent(stream.Value);
        for (int idx = 0; idx < seq.Count; idx++)
        {
          CObject obj = seq[idx];
          COperator op = obj as COperator;
          if (op != null && op.OpCode.OpCodeName == OpCodeName.Tj)
          {
            if (op.Operands.Count == 1)
            {

              CString s = op.Operands[0] as CString;
              if (s != null && s.Value == "zzzPsgiolePfrp")
              {
                seq.RemoveAt(idx - 5);
                seq.RemoveAt(idx - 5);
                seq.RemoveAt(idx - 5);
                seq.RemoveAt(idx - 5);
                seq.RemoveAt(idx - 5);
                seq.RemoveAt(idx - 5);
                seq.RemoveAt(idx - 5);
                byte[] cont = seq.ToContent();
                stream.Value = cont;
                break;
              }
            }
          }
        }
      }
    }
  }
}