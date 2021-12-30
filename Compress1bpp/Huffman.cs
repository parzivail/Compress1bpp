using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Compress1bpp
{
	public class Huffman<T>
	{
		private class Node
		{
			public T Data;
			public bool Leaf;
			public int Freq;
			public Node Left;
			public Node Right;
		}

		public Dictionary<T, BitArray> EncodingTable { get; } = new();
		public Dictionary<BitArray, T> DecodingTable { get; } = new();

		public void Build(IEnumerable<T> dataPoints)
		{
			// Create histogram
			var histogram = new Dictionary<T, int>();
			foreach (var p in dataPoints)
			{
				if (!histogram.ContainsKey(p))
					histogram.Add(p, 1);
				else
					histogram[p]++;
			}

			// Create leaves
			var nodes = histogram.Select(pair => new Node { Data = pair.Key, Freq = pair.Value, Leaf = true}).ToList();

			// Build tree
			Node root = null;
			while (nodes.Count > 1)
			{
				var orderedNodes = nodes.OrderBy(node => node.Freq).ToArray();

				if (orderedNodes.Length >= 2)
				{
					// Combine two highest-frequency nodes
					
					var taken = orderedNodes[..2];
					var parent = new Node
					{
						Data = default,
						Leaf = false,
						Freq = taken[0].Freq + taken[1].Freq,
						Left = taken[0],
						Right = taken[1]
					};

					nodes.Remove(taken[0]);
					nodes.Remove(taken[1]);
					nodes.Add(parent);
				}

				root = nodes.FirstOrDefault();
			}

			if (root == null)
				throw new ArgumentException("Unable to create tree", nameof(dataPoints));

			if (root.Right.Leaf && !root.Left.Leaf)
			{
				// enforce the most common symbol being '0'
				var temp = root.Right;
				root.Right = root.Left;
				root.Left = temp;
			}

			// Collect values into encoding table
			void Traverse(Node node, Stack<bool> encoding)
			{
				if (node == null)
					return;
				
				if (node.Leaf)
				{
					var bitArr = new BitArray(encoding.Reverse().ToArray());
					EncodingTable[node.Data] = bitArr;
					DecodingTable[bitArr] = node.Data;
					return;
				}
				
				encoding.Push(false);
				Traverse(node.Left, encoding);
				encoding.Pop();
				
				encoding.Push(true);
				Traverse(node.Right, encoding);
				encoding.Pop();
			}
			
			Traverse(root, new Stack<bool>());
		}

		public BitArray Encode(T key)
		{
			return EncodingTable.TryGetValue(key, out var value) ? value : null;
		}
	}
}