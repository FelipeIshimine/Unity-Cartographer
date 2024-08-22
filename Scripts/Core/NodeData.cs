namespace Cartographer.Core
{
	[System.Serializable]
	public abstract record NodeData
	{
		public override string ToString() => GetType().Name.Replace(nameof(NodeData),string.Empty);
	}
}