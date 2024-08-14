using UnityEngine;

namespace Cartographer.Core
{
	[RequireComponent(typeof(GraphBehaviour))]
	public class ForceDirectedGraphModifier : MonoBehaviour
	{
		[SerializeField] private GraphBehaviour graphBehaviour;
	
		public Vector2 minMaxInfluenceDistance = new Vector2(1,100);
	
		public float repulsiveForce = 1;
		public float magneticForce = 1;
		public float centerForce = 1;

		
		private void Reset()
		{
			graphBehaviour = GetComponent<GraphBehaviour>();
		}

		void FixedUpdate()
		{
			for (int x = 0; x < graphBehaviour.Count; x++)
			{
				for (int y = x+1; y < graphBehaviour.Count; y++)
				{
					ApplyForceWithSquareDecay(y, x,repulsiveForce);
				}
			}

			for (int i = 0; i < graphBehaviour.EdgesCount; i++)
			{
				var edge = graphBehaviour.GetEdge(i);
				ApplyForce(edge.From,edge.To,-magneticForce);
			}

			for (int i = 0; i < graphBehaviour.Count; i++)
			{
				var xPos = graphBehaviour.GetLocalPosition(i);
				var dir = xPos.normalized;
				xPos -= dir * (Time.deltaTime * centerForce);
				graphBehaviour.SetLocalPosition(i,xPos);
			}
	    
		}

    
		private void ApplyForceWithSquareDecay(int y, int x, float force)
		{
			var yPos = graphBehaviour.GetLocalPosition(y);
			var xPos = graphBehaviour.GetLocalPosition(x);

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

			graphBehaviour.SetLocalPosition(x,xPos);
			graphBehaviour.SetLocalPosition(y,yPos);
		}
	
		private void ApplyForce(int y, int x, float force)
		{
			var yPos = graphBehaviour.GetLocalPosition(y);
			var xPos = graphBehaviour.GetLocalPosition(x);

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

			graphBehaviour.SetLocalPosition(x,xPos);
			graphBehaviour.SetLocalPosition(y,yPos);
		}
	}
}
