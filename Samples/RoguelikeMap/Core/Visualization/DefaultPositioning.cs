using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public class DefaultPositioning : NodesPositioning
	{
		[SerializeField] private Vector2 separation = new Vector2(1, 1);

		public override void Process(RoguelikeMapBehaviour behaviour)
		{
			for (int i = 0; i < behaviour.Data.FloorCount; i++)
			{
				var floor = behaviour.Data.GetFloor(i);

				var startPos = Vector3.left * (floor.Count / 2f * separation.x - separation.x / 2);

				for (int j = 0; j < floor.Count; j++)
				{
					int index = floor[j];

					behaviour.GraphBehaviour.SetLocalPosition(
						index,
						startPos +
						new Vector3(separation.x * j, 0, separation.y * i));
				}
			}
		}
	}
}