namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class RemoveDuplicatedPaths : IRoguelikeMapProcessor
	{
		public void Process(ref RoguelikeMapData data)
		{
			data.graph.RemoveDuplicatedEdges();
		}
	}
}