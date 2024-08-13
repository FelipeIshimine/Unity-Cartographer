namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class FloorBlueprint : IFloorBlueprint
	{
		public abstract MapFloor Build();
	}

	public interface IFloorBlueprint
	{
		public MapFloor Build();
	}
}