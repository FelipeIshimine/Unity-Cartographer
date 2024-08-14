using System.Collections.Generic;
using Cartographer.Core;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class AddLocation : IRoguelikeMapProcessor, ISerializationCallbackReceiver
	{
		[SerializeField] private NodeType type;

		[SerializeField, IncreaseButton] private int count;
		[SerializeField,IncreaseButton] private int offset; 
		[SerializeField,IncreaseButton] private int cycle; 
		[SerializeField,IncreaseButton] private int repetition;

		[SerializeField] private bool canOverride = false;
		
		public void Process(ref RoguelikeMapData data)
		{
			if (type == null)
			{
				return;
			}

			
			for (int i = 0; i < count; i++)
			{
				int temp = -offset;
				int counter = 0;

				for (int j = 0; j < data.FloorCount; j++)
				{
					List<int> floor = new List<int>(data.GetFloor(j));

					if (!canOverride)
					{
						floor.Clear();
						foreach (var node in data.GetFloor(j))
						{
							if (data.GetContent(node) == null)
							{
								floor.Add(node);
							}
						}
					}

					if (floor.Count == 0)
					{
						continue;
					}
					temp++;
					if (cycle > 0)
					{
						if (temp % cycle == 0)
						{
							int node = floor[Random.Range(0, floor.Count)];
							data.SetContent(node, type.CreateContent());

							if (repetition > 0 && ++counter == repetition)
							{
								break;
							}
						}
					}
					else
					{
						int node = floor[Random.Range(0, floor.Count)];
						if (temp > 0)
						{
							data.SetContent(node, type.CreateContent());
						}
					}
				}
			}
		}

		public void OnBeforeSerialize()
		{
			count = Mathf.Max(0, count);
		}

		public void OnAfterDeserialize()
		{
		}
	}	
}