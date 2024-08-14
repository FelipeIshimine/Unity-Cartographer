using System.Collections.Generic;
using System.Linq;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	internal class AddRandomNodes : IRoguelikeMapProcessor, ISerializationCallbackReceiver
	{
		[IncreaseButton] public int value = 1;
		[IncreaseButton] public int floorSizeLimit = 5; 

		public void Process(ref RoguelikeMapData data)
		{
			for (int i = 0; i < value; i++)
			{
				int selectedFloorIndex;
				if (floorSizeLimit > 0)
				{
					List<int> validFloors = new List<int>();
					for (int j = 0; j < data.FloorCount; j++)
					{
						var floor = data.GetFloor(j);
						if (floor.Count > floorSizeLimit)
						{
							validFloors.Add(j);
						}
					}
					if (validFloors.Count == 0)
					{
						Debug.LogWarning("Every floor is full. Can't add more nodes");
						break;
					}
					selectedFloorIndex = validFloors[Random.Range(0, validFloors.Count)];
				}
				else
				{
					selectedFloorIndex = Random.Range(0, data.FloorCount);
				}

				data.CreateNodeAtFloor(selectedFloorIndex);
			}
		}


		public void OnBeforeSerialize()
		{
			value = Mathf.Max(1, value);
		}

		public void OnAfterDeserialize()
		{
		}
	}
}