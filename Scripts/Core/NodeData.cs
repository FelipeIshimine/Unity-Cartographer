namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class NodeData
	{
		public override string ToString() => GetType().Name.Replace("NodeData",string.Empty);
	}

	public abstract class NodeData<T, TB> : NodeData where T : NodeData<T, TB>, new() where TB : NodeType<TB, T>
	{
	} 

	
}