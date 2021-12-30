using System.Collections.Generic;
using BmpSharp;

namespace Compress1bpp
{
	public static class HuffmanEncoder
	{
		public static void Encode(BitStream src, BitStream dest, int symbolSize)
		{
			var data = new List<byte>();
			for (var i = 0; i < src.Length; i += symbolSize) 
				data.Add(src.Read8(symbolSize));

			var h = new Huffman<byte>();
			h.Build(data);
			
			// Don't need to write symbol size itself, it is
			// assumed to be constant for the decoder
			
			dest.Write(h.EncodingTable.Count, symbolSize);

			foreach (var (b, encoding) in h.EncodingTable)
			{
				dest.Write(b, symbolSize);
				dest.Write(encoding.Length, symbolSize);
				dest.Write(encoding);
			}

			foreach (var b in data)
				dest.Write(h.Encode(b));
		}

		public static Bitmap Decode(BitStream src, BitStream dest)
		{
			return null;
		}
	}
}