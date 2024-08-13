using System.Collections.Generic;
using System.Linq;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	internal class FillWithNodes : IRoguelikeMapProcessor, ISerializationCallbackReceiver
	{
		public float density = .5f;
		public Optional<int> floorMaxSize = new Optional<int>(5);

		public FillWithNodes()
		{
		}

		public FillWithNodes(float density, int maxSize=0)
		{
			this.density = density;
			floorMaxSize = maxSize > 0 ? new Optional<int>(maxSize,true) : new Optional<int>(1,false);
		}

		public void Process(ref RoguelikeMapData data)
		{
			var nodesToAdd = CalculateNodesToAdd(data);

			for (int i = 0; i < nodesToAdd; i++)
			{
				int selectedFloorIndex;
				if (floorMaxSize)
				{
					var validFloors = new List<int>(data.FindFloors(x => x.Count < floorMaxSize));
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

		private int CalculateNodesToAdd(RoguelikeMapData data)
		{
			int nodesToAdd = 0;
			foreach (var floor in data.GetAllFloors())
			{
				nodesToAdd += Mathf.Max(floorMaxSize - floor.Count, 0);
			}
			nodesToAdd = Mathf.RoundToInt(nodesToAdd*density);
			return nodesToAdd;
		}

		public void OnBeforeSerialize()
		{
			floorMaxSize.SetValue(Mathf.Max(floorMaxSize, 1));
		}

		public void OnAfterDeserialize()
		{
		}
	}
}