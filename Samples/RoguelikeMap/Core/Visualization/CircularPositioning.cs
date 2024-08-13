using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public class CircularPositioning : NodesPositioning
	{
		public float ringsSeparation = 2;

		[SerializeField,Range(0,1f)] public float progress = 1;
		
		[System.Serializable]
		public enum Direction
		{
			In,
			Out
		}
		public Direction direction;
		
		
		public override void Process(RoguelikeMapBehaviour behaviour)
		{
			float offset = 0;
			if (direction == Direction.Out)
			{
				offset = 0;
			}
			else
			{
				offset = -behaviour.Data.FloorCount * ringsSeparation;
			}

			float mult = Mathf.PI * 2 * progress;
			for (int i = 0; i < behaviour.Data.FloorCount; i++)
			{
				offset += ringsSeparation;
				var floor = behaviour.Data.GetFloor(i);
				for (int j = 0; j < floor.Count; j++)
				{
					//Debug.Log($"{i}/{j}:{floor[j]}");
					var t = ((float)j / floor.Count) * mult;
					var pos = new Vector3(Mathf.Sin(t), 0,Mathf.Cos(t))*offset;
					behaviour.MapBehaviour.SetLocalPosition(floor[j], pos);
				}
			}
		}

		public override void OnDrawGizmos(RoguelikeMapBehaviour behaviour)
		{
			#if UNITY_EDITOR
			var oldMatrix = UnityEditor.Handles.matrix;
			UnityEditor.Handles.matrix = behaviour.transform.localToWorldMatrix;
			UnityEditor.Handles.color = new Color(1, 1, 1, .5f);
			float offset = ringsSeparation/2;
			for (int i = 0; i < behaviour.Data.FloorCount; i++)
			{
				UnityEditor.Handles.DrawWireDisc(Vector3.zero, behaviour.transform.up, offset);
				offset += ringsSeparation;
			}

			UnityEditor.Handles.matrix = oldMatrix;
			#endif
		}
	}
}