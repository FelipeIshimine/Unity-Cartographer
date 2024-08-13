using UnityEngine;

namespace Cartographer.Core
{
	[RequireComponent(typeof(MapBehaviour))]
	public class ForceDirectedGraphModifier : MonoBehaviour
	{
		[SerializeField] private MapBehaviour mapBehaviour;
	
		public Vector2 minMaxInfluenceDistance = new Vector2(1,100);
	
		public float repulsiveForce = 1;
		public float magneticForce = 1;
		public float centerForce = 1;

		
		private void Reset()
		{
			mapBehaviour = GetComponent<MapBehaviour>();
		}

		void FixedUpdate()
		{
			for (int x = 0; x < mapBehaviour.Count; x++)
			{
				for (int y = x+1; y < mapBehaviour.Count; y++)
				{
					ApplyForceWithSquareDecay(y, x,repulsiveForce);
				}
			}

			for (int i = 0; i < mapBehaviour.EdgesCount; i++)
			{
				var edge = mapBehaviour.GetEdge(i);
				ApplyForce(edge.From,edge.To,-magneticForce);
			}

			for (int i = 0; i < mapBehaviour.Count; i++)
			{
				var xPos = mapBehaviour.GetLocalPosition(i);
				var dir = xPos.normalized;
				xPos -= dir * (Time.deltaTime * centerForce);
				mapBehaviour.SetLocalPosition(i,xPos);
			}
	    
		}

    
		private void ApplyForceWithSquareDecay(int y, int x, float force)
		{
			var yPos = mapBehaviour.GetLocalPosition(y);
			var xPos = mapBehaviour.GetLocalPosition(x);

			var diff = (yPos - xPos);

			if (diff == Vector3.zero)
			{
				return;
			}

			if (diff.magnitude < minMaxInfluenceDistance.x ||
			    diff.magnitude > minMaxInfluenceDistance.y)
			{
				return;
			}
			
			var dir = diff.normalized;

			var displacement = Time.deltaTime * force * (1f / diff.sqrMagnitude);
			xPos -= dir * displacement;
			yPos += dir * displacement;

			mapBehaviour.SetLocalPosition(x,xPos);
			mapBehaviour.SetLocalPosition(y,yPos);
		}
	
		private void ApplyForce(int y, int x, float force)
		{
			var yPos = mapBehaviour.GetLocalPosition(y);
			var xPos = mapBehaviour.GetLocalPosition(x);

			var diff = (yPos - xPos);
			if (diff == Vector3.zero)
			{
				return;
			}
			
			if (diff.magnitude < minMaxInfluenceDistance.x ||
			    diff.magnitude > minMaxInfluenceDistance.y)
			{
				return;
			}
			
			var dir = diff.normalized;

			var displacement = force * Time.deltaTime;
			xPos -= dir * displacement;
			yPos += dir * displacement;

			mapBehaviour.SetLocalPosition(x,xPos);
			mapBehaviour.SetLocalPosition(y,yPos);
		}
	}
}
