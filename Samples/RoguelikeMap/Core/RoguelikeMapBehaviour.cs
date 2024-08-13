using System;
using System.Collections.Generic;
using Cartographer.Core;
using Cartographer.RoguelikeMap.Core.Sources;
using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[RequireComponent(typeof(MapBehaviour))]
	public class RoguelikeMapBehaviour : MonoBehaviour
	{
		[field: SerializeReference, TypeDropdown] public IRoguelikeMapSource Source { get; set; } = new AdvancedRoguelikeMap();
		
		[SerializeField] private MapBehaviour mapBehaviour;
		public MapBehaviour MapBehaviour => mapBehaviour;
		
		[SerializeReference, TypeDropdown] private NodesPositioning visualization = new DefaultPositioning();
        
		[SerializeField] private RoguelikeMapData data;
		public RoguelikeMapData Data => data;
		public int FloorCount => Data.FloorCount;

		private void OnValidate()
		{
			if (mapBehaviour)
			{
				Load();
			}
		}

		private void Reset()
		{
			mapBehaviour = GetComponent<MapBehaviour>();
		}

		private void Awake() => Load();

		public void Load()
		{
			Source ??= new AdvancedRoguelikeMap();
			var data = this.data = Source.Get();
			if (this.data == null)
			{
				Debug.LogWarning("No valid map source");
				return;
			}
			mapBehaviour.Load(this.data);
			visualization ??= new DefaultPositioning();
			visualization.Process(this);
		}

		private void OnDrawGizmosSelected()
		{
			visualization?.OnDrawGizmos(this);
		}
	}
}