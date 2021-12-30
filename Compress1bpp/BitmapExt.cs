using BmpSharp;

namespace Compress1bpp
{
	public static class BitmapExt
	{
		public static bool GetPixel(this Bitmap b, int i)
		{
			var (x, y) = RleUtil.IdxCoord(i, b.Width);
			return b.PixelData[(x + y * b.Width) * b.BytesPerPixel] > 128;
		}

		public static bool GetPixel(this Bitmap b, int x, int y)
		{
			return b.PixelData[(x + y * b.Width) * b.BytesPerPixel] > 128;
		}

		public static void SetPixel(this Bitmap b, int i, bool set)
		{
			var (x, y) = RleUtil.IdxCoord(i, b.Width);
			b.SetPixel(x, y, set);
		}

		public static void SetPixel(this Bitmap b, int x, int y, bool set)
		{
			var i = (x + y * b.Width) * b.BytesPerPixel;
			var p = (byte)(set ? 255 : 0);
			b.PixelData[i] = p;
			b.PixelData[i + 1] = p;
			b.PixelData[i + 2] = p;
		}

		public static Bitmap FromBitStream(BitStream src, int width, int height)
		{
			var b = new Bitmap(width, height, new byte[3 * width * height]);

			var totalBits = width * height;
			for (var i = 0; i < totalBits; i++)
				b.SetPixel(i, src.ReadBit());

			return b;
		}

		public static void ToBitStream(Bitmap src, BitStream dest)
		{
			var totalBits = src.Width * src.Height;
			
			for (var i = 0; i < totalBits; i++)
				dest.Write(src.GetPixel(i));
		}
	}
}