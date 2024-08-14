using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class SetRandomSeed : IRoguelikeMapProcessor
	{
		[SerializeField] private int value;

		public SetRandomSeed()
		{
		}

		public SetRandomSeed(int value)
		{
			this.value = value;
		}

		public void Process(ref RoguelikeMapData data) => UnityEngine.Random.InitState(value);
	}
}