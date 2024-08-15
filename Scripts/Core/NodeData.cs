namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class NodeData
	{
		public override string ToString() => GetType().Name.Replace(nameof(NodeData),string.Empty);
		protected abstract NodeType GetNodeType();
	}

	public abstract class NodeData<TData, TType> : NodeData where TData : NodeData<TData, TType>, new() where TType : NodeType<TType, TData>
	{
		protected override NodeType GetNodeType() => NodeType;

		private static TType nodeType;
		public static TType NodeType => nodeType ??= CartographerData.GetNodeType<TType>();
	}
}