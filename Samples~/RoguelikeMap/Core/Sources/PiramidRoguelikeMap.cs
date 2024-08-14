using System.Collections.Generic;
using Cartographer.RoguelikeMap.Core.Processors;
using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Sources
{
	[System.Serializable]
	public class PiramidRoguelikeMap : IRoguelikeMapSource, ISerializationCallbackReceiver
	{
		[field: SerializeField] public Optional<int> RandomSeed = new Optional<int>(0);
		
		[field: SerializeField, IncreaseButton] public int Floors { get; private set; } = 1;
		public float FloorNodesMultiplier = 2;
		[field: SerializeField, Range(0,1f)] public float NodesDensity = .5f;

		[field: SerializeField] public bool AllowOverlappingPaths { get; set; } = false;
		[field: SerializeField] public bool LimitEdgeByCloseness { get; set; } = true;
		[field: SerializeField] public bool UseFewestConnections { get; set; } = false;
		[field: SerializeField] public bool ConnectSeem { get; set; } = false;

		[field: SerializeField, IncreaseButton] public int ConnectionIterations { get; set; } = 2;
		
		public RoguelikeMapData Build()
		{
			List<IRoguelikeMapProcessor> processors = new();

			if (RandomSeed)
			{
				processors.Add(new SetRandomSeed(RandomSeed));
			}

			for (int i = 0; i < Floors; i++)
			{
				int count = Mathf.RoundToInt(i * FloorNodesMultiplier);
				count = Mathf.Max(Mathf.RoundToInt(count*NodesDensity),1);
				processors.Add(new AddStartNodes(count));
			}

			for (int i = 0; i < ConnectionIterations; i++)
			{
				processors.Add(new ConnectAll(AllowOverlappingPaths,LimitEdgeByCloseness)
				{
					useFewestConnections = UseFewestConnections
				});	
			}

			if (ConnectSeem)
			{
				processors.Add(new MakeFloorsCircular());
			}

			processors.Add(new RemoveDuplicatedPaths());
			
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
		}

		public void OnAfterDeserialize()
		{
		}
	}
}