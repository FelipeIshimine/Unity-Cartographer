using Cartographer.Core;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class FillEmptyLocations : IRoguelikeMapProcessor
	{
		[SerializeField] private NodeType type;
		
		public void Process(ref RoguelikeMapData data)
		{
			if(type == null)
			{return;}
			foreach (int node in data.FindAllEmptyLocations())
			{
				data.SetContent(node,type.CreateContent());
			}
		}
	}
}