using System.IO;

namespace com.itextpdf.io.source
{
	public class ByteArrayOutputStream : MemoryStream
	{
		public ByteArrayOutputStream()
			: base()
		{
		}

		public ByteArrayOutputStream(int size)
			: base(size)
		{
		}

		public virtual void AssignBytes(byte[] bytes, int count)
		{
			SetLength(0);
			Write(bytes, 0, count);
		}
	}
}