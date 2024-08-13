using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core
{
	public class MapBehaviour : MonoBehaviour, ISerializationCallbackReceiver
	{
		public event Action<MapBehaviour> OnLoad;
		[SerializeField] internal MapData data;
		[SerializeField] private List<Vector3> positions = new List<Vector3>();

		public int Count => data.Count;
		public int EdgesCount => data.EdgesCount;

		internal IReadOnlyList<NodeData> Content => data.Content;

		public void Load(MapData mapData)
		{
			
			data = mapData;
			positions.Clear();
			
			for (int i = 0; i < data.Count; i++)
			{
				positions.Add(Vector3.zero);
			}
			
			OnLoad?.Invoke(this);
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

		public void Remove(int index)
		{
			(positions[index], positions[^1]) = (positions[^1], positions[index]); 
			positions.RemoveAt(positions.Count-1);
			data.RemoveNode(index);
		}

		public void SetWorldPosition(int selectedIndex, Vector3 newPosition)
		{
			positions[selectedIndex] = transform.InverseTransformPoint(newPosition);
		}

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

		public int AddNodeAtWorldPosition(Vector3 worldPosition) => AddNodeAtLocalPosition(transform.InverseTransformPoint(worldPosition));

		public int AddNodeAtLocalPosition(Vector3 localPosition)
		{
			int index=data.CreateNode();
			Debug.Log($"AddNodeAtLocalPosition: {index}: {localPosition}");
			positions.Add(localPosition);
			return index;
		}

		public EdgeData GetEdge(int index) => data.GetEdge(index);

		public bool RemoveEdge(int selectedIndex, int index) => data.Disconnect(selectedIndex, index);

		public void OnBeforeSerialize()
		{
			/*int diff = Count - content.Count;
			if (diff > 0)
			{
				for (int i = 0; i < diff; i++)
				{
					content.Add(null);
				}
			}
			else if(diff < 0)
			{
				for (int i = 0; i > diff; i--)
				{
					content.RemoveAt(content.Count-1);
				}
			}*/
		}

		public void OnAfterDeserialize()
		{
		}

		public NodeData GetContent(int index) => data.Content[index];

		public bool TryGetContent(int index, out NodeData nodeData) => (nodeData = GetContent(index)) != null;

		public IEnumerable<EdgeData> FindAllEdgeIn(int id) => data.FindInEdges(id);
		public IEnumerable<EdgeData> FindAllEdgeOut(int id) => data.FindOutEdges(id);
		
	}
	
	
	
}