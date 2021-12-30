using BmpSharp;

namespace Compress1bpp
{
	public static class SingleRunEncoder
	{
		public static void Encode(BitStream src, BitStream dest)
		{
			var payloadBits = src.Length;
			
			var isRlPacket = src.Peek8(2) == 0;
			var runLength = 0;

			dest.Write(isRlPacket);

			for (var i = 0; i < payloadBits; i += 2)
			{
				var p1 = src.ReadBit();

				// Support odd-length images
				if (i + 1 >= payloadBits)
				{
					// terminate RL packet if necessary
					if (isRlPacket)
						RleUtil.PackRunLength(dest, runLength);

					// write last pixel
					dest.Write(p1);
					break;
				}

				var p2 = src.ReadBit();

				if (isRlPacket)
				{
					if (p1 || p2)
					{
						// Next pair requires a data pair, encode run length
						isRlPacket = false;
						RleUtil.PackRunLength(dest, runLength);

						// Start data packet
						dest.Write(p1);
						dest.Write(p2);
					}
					else
						// Next pair continues the run
						runLength++;
				}
				else
				{
					if (p1 || p2)
					{
						// Next pair continues data packet
						dest.Write(p1);
						dest.Write(p2);
					}
					else
					{
						// Next pair starts a run, write data packet terminator
						dest.Write(0, 2);

						// Start accumulating run
						isRlPacket = true;
						runLength = 1;
					}
				}
			}

			// terminate RL packet if necessary
			if (isRlPacket) 
				RleUtil.PackRunLength(dest, runLength);
		}

		public static void Decode(BitStream src, BitStream dest)
		{
			var payloadBits = src.Length;
			var pixelIdx = 0;

			var isRlPacket = src.ReadBit();

			while (pixelIdx < payloadBits - 1)
			{
				if (isRlPacket)
				{
					var nPairs = RleUtil.UnpackRunLength(src);
					
					for (var i = 0; i < nPairs * 2; i++)
					{
						dest.Write(false);
						pixelIdx++;
					}
					
					isRlPacket = false;
				}
				else
				{
					var b1 = src.ReadBit();
					var b2 = src.ReadBit();

					if (b1 || b2)
					{
						dest.Write(b1);
						dest.Write(b2);
						pixelIdx += 2;
					}
					else
						isRlPacket = true;
				}
			}

			if (pixelIdx != payloadBits)
			{
				var oddBit = src.ReadBit();
				dest.Write(oddBit);
			}
		}
	}
}