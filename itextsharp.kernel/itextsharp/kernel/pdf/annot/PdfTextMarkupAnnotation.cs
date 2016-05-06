/*
$Id: fa2a5d7e1f2e9b88e400fd0055eff9d65915e4f7 $

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
using com.itextpdf.kernel.geom;
using com.itextpdf.kernel.pdf;

namespace com.itextpdf.kernel.pdf.annot
{
	public class PdfTextMarkupAnnotation : PdfMarkupAnnotation
	{
		private const long serialVersionUID = 2189266742204503217L;

		/// <summary>Subtypes</summary>
		public static readonly PdfName MarkupHighlight = PdfName.Highlight;

		public static readonly PdfName MarkupUnderline = PdfName.Underline;

		public static readonly PdfName MarkupStrikeout = PdfName.StrikeOut;

		public static readonly PdfName MarkupSquiggly = PdfName.Squiggly;

		public PdfTextMarkupAnnotation(Rectangle rect, PdfName subtype, float[] quadPoints
			)
			: base(rect)
		{
			Put(PdfName.Subtype, subtype);
			SetQuadPoints(new PdfArray(quadPoints));
		}

		public PdfTextMarkupAnnotation(PdfDictionary pdfObject)
			: base(pdfObject)
		{
		}

		public static com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation CreateHighLight
			(Rectangle rect, float[] quadPoints)
		{
			return new com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation(rect, MarkupHighlight
				, quadPoints);
		}

		public static com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation CreateUnderline
			(Rectangle rect, float[] quadPoints)
		{
			return new com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation(rect, MarkupUnderline
				, quadPoints);
		}

		public static com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation CreateStrikeout
			(Rectangle rect, float[] quadPoints)
		{
			return new com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation(rect, MarkupStrikeout
				, quadPoints);
		}

		public static com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation CreateSquiggly
			(Rectangle rect, float[] quadPoints)
		{
			return new com.itextpdf.kernel.pdf.annot.PdfTextMarkupAnnotation(rect, MarkupSquiggly
				, quadPoints);
		}

		public override PdfName GetSubtype()
		{
			PdfName subType = GetPdfObject().GetAsName(PdfName.Subtype);
			if (subType == null)
			{
				subType = PdfName.Underline;
			}
			return subType;
		}
	}
}