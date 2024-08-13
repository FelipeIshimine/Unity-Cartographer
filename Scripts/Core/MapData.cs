using System;
using System.Collections.Generic;
using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.Core
{
	[System.Serializable]
	public class MapData
	{
		public event Action<int> OnAdd;
		public event Action<int> OnRemove;
		public event Action<int,int> OnSwap;
		
		public int Count => content.Count;
		public int EdgesCount => graph.Edges.Count;

		[SerializeField] protected GraphData graph = new();
		[SerializeReference,TypeDropdown] protected internal List<NodeData> content = new();
		public IReadOnlyList<NodeData> Content => content;

		public MapData()
		{
			graph.OnAddNode += () => content.Add(null);
			graph.OnRemoveNode += () => content.RemoveAt(content.Count-1);
			graph.OnSwapNodes += (x, y) => (content[x], content[y]) = (content[y], content[x]);
		}

		public void Connect(int selectedIndex, int targetIndex) => graph.Connect(selectedIndex, targetIndex);
		public bool Disconnect(int selectedIndex, int targetIndex) => graph.Disconnect(selectedIndex, targetIndex);
		public EdgeData GetEdge(int index) => graph.Edges[index];
		public IEnumerable<EdgeData> FindInEdges(int id) => graph.FindInEdges(id);
		public IEnumerable<EdgeData> FindOutEdges(int id) => graph.FindOutEdges(id);

		public virtual void Merge(int x, int y)
		{
			graph.RedirectConnections(y, x);
			RemoveNode(y);
		}

		public int CreateNode(NodeData data = null)
		{
			int value = graph.AddNode();
			content[value] = data;
			OnAdd?.Invoke(value);
			return value;
		}

		public virtual void RemoveNode(int node)
		{
			graph.RemoveNode(node);
			OnRemove?.Invoke(content.Count-1);
		}

		public void Swap(int x, int y)
		{
			graph.Swap(x,y);
			OnSwap?.Invoke(x,y);
		}

		public void RemoveDuplicatedEdges() => graph.RemoveDuplicatedEdges();
	}

	[System.Serializable]
	public record MapFloor
	{
		public int Count => Nodes.Count;
		[field: TypeDropdown] private List<NodeData> Nodes { get; set; } = new List<NodeData>();
	}


	[System.Serializable]
	public struct EdgeData
	{
		public int From;
		public int To;
		public EdgeData(int from, int to)
		{
			From = from;
			To = to;
		}

		public static implicit operator Vector2Int(EdgeData edgeData) => new(edgeData.From, edgeData.To);
		public static implicit operator EdgeData(Vector2Int vector) => new(vector.x, vector.y);
	}

	
}
