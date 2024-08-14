using Cartographer.Core;
using Cartographer.RoguelikeMap.Core.Sources;
using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[RequireComponent(typeof(GraphBehaviour))]
	public class RoguelikeMapBehaviour : MonoBehaviour, ISerializationCallbackReceiver
	{
		[field: SerializeReference, TypeDropdown] public IRoguelikeMapSource Source { get; set; } = new AdvancedRoguelikeMap();
		
		[SerializeField] private GraphBehaviour graphBehaviour;
		public GraphBehaviour GraphBehaviour => graphBehaviour;
		
		[SerializeReference, TypeDropdown] private NodesPositioning visualization = new DefaultPositioning();
        
		[SerializeField] private RoguelikeMapData data;
		public RoguelikeMapData Data => data;
		public int FloorCount => Data.FloorCount;

		private void OnValidate()
		{
			if (graphBehaviour)
			{
				Load();
			}
		}

		private void Reset()
		{
			graphBehaviour = GetComponent<GraphBehaviour>();
		}

		private void Awake() => Load();

		public void Load()
		{
			Source ??= new AdvancedRoguelikeMap();
			data = Source.Get();
			if (data == null)
			{
				Debug.LogWarning("No valid map source");
				return;
			}
			graphBehaviour.Load(data.graph);
			visualization ??= new DefaultPositioning();
			visualization.Process(this);
		}

		private void OnDrawGizmosSelected()
		{
			visualization?.OnDrawGizmos(this);
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			graphBehaviour.Load(data.graph);
		}
	}
}