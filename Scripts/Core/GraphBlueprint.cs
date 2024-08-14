namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class GraphBlueprint : IGraphBlueprint
	{
		public abstract GraphData Build();
	}

	public interface IGraphBlueprint
	{
		public GraphData Build();
	}

	[System.Serializable]
	public class DefaultGraphBlueprint : GraphBlueprint
	{
		public override GraphData Build()
		{
			throw new System.NotImplementedException();
		}
	}
}
