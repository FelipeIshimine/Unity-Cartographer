using Cartographer.Utilities.Attributes;

namespace Cartographer.RoguelikeMap.Core.Sources
{
	[System.Serializable,DropdownPath("Preset")]
	public class PresetRoguelikeMap : IRoguelikeMapSource
	{
		public RoguelikeMapBlueprintPreset preset;
		public RoguelikeMapData Get() => preset ? preset.Source.Get() : null;
	}
}