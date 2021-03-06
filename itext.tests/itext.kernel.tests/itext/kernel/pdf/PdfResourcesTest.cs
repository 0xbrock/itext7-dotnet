using System;
using System.Collections.Generic;
using System.IO;
using iText.IO.Source;
using iText.Kernel.Pdf.Extgstate;

namespace iText.Kernel.Pdf {
    public class PdfResourcesTest {
        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void ResourcesTest1() {
            PdfDocument document = new PdfDocument(new PdfWriter(new MemoryStream()));
            PdfPage page = document.AddNewPage();
            PdfExtGState egs1 = new PdfExtGState();
            PdfExtGState egs2 = new PdfExtGState();
            PdfResources resources = page.GetResources();
            PdfName n1 = resources.AddExtGState(egs1);
            NUnit.Framework.Assert.AreEqual("Gs1", n1.GetValue());
            PdfName n2 = resources.AddExtGState(egs2);
            NUnit.Framework.Assert.AreEqual("Gs2", n2.GetValue());
            n1 = resources.AddExtGState(egs1);
            NUnit.Framework.Assert.AreEqual("Gs1", n1.GetValue());
            document.Close();
        }

        /// <exception cref="System.Exception"/>
        [NUnit.Framework.Test]
        public virtual void ResourcesTest2() {
            MemoryStream baos = new MemoryStream();
            PdfDocument document = new PdfDocument(new PdfWriter(baos));
            PdfPage page = document.AddNewPage();
            PdfExtGState egs1 = new PdfExtGState();
            PdfExtGState egs2 = new PdfExtGState();
            PdfResources resources = page.GetResources();
            resources.AddExtGState(egs1);
            resources.AddExtGState(egs2);
            document.Close();
            PdfReader reader = new PdfReader(new MemoryStream(baos.ToArray()));
            document = new PdfDocument(reader, new PdfWriter(new ByteArrayOutputStream()));
            page = document.GetPage(1);
            resources = page.GetResources();
            ICollection<PdfName> names = resources.GetResourceNames();
            NUnit.Framework.Assert.AreEqual(2, names.Count);
            String[] expectedNames = new String[] { "Gs1", "Gs2" };
            int i = 0;
            foreach (PdfName name in names) {
                NUnit.Framework.Assert.AreEqual(expectedNames[i++], name.GetValue());
            }
            PdfExtGState egs3 = new PdfExtGState();
            PdfName n3 = resources.AddExtGState(egs3);
            NUnit.Framework.Assert.AreEqual("Gs3", n3.GetValue());
            PdfDictionary egsResources = page.GetPdfObject().GetAsDictionary(PdfName.Resources).GetAsDictionary(PdfName
                .ExtGState);
            PdfDictionary e1 = egsResources.GetAsDictionary(new PdfName("Gs1"));
            PdfName n1 = resources.AddExtGState(e1);
            NUnit.Framework.Assert.AreEqual("Gs1", n1.GetValue());
            PdfDictionary e2 = egsResources.GetAsDictionary(new PdfName("Gs2"));
            PdfName n2 = resources.AddExtGState(e2);
            NUnit.Framework.Assert.AreEqual("Gs2", n2.GetValue());
            PdfDictionary e4 = (PdfDictionary)e2.Clone();
            PdfName n4 = resources.AddExtGState(e4);
            NUnit.Framework.Assert.AreEqual("Gs4", n4.GetValue());
            document.Close();
        }
    }
}
