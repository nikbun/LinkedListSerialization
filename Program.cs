// Сериализация и десериализация двусвязного списка
using System.Text;

internal static class Program
{
	private static void Main(string[] args)
	{
		var fileName = "SerializedListRand";
		var elemintsCount = 5;
		
		var listRand = new ListRand();
		for (int i = 0; i < elemintsCount; i++)
		{
			listRand.AddLast($"Элемент {i}");
		}
		listRand.Randomize();
		listRand.WriteToConsole();
		
		using (FileStream stream = new FileStream(fileName, FileMode.Create))
		{
			listRand.Serialize(stream);
		}

		listRand = new ListRand();
		using (FileStream stream = new FileStream(fileName, FileMode.Open))
		{
			listRand.Deserialize(stream);
		}
		listRand.WriteToConsole();
	}

	class ListNode
	{
		public ListNode Prev;
		public ListNode Next;
		public ListNode Rand; // произвольный элемент внутри списка
		public string Data;
	}

	class ListRand
	{
		public ListNode Head;
		public ListNode Tail;
		public int Count;

		public ListNode AddLast(string dataNode)
		{
			var node = new ListNode() {Data = dataNode};
			AddLast(node);
			return node;
		}

		public void AddLast(ListNode node)
		{
			if (Count == 0)
			{
				Head = node;
				Tail = node;
			}
			else
			{
				Tail.Next = node;
				node.Prev = Tail;
				Tail = node;
			}
			Count++;
		}

		public void WriteToConsole()
		{
			
			Console.WriteLine("--- НАЧАЛО СПИСКА ---");
			
			if (Count == 0)
			{
				Console.WriteLine("ListRand is empty!");
				return;
			}
			
			Console.WriteLine($"Head - {Head.Data}");
			Console.WriteLine($"Tail - {Tail.Data}");
			Console.WriteLine($"Count - {Count}");
			Console.WriteLine("ЭЛЕМЕНТЫ:");
			
			var currentNode = Head;
			do
			{
				Console.WriteLine(currentNode.Data);
				Console.WriteLine($"  Prev - {(currentNode.Prev == null ? "<empty>" : currentNode.Prev.Data)}");
				Console.WriteLine($"  Next - {(currentNode.Next == null ? "<empty>" : currentNode.Next.Data)}");
				Console.WriteLine($"  Rand - {(currentNode.Rand == null ? "<empty>" : currentNode.Rand.Data)}");
				currentNode = currentNode.Next;
			} while (currentNode != null);
			
			Console.WriteLine("--- КОНЕЦ СПИСКА ---");
			Console.WriteLine();
		}

		public void Randomize()
		{
			if (Count <= 0)
			{
				return;
			}
			
			var nodes = new ListNode[Count];
			var currentNode = Head;
			for (int i = 0; i < Count; i++)
			{
				nodes[i] = currentNode;
				currentNode = currentNode.Next;
			}
			
			var random = new Random();
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i].Rand = nodes[random.Next(0, Count)];
			}
		}

		#region Serialization

		public void Serialize(FileStream s)
		{
			if (Count == 0)
			{
				return;
			}

			// Заполняем словарь, чтобы получать доступ к элементам через хэш
			var nodePositionDictionary = new Dictionary<ListNode, int>(Count);
			var currentNode = Head;
			for (int i = 0; i < Count; i++)
			{
				nodePositionDictionary.Add(currentNode, i);
				currentNode = currentNode.Next;
			}

			// Сериализуем
			currentNode = Head;
			for (int i = 0; i < Count; i++)
			{
				if (currentNode.Rand != null 
					&& nodePositionDictionary.TryGetValue(currentNode.Rand, out var randNodePosition))
				{
					WriteInt(randNodePosition, s);
				}
				else
				{
					WriteInt(-1, s);
				}
				WriteString(currentNode.Data, s);
				currentNode = currentNode.Next;
			}
		}

		public void Deserialize(FileStream s)
		{
			if (s.Length == 0)
			{
				return;
			}

			// Восстанавливаем список и формируем список с позициями рандомных нод
			var nodeDataList = new List<NodeData>();
			do
			{
				var randNodePosition = ReadInt(s);
				var node = AddLast(ReadString(s));
				var nodeData = new NodeData(node, randNodePosition);
				nodeDataList.Add(nodeData);
			} while (s.Position < s.Length);
			
			// Восстанавливаем Rand ноды
			for (int i = 0; i < nodeDataList.Count; i++)
			{
				var nodeData = nodeDataList[i];
				var randNode = nodeDataList[nodeData.randNodePosition].node;
				nodeData.node.Rand = randNode;
			}
		}

		private static void WriteString(string value, FileStream stream)
		{
			WriteInt(value.Length, stream);
			for (int i = 0; i < value.Length; i++)
			{
				WriteChar(value[i], stream);
			}
		}

		private static string ReadString(FileStream stream)
		{
			var lenght = ReadInt(stream);
			var stringBuilder = new StringBuilder(lenght);
			for (int i = 0; i < lenght; i++)
			{
				stringBuilder.Append(ReadChar(stream));
			}

			return stringBuilder.ToString();
		}

		private static void WriteInt(int value, FileStream stream)
		{
			for (int shift = 24; shift >= 0; shift -= 8)
			{
				stream.WriteByte((byte) (value >> shift));
			}
		}

		private static int ReadInt(FileStream stream)
		{
			int value = 0;
			for (int shift = 24; shift >= 0; shift -= 8)
			{
				value = value | (stream.ReadByte() << shift);
			}

			return value;
		}

		private static void WriteChar(char value, FileStream stream)
		{
			for (int shift = 8; shift >= 0; shift -= 8)
			{
				stream.WriteByte((byte) (value >> shift));
			}
		}

		private static char ReadChar(FileStream stream)
		{
			int value = 0;
			for (int shift = 8; shift >= 0; shift -= 8)
			{
				value = value | (stream.ReadByte() << shift);
			}

			return (char) value;
		}

		private class NodeData
		{
			public ListNode node;
			public int randNodePosition;

			public NodeData(ListNode node, int randNodePosition)
			{
				this.node = node;
				this.randNodePosition = randNodePosition;
			}
		}

		#endregion
	}
}