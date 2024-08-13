using System.Collections.Generic;
using Cartographer.RoguelikeMap.Core.Processors;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Sources
{
	[System.Serializable]
	public class SimpleRoguelikeMap : IRoguelikeMapSource, ISerializationCallbackReceiver
	{
		[field: SerializeField] public Optional<int> RandomSeed = new Optional<int>(0);
		
		[field: SerializeField, IncreaseButton] public int EnterNodes;
		[field: SerializeField, IncreaseButton] public int Floors { get; private set; } = 1;
		[field: SerializeField, IncreaseButton] public int ExitNodes;
		[field: SerializeField, IncreaseButton] public int MaxFloorSize = 5;

		[field: SerializeField, Range(0,1f)] public float NodesDensity = .5f;

		[field: SerializeField] public bool AllowOverlappingPaths { get; set; } = false;
		[field: SerializeField] public bool LimitEdgeByCloseness { get; set; } = false;
		
		public RoguelikeMapData Build()
		{
			List<IRoguelikeMapProcessor> processors = new();

			if (RandomSeed)
			{
				processors.Add(new SetRandomSeed(RandomSeed));
			}

			if (Floors > 0)
			{
				processors.Add(new AddFloors(Floors));
			}
			
			processors.Add(new FillWithNodes(NodesDensity, MaxFloorSize));

			if (EnterNodes>0)
			{
				processors.Add(new AddStartNodes(EnterNodes));
			}

			if (ExitNodes>0)
			{
				processors.Add(new AddEndNodes(ExitNodes));
			}
			
			processors.Add(new ConnectAll(AllowOverlappingPaths, LimitEdgeByCloseness));
			
			var data = new RoguelikeMapData();
			foreach (var processor in processors)
			{
				processor.Process(ref data);
			}
			return data;
		}

		public RoguelikeMapData Get() => Build();
		
		public void OnBeforeSerialize()
		{
			Floors = Mathf.Max(0, Floors);
			MaxFloorSize = Mathf.Max(0,MaxFloorSize);
			EnterNodes = Mathf.Max(EnterNodes,0);
			ExitNodes = Mathf.Max(ExitNodes,0);
		}

		public void OnAfterDeserialize()
		{
		}
	}
}