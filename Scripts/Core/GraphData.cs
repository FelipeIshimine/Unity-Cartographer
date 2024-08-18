using System;
using System.Collections.Generic;
using System.Linq;
using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.Core
{
	[System.Serializable]
	public class GraphData
	{
		public event Action OnAddNode; 
		public event Action OnRemoveNode; 
		public event Action<int> OnCountChange; 
		public event Action<int,int> OnSwapNodes;
		public event Action<EdgeData> OnAddEdge; 
		public event Action<EdgeData> OnRemoveEdge; 

		[field:SerializeField] public int Count { get; private set; }
		[field:SerializeField] public List<EdgeData> Edges = new();
		[field:SerializeReference,TypeDropdown] public List<NodeData> Content = new();

		public IEnumerable<EdgeData> FindEdgesTo(int index) => Edges.Where(x => x.To == index);

		public IEnumerable<EdgeData> FindOutEdges(int index) => Edges.Where(x => x.From == index);
		
		public IEnumerable<int> FindInIndices(int index)
		{
			foreach (EdgeData connection in Edges)
			{
				if (connection.To == index)
				{
					yield return connection.From;
				}
			}
		}
		
		public IEnumerable<int> FindOudIndices(int index)
		{
			foreach (EdgeData connection in Edges)
			{
				if (connection.From == index)
				{
					yield return connection.To;
				}
			}
		}

		public int AddNode()
		{
			Count++;
			Content.Add(null);
			OnAddNode?.Invoke();
			OnCountChange?.Invoke(Count);
			return Count - 1;
		}

		public void RemoveNode(int index)
		{
			--Count;
			if (index != Count)
			{
				Swap(index, Count);
			}
			
			for (int i = Edges.Count - 1; i >= 0; i--)
			{
				var edge = Edges[i];
				if (Count == edge.From || Count == edge.To)
				{
					RemoveEdge(i);
				}
			}
			//Debug.Log($"RemoveNode:{Count-1}");
			Content.RemoveAt(Count);
			OnRemoveNode?.Invoke();
			OnCountChange?.Invoke(Count);
		}

		public void Insert(int index)
		{
			AddNode();
			Swap(index,Count-1);
		}

		public void Swap(int from, int to)
		{
			for (var index = 0; index < Edges.Count; index++)
			{
				var edge = Edges[index];

				if (edge.From == from)
				{
					edge.From = to;
				}
				else if (edge.From == to)
				{
					edge.From = from;
				}
				
				if (edge.To == to)
				{
					edge.To = from;
				}
				else if (edge.To == from)
				{
					edge.To = to;
				}
				Edges[index] = edge;
			}
			OnSwapNodes?.Invoke(from, to);
		}

		public int Connect(int selectedIndex, int targetIndex)
		{
			var newEdge = new EdgeData(selectedIndex, targetIndex);
			Edges.Add(newEdge);
			OnAddEdge?.Invoke(newEdge);
			return Edges.Count-1;
		}

		public bool Disconnect(int from, int to)
		{
			for (int i = Edges.Count - 1; i >= 0; i--)
			{
				var edge = Edges[i];
				if (edge.From == from && edge.To == to)
				{
					RemoveEdge(i);
					return true;
				}
			}
			return false;
		}

		public void RemoveEdge(int index)
		{
			var edge = Edges[index];
			Edges.RemoveAt(index);
			OnRemoveEdge?.Invoke(edge);
		}

		public bool TryFindFirstEdgeIn(int i, out EdgeData data)
		{
			foreach (EdgeData edgeData in Edges)
			{
				if (edgeData.To == i)
				{
					data = edgeData;
					return true;
				}
			}
			data = default;
			return false;
		}
		
		public bool TryFindFirstEdgeOut(int i, out EdgeData data)
		{
			foreach (EdgeData edgeData in Edges)
			{
				if (edgeData.From == i)
				{
					data = edgeData;
					return true;
				}
			}
			data = default;
			return false;
		}

		public void DisconnectAll() => Edges.Clear();

		public void RedirectConnections(int from, int to)
		{
			for (int i = Edges.Count - 1; i >= 0; i--)
			{
				var edge = Edges[i];

				if (edge.From == from)
				{
					edge.From = to;
				}
				
				if (edge.To == from)
				{
					edge.To = to;
				}

				Edges[i] = edge;
			}
		}

		public void RemoveDuplicatedEdges()
		{
			Dictionary<int, HashSet<int>> found = new Dictionary<int, HashSet<int>>();

			for (int i = Edges.Count - 1; i >= 0; i--)
			{
				var edge = Edges[i];
				if (!found.TryGetValue(edge.From, out var destinations))
				{
					found[edge.From] = destinations =new HashSet<int>();
				}

				if (!destinations.Add(edge.To))
				{
					Edges.RemoveAt(i);
				}
			}
		}

		public void Merge(int selectedIndex, int targetIndex)
		{
			RedirectConnections(targetIndex,selectedIndex);
			RemoveNode(targetIndex);
		}

		public void FindAllPathsTo(int i, ref HashSet<EdgeData> visited)
		{
			foreach (var edge in FindEdgesTo(i))
			{
				if (visited.Add(edge))
				{
					FindAllPathsTo(edge.From, ref visited);
				}
			}
		}

		public void FindAllPathsFrom(int i, ref HashSet<EdgeData> visited)
		{
			foreach (var edge in FindOutEdges(i))
			{
				if (visited.Add(edge))
				{
					FindAllPathsFrom(edge.To, ref visited);
				}
			}
		}
	}
}