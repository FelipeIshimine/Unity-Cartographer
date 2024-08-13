using System.Collections.Generic;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	internal class AddStartNodes : IRoguelikeMapProcessor, ISerializationCallbackReceiver
	{
		[IncreaseButton] public int count = 1;

		public AddStartNodes()
		{
		}

		public AddStartNodes(int count)
		{
			this.count = count;
		}

		public void Process(ref RoguelikeMapData data)
		{
			var floor = new List<int>();
			for (int i = 0; i < count; i++)
			{
				floor.Add(data.CreateNode());
			}
			data.InsertFloorAt(0,floor);	
		}

		public void OnBeforeSerialize()
		{
			count = Mathf.Max(count, 1);
		}

		public void OnAfterDeserialize()
		{
		}
	}
}