namespace Cartographer.RoguelikeMap.Core.Processors
{
	public interface IRoguelikeMapProcessor
	{
		public void Process(ref RoguelikeMapData data);
	}
}