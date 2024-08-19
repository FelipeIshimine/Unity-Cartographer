using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cartographer.Core
{
	public class GraphBehaviour : MonoBehaviour, ISerializationCallbackReceiver
	{
		public event Action<GraphBehaviour> OnLoad;
		[SerializeField] public bool showGizmos;
		[SerializeField] public GraphData data;
		[SerializeField] private List<Vector3> localPositions = new List<Vector3>();

		public int Count => data?.Count ?? 0;
		public int EdgesCount => data.Edges.Count;
		public IEnumerable<EdgeData> Edges => data.Edges;

		public void Load(GraphData newData)
		{
			if (data != null)
			{
				Unregister(data);
			}
			
			localPositions.Clear();
			data = newData;
			
			if (data != null)
			{
				Register(data);
				for (int i = 0; i < data.Count; i++)
				{
					localPositions.Add(Vector3.zero);
				}
			}
			
			OnLoad?.Invoke(this);
		}

		private void Unregister(GraphData graphData)
		{
			graphData.OnAddNode -= OnNodeAdded;
			graphData.OnRemoveNode -= OnNodeRemoved;
			graphData.OnSwapNodes -= OnNodeSwapped;
		}

		private void Register(GraphData graphDat)
		{
			graphDat.OnAddNode += OnNodeAdded;
			graphDat.OnRemoveNode += OnNodeRemoved;
			graphDat.OnSwapNodes += OnNodeSwapped;
		}

		private void OnNodeSwapped(int from, int to)
		{
			(localPositions[from], localPositions[to]) = (localPositions[to], localPositions[from]);
		}

		private void OnNodeRemoved()
		{
			localPositions.RemoveAt(localPositions.Count - 1);
		}

		private void OnNodeAdded()
		{
			localPositions.Add(Vector3.zero);
		}

		public IEnumerable<Vector3> GetWorldPositions()
		{
			foreach (Vector3 position in localPositions)
			{
				yield return transform.TransformPoint(position);
			}
		}

		public IEnumerable<Vector3> GetLocalPositions() => localPositions;

		public Vector3 GetLocalPosition(int i) => localPositions[i];

		public Vector3 GetWorldPosition(int i) => transform.TransformPoint(localPositions[i]);

		public void Remove(int index) => data.RemoveNode(index);

		public void SetWorldPosition(int selectedIndex, Vector3 newPosition) => localPositions[selectedIndex] = transform.InverseTransformPoint(newPosition);

		public void SetLocalPosition(int selectedIndex, Vector3 newPosition)
		{
			try
			{
				localPositions[selectedIndex] = newPosition;
			}
			catch
			{
				Debug.Log($"{selectedIndex}/{localPositions.Count}");
				throw;
			}
		}

		public void Connect(int selectedIndex, int targetIndex) => data.Connect(selectedIndex, targetIndex);

		public int AddNodeAtWorldPosition(Vector3 worldPosition)
		{
			return AddNodeAtLocalPosition(transform.InverseTransformPoint(worldPosition));
		}

		public int AddNodeAtLocalPosition(Vector3 localPosition)
		{
			int index = data.AddNode();
			//Debug.Log($"AddNodeAtLocalPosition: {index}: {localPosition}");
			localPositions[index] = localPosition;
			return index;
		}

		public EdgeData GetEdge(int index) => data.Edges[index];

		public bool RemoveEdge(int selectedIndex, int index) => data.Disconnect(selectedIndex, index);

		public NodeData GetContent(int index) => data.Content[index];

		public bool TryGetContent(int index, out NodeData nodeData) => (nodeData = GetContent(index)) != null;

		public IEnumerable<EdgeData> FindEdgesTo(int id) => data.FindEdgesTo(id);
		public IEnumerable<EdgeData> FindEdgesFrom(int id) => data.FindOutEdges(id);

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			Register(data);
		}

		public void Clear()
		{
			Load(new GraphData());
		}

		public IEnumerable<int> GetEveryIndex()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return i;
			}
		}

		public bool TryFindEdge(int from, int to, out EdgeData edge) => data.TryFindEdge(from,to, out edge);

	}
}