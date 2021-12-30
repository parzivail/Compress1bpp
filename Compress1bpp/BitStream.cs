using System;
using System.Collections;
using System.Text;

namespace Compress1bpp
{
	public class BitStream
	{
		private byte[] _stream;
		private int _position;
		
		public int Length { get; private set; }

		public int Position
		{
			get => _position;
			set
			{
				_position = value;
				if (_position > Length)
					Length = _position;
				while (_position >= _stream.Length * 8)
					Array.Resize(ref _stream, _stream.Length * 2);
			}
		}

		public BitStream()
		{
			_stream = new byte[1];
		}

		private static byte Mask8(int bit)
		{
			return (byte)(1 << (7 - bit));
		}

		private static int Mask32(int bit)
		{
			return 1 << (31 - bit);
		}

		public void Write(int container, int lowerN)
		{
			for (var i = 0; i < lowerN; i++) 
				Write((container & Mask32(32 - lowerN + i)) > 0);
		}

		public void Write(bool bit)
		{
			if (bit)
				_stream[Position / 8] |= Mask8(Position % 8);
			Position++;
		}

		public void Write(BitArray data)
		{
			foreach (bool b in data)
				Write(b);
		}

		public bool ReadBit()
		{
			var ret = BitAt(Position);
			Position++;
			return ret;
		}

		private bool BitAt(int position)
		{
			return (_stream[position / 8] & Mask8(position % 8)) > 0;
		}

		public byte Peek8(int n)
		{
			var b = 0;

			for (var i = 0; i < n; i++)
			{
				b <<= 1;
				b |= BitAt(Position + i) ? 1 : 0;
			}
			
			return (byte)b;
		}

		public byte Read8(int n)
		{
			var b = Peek8(n);
			Position += n;
			
			return b;
		}

		public override string ToString()
		{
			var oldCursor = Position;

			var sb = new StringBuilder();
			for (Position = 0; Position < oldCursor;)
				sb.Append(ReadBit() ? "1" : "0");

			Position = oldCursor;

			return sb.ToString();
		}
	}
}