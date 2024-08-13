namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class MapBlueprint : IMapBlueprint
	{
		public abstract MapData Build();
	}

	public interface IMapBlueprint
	{
		public MapData Build();
	}

	[System.Serializable]
	public class DefaultMapBlueprint : MapBlueprint
	{
		public override MapData Build()
		{
			throw new System.NotImplementedException();
		}
	}
}
