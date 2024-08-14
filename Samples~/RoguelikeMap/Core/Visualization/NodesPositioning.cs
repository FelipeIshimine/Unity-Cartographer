namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public abstract class NodesPositioning
	{
		public abstract void Process(RoguelikeMapBehaviour behaviour);
		public virtual void OnDrawGizmos(RoguelikeMapBehaviour behaviour){}
	}
}