using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class MakeFloorsCircular : IRoguelikeMapProcessor
	{
		public void Process(ref RoguelikeMapData data)
		{
			for (int i = 0; i < data.FloorCount; i++)
			{
				var floor = data.GetFloor(i);
				if (floor.Count > 1)
				{
					if(Random.Range(0,2)==0)
					{
						data.graph.Merge(floor[0], floor[^1]);
					}
					else
					{
						data.graph.Merge(floor[^1], floor[0]);
					}
				}
			}
		}
	}
}