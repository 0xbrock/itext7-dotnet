/*

This file is part of the iText (R) project.
Copyright (c) 1998-2016 iText Group NV
Authors: Bruno Lowagie, Paulo Soares, et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System;
using iText.IO.Source;

namespace iText.Kernel.Pdf {
    public class PdfBoolean : PdfPrimitiveObject {
        public static readonly iText.Kernel.Pdf.PdfBoolean TRUE = new iText.Kernel.Pdf.PdfBoolean(true, true);

        public static readonly iText.Kernel.Pdf.PdfBoolean FALSE = new iText.Kernel.Pdf.PdfBoolean(false, true);

        private static readonly byte[] True = ByteUtils.GetIsoBytes("true");

        private static readonly byte[] False = ByteUtils.GetIsoBytes("false");

        private bool value;

        /// <summary>Store a boolean value</summary>
        /// <param name="value">value to store</param>
        public PdfBoolean(bool value)
            : this(value, false) {
        }

        private PdfBoolean(bool value, bool directOnly)
            : base(directOnly) {
            this.value = value;
        }

        private PdfBoolean()
            : base() {
        }

        public virtual bool GetValue() {
            return value;
        }

        public override byte GetObjectType() {
            return BOOLEAN;
        }

        /// <summary>Marks object to be saved as indirect.</summary>
        /// <param name="document">a document the indirect reference will belong to.</param>
        /// <returns>object itself.</returns>
        public override PdfObject MakeIndirect(PdfDocument document) {
            return (iText.Kernel.Pdf.PdfBoolean)base.MakeIndirect(document);
        }

        /// <summary>Marks object to be saved as indirect.</summary>
        /// <param name="document">a document the indirect reference will belong to.</param>
        /// <returns>object itself.</returns>
        public override PdfObject MakeIndirect(PdfDocument document, PdfIndirectReference reference) {
            return (iText.Kernel.Pdf.PdfBoolean)base.MakeIndirect(document, reference);
        }

        /// <summary>Copies object to a specified document.</summary>
        /// <remarks>
        /// Copies object to a specified document.
        /// Works only for objects that are read from existing document, otherwise an exception is thrown.
        /// </remarks>
        /// <param name="document">document to copy object to.</param>
        /// <returns>copied object.</returns>
        public override PdfObject CopyTo(PdfDocument document) {
            return (iText.Kernel.Pdf.PdfBoolean)base.CopyTo(document, true);
        }

        /// <summary>Copies object to a specified document.</summary>
        /// <remarks>
        /// Copies object to a specified document.
        /// Works only for objects that are read from existing document, otherwise an exception is thrown.
        /// </remarks>
        /// <param name="document">document to copy object to.</param>
        /// <param name="allowDuplicating">
        /// indicates if to allow copy objects which already have been copied.
        /// If object is associated with any indirect reference and allowDuplicating is false then already existing reference will be returned instead of copying object.
        /// If allowDuplicating is true then object will be copied and new indirect reference will be assigned.
        /// </param>
        /// <returns>copied object.</returns>
        public override PdfObject CopyTo(PdfDocument document, bool allowDuplicating) {
            return (iText.Kernel.Pdf.PdfBoolean)base.CopyTo(document, allowDuplicating);
        }

        public override String ToString() {
            return value ? "true" : "false";
        }

        protected internal override void GenerateContent() {
            content = value ? True : False;
        }

        protected internal override PdfObject NewInstance() {
            return new iText.Kernel.Pdf.PdfBoolean();
        }

        protected internal override void CopyContent(PdfObject from, PdfDocument document) {
            base.CopyContent(from, document);
            iText.Kernel.Pdf.PdfBoolean @bool = (iText.Kernel.Pdf.PdfBoolean)from;
            value = @bool.value;
        }

        public override bool Equals(Object obj) {
            return this == obj || obj != null && GetType() == obj.GetType() && value == ((iText.Kernel.Pdf.PdfBoolean)
                obj).value;
        }

        public override int GetHashCode() {
            return (value ? 1 : 0);
        }
    }
}
