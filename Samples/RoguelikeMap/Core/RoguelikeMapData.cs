using System;
using System.Collections.Generic;
using System.Linq;
using Cartographer.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public class RoguelikeMapData : MapData
	{
		public int FloorCount => floors.Count; 
		
		private List<List<int>> floors = new();

		public RoguelikeMapData()
		{
			graph.OnSwapNodes += (x, y) =>
			{
				if (TryFindCoordinates(x, out var xCoord) && TryFindCoordinates(y, out var yCoord))
				{
					(floors[xCoord.x][xCoord.y], floors[yCoord.x][yCoord.y]) = (floors[yCoord.x][yCoord.y], floors[xCoord.x][xCoord.y]);
				}
				else
				{
					Debug.LogError("Not found");
				}
			};
		}

		public IEnumerable<EdgeData> FindRequiredEdges()
		{
			HashSet<EdgeData> edges = new HashSet<EdgeData>();

			foreach (List<int> floor in floors)
			{
				foreach (int i in floor)
				{
					if (graph.TryFindFirstEdgeIn(i, out var edgeData))
					{
						edges.Add(edgeData);
					}

					if (graph.TryFindFirstEdgeOut(i, out edgeData))
					{
						edges.Add(edgeData);
					}
				}
			}

			return edges;

		}

		public List<EdgeData> FindRedundantEdges()
		{
			var required = new List<EdgeData>(FindRequiredEdges());
			return graph.Edges.FindAll(x => !required.Contains(x));
		}

		public void DisconnectAll() => graph.DisconnectAll();

		public IEnumerable<int> GetAvailableParents(int floor, int slot, bool allowOverlappingEdges, bool limitByCloseness)
		{
			if (floor == 0)
			{
				yield break;
			}

			if (allowOverlappingEdges)
			{
				for (var j = 0; j < floors[floor - 1].Count; j++)
				{
					int i = floors[floor - 1][j];
					if (!limitByCloseness ||
					    HaveOverlappingDegrees(new Vector2Int(floor, slot), new Vector2Int(floor - 1, j)))
					{
						yield return i;
					}
				}
			}
			else
			{
				var range = GetAvailableParentsRange(floor, slot);
				var parentsInRange = floors[floor - 1].GetRange(range.x, range.y - range.x);
				for (var j = 0; j < parentsInRange.Count; j++)
				{
					var i = parentsInRange[j];
					if (!limitByCloseness ||
					    HaveOverlappingDegrees(new Vector2Int(floor, slot), new Vector2Int(floor - 1, range.x+j)))
					{
						yield return i;
					}
				}
			}
		}
		
		public IEnumerable<int> GetAvailableChildren(int floor,
		                                             int slot,
		                                             bool allowOverlappingEdges,
		                                             bool limitByCloseness)
		{
			if (floor == floors.Count-1)
			{
				yield break;
			}

			var childrenFloor = floors[floor + 1];
			if (allowOverlappingEdges)
			{
				for (var j = 0; j < childrenFloor.Count; j++)
				{
					var i = childrenFloor[j];
					if (!limitByCloseness ||
					    HaveOverlappingDegrees(new Vector2Int(floor, slot), new Vector2Int(floor + 1, j)))
					{
						yield return i;
					}
				}
			}
			else
			{
				var range = GetAvailableChildrenRange(floor, slot);
				var childrenRange = childrenFloor.GetRange(range.x, range.y - range.x);
				for (var j = 0; j < childrenRange.Count; j++)
				{
					var i = childrenRange[j];
					if (!limitByCloseness ||
					    HaveOverlappingDegrees(new Vector2Int(floor, slot), new Vector2Int(floor + 1, range.x+j)))
					{
						yield return i;
					}
				}
			}
		}

		private Vector2Int GetAvailableParentsRange(int floorIndex, int slotIndex)
		{
			if (floorIndex == 0)
			{
				Debug.LogWarning("Floor is 0. No parents exist");
				return new Vector2Int(0, 0);
			}
			
			var floor = floors[floorIndex];
			var parentFloor = floors[floorIndex-1];

			Vector2Int range = new Vector2Int(0, parentFloor.Count);
			
			if (slotIndex > 0)
			{
				if(TryFindFirstLeftSiblingWithParent(floorIndex,slotIndex, out var siblingIndex))
				{
					range.x = GetHighestParentIndex(floorIndex, siblingIndex);
					if (range.x == -1)
					{
						range.x = 0;
					}
				}
			}

			if (slotIndex < floor.Count - 1)
			{
				if (TryFindFirstRightSiblingWithParent(floorIndex,slotIndex, out var siblingIndex))
				{
					range.y = GetLowestParentIndex(floorIndex, siblingIndex) + 1;
					if (range.y == -1)
					{
						range.y = parentFloor.Count;
					}
				}
			}

			//Debug.Log($"{floorIndex},{slotIndex} => [{range.x},{range.y}]");
			return range;
		}
		
		
		private Vector2Int GetAvailableChildrenRange(int floorIndex, int slotIndex)
		{
			if (floorIndex == floors.Count-1)
			{
				return new Vector2Int(0, 0);
			}
			
			var floor = floors[floorIndex];
			var nextFloor = floors[floorIndex+1];

			Vector2Int range = new Vector2Int(0, nextFloor.Count);
			
			if (slotIndex > 0)
			{
				if(TryFindFirstLeftSiblingWithChildren(floorIndex,slotIndex, out var siblingIndex))
				{
					range.x = GetHighestChildIndex(floorIndex, siblingIndex);
					if (range.x == -1)
					{
						range.x = 0;
					}
				}
			}

			if (slotIndex < floor.Count - 1)
			{
				if(TryFindFirstRightSiblingWithChildren(floorIndex,slotIndex, out var siblingIndex))
				{
					range.y = GetLowestChildIndex(floorIndex, siblingIndex) + 1;
					if (range.y == -1)
					{
						range.y = nextFloor.Count;
					}
				}
			}
			return range;
		}

		private bool TryFindFirstRightSiblingWithChildren(int floorIndex, int slotIndex, out int siblingIndex)
		{
			var floor = floors[floorIndex];
			siblingIndex = -1;
			
			for (int i = slotIndex+1; i < floor.Count; i++)
			{
				Assert.IsTrue(slotIndex != i);
				if (graph.TryFindFirstEdgeOut(floor[i], out _))
				{
					siblingIndex = i;
					return true;
				}
			}
			return false;
		}
		
		private bool TryFindFirstLeftSiblingWithChildren(int floorIndex, int slotIndex, out int siblingIndex)
		{
			var floor = floors[floorIndex];
			siblingIndex = -1;
			
			for (int i = slotIndex-1; i >= 0; i--)
			{
				if (graph.TryFindFirstEdgeOut(floor[i], out _))
				{
					siblingIndex = i;
					return true;
				}
			}
			return false;
		}
		
		
		private bool TryFindFirstRightSiblingWithParent(int floorIndex, int slotIndex, out int siblingIndex)
		{
			var floor = floors[floorIndex];
			siblingIndex = -1;
			
			for (int i = slotIndex+1; i < floor.Count; i++)
			{
				if (graph.TryFindFirstEdgeIn(floor[i], out _))
				{
					siblingIndex = i;
					return true;
				}
			}
			return false;
		}
		
		private bool TryFindFirstLeftSiblingWithParent(int floorIndex, int slotIndex, out int siblingIndex)
		{
			var floor = floors[floorIndex];
			siblingIndex = -1;
			
			for (int i = slotIndex-1; i >= 0; i--)
			{
				Assert.IsTrue(slotIndex != i);
				if (graph.TryFindFirstEdgeIn(floor[i], out _))
				{
					siblingIndex = i;
					return true;
				}
			}
			return false;
		}


		private int GetHighestParentIndex(int floorIndex, int slotIndex)
		{
			if (floorIndex == 0)
			{
				return -1;
			}
			
			var previousFloor = floors[floorIndex - 1];
			var id = floors[floorIndex][slotIndex];

			int resultIndex = int.MinValue;
			foreach (int parentIndex in graph.FindInEdges(id)
			                                 .Select(x => previousFloor.IndexOf(x.From))
			                                 .Where(x => x != -1))
			{
				if (parentIndex > resultIndex)
				{
					resultIndex = parentIndex;
				}
			}

			if (resultIndex == int.MinValue)
			{
				return -1;
			}
			return resultIndex;
		}
		
		private int GetLowestParentIndex(int floorIndex, int slotIndex)
		{
			if (floorIndex == 0)
			{
				return -1;
			}
			
			var previousFloor = floors[floorIndex - 1];
			var id = floors[floorIndex][slotIndex];

			int resultIndex = previousFloor.Count-1;
			foreach (int parentIndex in graph.FindInEdges(id)
			                                 .Select(x => previousFloor.IndexOf(x.From))
			                                 .Where(x => x != -1))
			{
				if (parentIndex < resultIndex)
				{
					resultIndex = parentIndex;
				}
			}
			return resultIndex;
		}
		
		private int GetHighestChildIndex(int floorIndex, int slotIndex)
		{
			if (floorIndex == floors.Count-1)
			{
				return -1;
			}
			
			var nextFloor = floors[floorIndex + 1];
			var id = floors[floorIndex][slotIndex];
			
			int resultIndex = int.MinValue;
			foreach (int childIndex in graph.FindOutEdges(id)
			                                .Select(x => nextFloor.IndexOf(x.To))
			                                .Where(x => x != -1))
			{
				if (childIndex > resultIndex)
				{
					resultIndex = childIndex;
				}
			}
			if (resultIndex == int.MinValue)
			{
				return -1;
			}
			return resultIndex;
		}
		
		private int GetLowestChildIndex(int floorIndex, int slotIndex)
		{
			if (floorIndex == floors.Count-1)
			{
				return -1;
			}
			
			var nextFloor = floors[floorIndex + 1];
			var id = floors[floorIndex][slotIndex];

			int resultIndex = nextFloor.Count-1;
			foreach (int childIndex in graph.FindOutEdges(id)
			                                .Select(x => nextFloor.IndexOf(x.To))
			                                .Where(x => x != -1))
			{
				if (childIndex < resultIndex)
				{
					resultIndex = childIndex;
				}
			}
			return resultIndex;
		}

		public bool ExistsConnection(int from, int to) => graph.Edges.Exists(x => x.From == from && x.To == to);

		public bool HasAnyChildren(int floorIndex, int slotIndex) => graph.TryFindFirstEdgeOut(floors[floorIndex][slotIndex],out _);
		public bool HasAnyParent(int floorIndex, int slotIndex) => graph.TryFindFirstEdgeIn(floors[floorIndex][slotIndex],out _);

		public IEnumerable<int> FindAllEmptyLocations()
		{
			for (int i = 0; i < content.Count; i++)
			{
				if (content[i] == null)
				{
					yield return i;
				}
			}
		}

		public void SetContent(int node, NodeData newData) => content[node] = newData;
		public NodeData GetContent(int node) => content[node];

		public Vector2 GetDegreesRange(int floorIndex, int slotIndex)
		{
			var center = GetDegree(floorIndex, slotIndex, out float extent);
			return new Vector2(center - extent, center + extent);
		}

		private float GetDegree(int floorIndex, int slotIndex, out float extent)
		{
			extent = 1f / (floors[floorIndex].Count + 1);
			return extent + extent * slotIndex;
		}

		public bool HaveOverlappingDegrees(Vector2Int from, Vector2Int to)
		{
			Vector2 a = GetDegreesRange(from.x,from.y);
			Vector2 b = GetDegreesRange(to.x,to.y);

			if ((a.x <= b.x && a.y >= b.y) ||
			    (b.x <= a.x && b.y >= a.y))
			{
				return true;
			}

			if ((a.x >= b.x && a.x <= b.y) ||
			    (a.y >= b.x && a.y <= b.y))
			{
				return true;
			}
			
			if ((b.x >= a.x && b.x <= a.y) ||
			    (b.y >= a.x && b.y <= a.y))
			{
				return true;
			}

			return false;
		}

		public override void RemoveNode(int index)
		{
			if(TryFindCoordinates(index, out Vector2Int coordinates))
			{
				Debug.Log($"RoguelikeMap.RemoveNode:{index} at {coordinates}");
				var floor = floors[coordinates.x];
				base.RemoveNode(index);
				floor.RemoveAt(coordinates.y);
			}
			else
			{
				base.RemoveNode(index);
				Debug.Log($"RoguelikeMap.RemoveNode:{index} Fail");
			}
		}

		private bool TryFindCoordinates(int index, out Vector2Int coordinates)
		{
			for (var floorIndex = 0; floorIndex < floors.Count; floorIndex++)
			{
				var floor = floors[floorIndex];
				for (var nodeIndex = 0; nodeIndex < floor.Count; nodeIndex++)
				{
					var node = floor[nodeIndex];
					if (node == index)
					{
						coordinates = new Vector2Int(floorIndex, nodeIndex);
						return true;
					}
				}
			}
			coordinates = new Vector2Int(-1,-1);
			return false;
		}

		public IReadOnlyList<int> GetFloor(int i) => floors[i];

		public void AddFloor(IEnumerable<int> floor)
		{
			floors.Add(new List<int>());
			int floorIndex = floors.Count - 1;
			foreach (var i in floor)
			{
				AddToFloor(floorIndex,i);
			} 
		}

		public IEnumerable<IReadOnlyList<int>> GetAllFloors() => floors;

		public void CreateNodeAtFloor(int floorIndex) => AddToFloor(floorIndex, CreateNode());

		private void AddToFloor(int floorIndex, int node)
		{
			var floor = floors[floorIndex];
			floor.Add(node);
		}

		public void InsertFloorAt(int floorIndex, IEnumerable<int> floor)
		{
			floors.Insert(floorIndex, new());
			foreach (var node in floor)
			{
				AddToFloor(floorIndex,node);
			}
		}
		
		public void InsertNode(int floorIndex,int floorPosition, int node) => floors[floorIndex].Insert(floorPosition, node);

		public IEnumerable<int> FindFloorsBelowSize(int size)=> FindFloors(x => x.Count < size);
		public IEnumerable<int> FindFloorsAboveSize(int size) => FindFloors(x => x.Count > size);
		public IEnumerable<int> FindFloorsWithSize(int size) => FindFloors(x => x.Count == size);

		public IEnumerable<int> FindFloors(Predicate<IReadOnlyList<int>> predicate)
		{
			for (int i = 0; i < floors.Count; i++)
			{
				if (predicate.Invoke(floors[i]))
				{
					yield return i;
				}
			}
		}
	}


	
}