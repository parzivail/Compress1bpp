using System;
using System.IO;
using BmpSharp;

namespace Compress1bpp
{
	class Program
	{
		public static void TestSize(string testName, BitStream inputStream, params Action<BitStream, BitStream>[] pipeline)
		{
			var originalSizeBits = inputStream.Length;
			inputStream.Position = 0;

			foreach (var encoder in pipeline)
			{
				inputStream.Position = 0;

				var outputStream = new BitStream();
				encoder(inputStream, outputStream);

				inputStream = outputStream;
			}

			var size = BitsToBytes(inputStream.Length);
			var originalSize = (float)BitsToBytes(originalSizeBits);

			Console.WriteLine($"{testName.PadRight(15)}: {size:N0} {originalSize / size}");
		}

		static void Main(string[] args)
		{
			const string filename = @"X:\TI LCD\160x_densetext.bmp";

			var bmp = BitmapFileHelper.ReadFileAsBitmap(filename);
			
			var bmpStream = new BitStream();
			BitmapExt.ToBitStream(bmp, bmpStream);

			// (1bpp * width * height) bits
			var originalBits = bmpStream.Length;
			Console.WriteLine($"Original bits  : {originalBits:N0} ({BitsToBytes(originalBits):N0} bytes)");

			var huff5 = GetHuffmanEncoder(5);

			TestSize("Single", bmpStream, SingleRunEncoder.Encode);
			TestSize("DE+Single", bmpStream, RleUtil.DeltaEncode, SingleRunEncoder.Encode);
			TestSize("Huff5", bmpStream, huff5);
			TestSize("DE+Huff5", bmpStream, RleUtil.DeltaEncode, huff5);
			TestSize("DE+Huff5+Single", bmpStream, RleUtil.DeltaEncode, huff5, SingleRunEncoder.Encode);
			TestSize("Huff5+DE+Single", bmpStream, huff5, RleUtil.DeltaEncode, SingleRunEncoder.Encode);

			// Output metadata header:
			// [ 00000000 00000000 | 00000000 00000000 | 00000000 ]
			//   Width             | Height            |      ||\_ Enum: 0: single-run encoding, 1: 5-bit huffman encoding (ignored if not compressed)
			//                                                |\__ Flag: delta encoding before compression (ignored if not compressed)
			//                                                \___ Flag: is compressed

			// var r = BitmapExt.FromBitStream(o, img.Width, img.Height);
			// BitmapFileHelper.SaveBitmapToFile(filename + ".out.bmp", r);
		}

		private static Action<BitStream, BitStream> GetHuffmanEncoder(int symbolSize)
		{
			return (i, o) => HuffmanEncoder.Encode(i, o, symbolSize);
		}

		private static int BitsToBytes(int bits)
		{
			return (bits + 7) / 8;
		}
	}
}