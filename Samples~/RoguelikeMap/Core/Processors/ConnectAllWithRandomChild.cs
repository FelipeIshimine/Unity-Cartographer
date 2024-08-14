using System.Collections.Generic;
using System.Linq;
using Cartographer.Core;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class ConnectAllWithRandomChild : IRoguelikeMapProcessor
	{
		public bool allowOverlappingEdges;
		public bool limitByCloseness = false;
		public bool limitToOne = true;

		public ConnectAllWithRandomChild()
		{
		}
		public ConnectAllWithRandomChild(bool b, bool limitEdgeByCloseness)
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

				if (limitToOne && data.HasAnyChildren(floorIndex, slotIndex))
				{
					continue;
				}
				
				var available = data.GetAvailableChildren(
					floorIndex,
					slotIndex,
					allowOverlappingEdges,
					limitByCloseness).ToArray();
				var floor = data.GetFloor(floorIndex);

				if(available.Length > 0)
				{
					var childID = available[Random.Range(0, available.Length)];
						
					if(!data.ExistsConnection(floor[slotIndex], childID))
					{
						data.graph.Connect(floor[slotIndex], childID);
					}
                        
				}
			}
		}
	}
}