using System.Collections.Generic;
using Cartographer.RoguelikeMap.Core.Processors;
using Cartographer.Utilities;
using Cartographer.Utilities.Attributes;

namespace Cartographer.RoguelikeMap.Core.Sources
{
	[System.Serializable,DropdownPath("AdvancedProcedural")]
	public class AdvancedRoguelikeMap : IRoguelikeMapSource
	{
		public List<OptionalReference<IRoguelikeMapProcessor>> Processors = new();
		internal RoguelikeMapData data = new();
		public RoguelikeMapData Data => data;
		
		public RoguelikeMapData Build()
		{
			data = new RoguelikeMapData();
			foreach (var processor in Processors)
			{
				if (processor && processor.Value != null)
				{
					processor.Value.Process(ref data);
				}
			}
			return data;
		}

		public RoguelikeMapData Get() => Build();
	}
}