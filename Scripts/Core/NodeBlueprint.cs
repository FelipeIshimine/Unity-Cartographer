namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class NodeBlueprint
	{
		protected NodeBlueprint(){}
		public abstract NodeData Build();
	}

	[System.Serializable]
	public abstract class NodeBlueprint<T> : NodeBlueprint where T : NodeData
	{
		public override NodeData Build() => OnBuild();
		protected abstract T OnBuild();
	} 
}