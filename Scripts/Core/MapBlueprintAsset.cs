using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.Core
{
	[CreateAssetMenu( menuName = "Roguelike Map/Map Blueprint")]
	public class MapBlueprintAsset : ScriptableObject, IMapBlueprint
	{
		[SerializeReference,TypeDropdown] 
		public MapBlueprint Blueprint;
		public MapData Build() => Blueprint.Build();
	}
}