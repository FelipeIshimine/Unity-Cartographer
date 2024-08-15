namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class NodeData
	{
		public override string ToString() => GetType().Name.Replace(nameof(NodeData),string.Empty);
	}
}