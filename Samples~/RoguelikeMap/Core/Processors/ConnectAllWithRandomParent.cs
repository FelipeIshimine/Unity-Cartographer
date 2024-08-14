using System.Collections.Generic;
using System.Linq;
using Cartographer.Core;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class ConnectAllWithRandomParent : IRoguelikeMapProcessor
	{
		public bool allowOverlappingEdges;
		public bool limitByCloseness = false;
		public bool limitToOne = true;
		//public bool allowLoopEdge;
		
		public ConnectAllWithRandomParent()
		{
		}

		public ConnectAllWithRandomParent(bool b, bool limitEdgeByCloseness)
		{
			allowOverlappingEdges = b;
			limitByCloseness = limitEdgeByCloseness;
		}

		public void Process(ref RoguelikeMapData data)
		{
			List<(int floorIndex, int slotIndex)> remaining = new List<(int floorIndex, int slotIndex)>();

			for (int x = 0; x < data.FloorCount; x++)
			{
				var floor = data.GetFloor(x);
				for (int y = 0; y < floor.Count; y++)
				{
					remaining.Add((x, y));
				}
			}

			remaining.Shuffle();

			while (remaining.Count > 0)
			{
				var (floorIndex, slotIndex) = remaining[^1];
				remaining.RemoveAt(remaining.Count-1);
				
				if (limitToOne && data.HasAnyParent(floorIndex, slotIndex))
				{
					continue;
				}

				var availableParents = data.GetAvailableParents(
					floorIndex,
					slotIndex,
					allowOverlappingEdges, 
					limitByCloseness).ToArray();
				var floor = data.GetFloor(floorIndex);

				if(availableParents.Length > 0)
				{
					var parentID = availableParents[Random.Range(0, availableParents.Length)];
						
					if(!data.ExistsConnection(parentID, floor[slotIndex]))
					{
						data.graph.Connect(parentID, floor[slotIndex]);
					}
                        
				}
			}
		}
	}
}