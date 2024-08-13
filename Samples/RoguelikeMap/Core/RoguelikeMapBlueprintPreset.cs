using Cartographer.RoguelikeMap.Core.Sources;
using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[CreateAssetMenu(menuName = "Cartographer/Blueprints/Roguelike")]
	public class RoguelikeMapBlueprintPreset : ScriptableObject
	{
		[field: SerializeReference, TypeDropdown] public IRoguelikeMapSource Source { get; set; } = new AdvancedRoguelikeMap();
	}
}