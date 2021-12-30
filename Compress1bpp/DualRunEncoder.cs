using BmpSharp;

namespace Compress1bpp
{
	public static class DualRunEncoder
	{
		public static void Encode(BitStream src, BitStream dest)
		{
			var payloadBits = src.Length;
			
			var isRlPacket = src.Peek8(2) is 0b00 or 0b11;
			var runLength = 0;
			var runType = src.Peek8(1) == 1;

			dest.Write(isRlPacket);

			for (var i = 0; i < payloadBits; i += 2)
			{
				var p1 = src.ReadBit();

				// Support odd-length images
				if (i + 1 >= payloadBits)
				{
					// terminate RL packet if necessary
					if (isRlPacket)
					{
						RleUtil.PackRunLength(dest, runLength);
						dest.Write(runType);

						// Signal that we're switching packet types
						dest.Write(false);
					}

					// write last pixel
					dest.Write(p1);
					break;
				}

				var p2 = src.ReadBit();

				if (isRlPacket)
				{
					if (p1 ^ p2)
					{
						// Next pair requires a data pair, encode run length
						isRlPacket = false;
						RleUtil.PackRunLength(dest, runLength);
						dest.Write(runType);

						// Signal that we're switching packet types
						dest.Write(false);

						// Start data packet
						dest.Write(p1);
						dest.Write(p2);
					}
					else if (p1 != runType)
					{
						// switching types of runs
						RleUtil.PackRunLength(dest, runLength);
						dest.Write(runType);

						// Signal that we're not switching packet types
						dest.Write(true);

						runType = p1;
						runLength = 1;
					}
					else
						// Next pair continues the run
						runLength++;
				}
				else
				{
					if (p1 ^ p2)
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
						runType = p1;
						runLength = 1;
					}
				}
			}

			// terminate RL packet if necessary
			if (isRlPacket)
			{
				RleUtil.PackRunLength(dest, runLength);
				dest.Write(runType);

				// Signal that we're switching packet types
				dest.Write(false);
			}
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
					var pairType = src.ReadBit();
					isRlPacket = src.ReadBit();

					for (var i = 0; i < nPairs * 2; i++)
					{
						dest.Write(pairType);
						pixelIdx++;
					}
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