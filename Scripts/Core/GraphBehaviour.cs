using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core
{
	public class GraphBehaviour : MonoBehaviour, ISerializationCallbackReceiver
	{
		public event Action<GraphBehaviour> OnLoad;
		
		[SerializeField] public GraphData data;
		[SerializeField] private List<Vector3> positions = new List<Vector3>();

		public int Count => data.Count;
		public int EdgesCount => data.Edges.Count;

		public void Load(GraphData newData)
		{
			if (data != null)
			{
				Unregister(data);
			}
			
			positions.Clear();
			data = newData;
			
			if (data != null)
			{
				Register(data);
				for (int i = 0; i < data.Count; i++)
				{
					positions.Add(Vector3.zero);
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
			(positions[from], positions[to]) = (positions[to], positions[from]);
		}

		private void OnNodeRemoved()
		{
			positions.RemoveAt(positions.Count - 1);
		}

		private void OnNodeAdded()
		{
			positions.Add(Vector3.zero);
		}

		public IEnumerable<Vector3> GetWorldPositions()
		{
			foreach (Vector3 position in positions)
			{
				yield return transform.TransformPoint(position);
			}
		}

		public IEnumerable<Vector3> GetLocalPositions() => positions;

		public Vector3 GetLocalPosition(int i) => positions[i];

		public Vector3 GetWorldPosition(int i) => transform.TransformPoint(positions[i]);

		public void Remove(int index) => data.RemoveNode(index);

		public void SetWorldPosition(int selectedIndex, Vector3 newPosition) => positions[selectedIndex] = transform.InverseTransformPoint(newPosition);

		public void SetLocalPosition(int selectedIndex, Vector3 newPosition)
		{
			try
			{
				positions[selectedIndex] = newPosition;
			}
			catch
			{
				Debug.Log($"{selectedIndex}/{positions.Count}");
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
			positions[index] = localPosition;
			return index;
		}

		public EdgeData GetEdge(int index) => data.Edges[index];

		public bool RemoveEdge(int selectedIndex, int index) => data.Disconnect(selectedIndex, index);

		public NodeData GetContent(int index) => data.Content[index];

		public bool TryGetContent(int index, out NodeData nodeData) => (nodeData = GetContent(index)) != null;

		public IEnumerable<EdgeData> FindAllEdgeIn(int id) => data.FindInEdges(id);
		public IEnumerable<EdgeData> FindAllEdgeOut(int id) => data.FindOutEdges(id);

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
	}
}