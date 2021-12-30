namespace Compress1bpp
{
	public class RleUtil
	{
		public static (int x, int y) IdxCoord(int i, int width)
		{
			var x = i % width;
			var y = i / width;
			return (x, y);
		}

		public static int UnpackRunLength(BitStream s)
		{
			var l = 0;
			var v = 0;

			while (s.ReadBit())
			{
				l <<= 1;
				l |= 1;
			}

			l <<= 1;

			var numBits = Log2(l);
			for (var i = 0; i <= numBits; i++)
			{
				v <<= 1;
				if (s.ReadBit()) v |= 1;
			}

			return l + v + 1;
		}

		public static void PackRunLength(BitStream s, int x)
		{
			x++;
			var l2 = Log2(x);
			var highBit = 1 << l2;
			var encodedL = highBit - 2;
			var encodedV = x - highBit;

			s.Write(encodedL, l2);
			s.Write(encodedV, l2);
		}

		private static int Log2(int x)
		{
			var r = 0;
			while ((x >>= 1) > 0) r++;
			return r;
		}

		public static void DeltaEncode(BitStream src, BitStream dest)
		{
			var prevPixel = false;
			for (var i = 0; i < src.Length; i++)
			{
				var pixel = src.ReadBit();
				dest.Write(pixel ^ prevPixel);
				prevPixel = pixel;
			}
		}

		public static void DeltaDecode(BitStream src, BitStream dest)
		{
			var prevPixel = false;
			for (var i = 0; i < src.Length; i++)
			{
				var pixel = src.ReadBit();
				prevPixel ^= pixel;
				dest.Write(prevPixel);
			}
		}
	}
}