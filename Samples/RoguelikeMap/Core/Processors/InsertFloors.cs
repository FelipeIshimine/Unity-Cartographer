using System.Collections.Generic;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	internal class InsertFloors : IRoguelikeMapProcessor, ISerializationCallbackReceiver
	{
		[IncreaseButton] public int count = 1;
		public int index;
		public void Process(ref RoguelikeMapData data)
		{
			for (int i = 0; i < count; i++)
			{
				var newNodeIndex = data.CreateNode();
				var floor = new List<int>() {  newNodeIndex};
				data.InsertFloorAt(index,floor);
			}
		}

		public void OnBeforeSerialize()
		{
			count = Mathf.Max(1, count);
		}

		public void OnAfterDeserialize()
		{
		}
	}
}