using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.Core
{
	[CreateAssetMenu( menuName = "Roguelike Map/Map Blueprint")]
	public class GraphBlueprintAsset : ScriptableObject, IGraphBlueprint
	{
		[SerializeReference,TypeDropdown] 
		public GraphBlueprint Blueprint;
		public GraphData Build() => Blueprint.Build();
	}
}