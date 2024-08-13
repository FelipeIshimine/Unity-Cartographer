using System;
using Cartographer.Core;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public class RoguelikeGraphForceSolver
	{
		private Vector3[] targetVelocity = Array.Empty<Vector3>();
		private Vector3[] velocity = Array.Empty<Vector3>();
		private readonly GraphSolverSettings graphSolverSettings;
		private readonly RoguelikeMapBehaviour roguelikeMap;

		public RoguelikeGraphForceSolver(GraphSolverSettings graphSolverSettings, RoguelikeMapBehaviour roguelikeMap)
		{
			this.graphSolverSettings = graphSolverSettings;
			this.roguelikeMap = roguelikeMap;
		}

		public void Step(float deltaTime)
		{
			RefreshLength(roguelikeMap.MapBehaviour);

			ResetTargetVelocity();

			for (int i = 0; i < roguelikeMap.MapBehaviour.EdgesCount; i++)
			{
				var edge = roguelikeMap.MapBehaviour.GetEdge(i);
				ApplyForce(edge.From,edge.To,-graphSolverSettings.magneticForce, deltaTime,roguelikeMap.MapBehaviour);
			}
			
			for (int i = 0; i < roguelikeMap.FloorCount; i++)
			{
				var floor = roguelikeMap.Data.GetFloor(i);
				for (int x = 0; x < floor.Count; x++)
				{
					for (int y = x+1; y < floor.Count; y++)
					{
						ApplyForceWithSquareDecay(floor[x],floor[y], graphSolverSettings.repulsiveForce, deltaTime,roguelikeMap.MapBehaviour);
					}	
				}
			}
			ApplyVelocity(deltaTime, roguelikeMap.MapBehaviour);
		}

		private void ResetTargetVelocity()
		{
			for (int i = 0; i < targetVelocity.Length; i++)
			{
				targetVelocity[i] = Vector3.zero;
			}
		}

		private void ApplyVelocity(float deltaTime, MapBehaviour mapBehaviour)
		{
			for (int i = 0; i < targetVelocity.Length; i++)
			{
				velocity[i] = Vector3.MoveTowards(velocity[i], targetVelocity[i], deltaTime * graphSolverSettings.acceleration);
				mapBehaviour.SetLocalPosition(i,mapBehaviour.GetLocalPosition(i) + velocity[i]);
			}
		}

		private void RefreshLength(MapBehaviour mapBehaviour)
		{
			if (targetVelocity.Length != mapBehaviour.Count)
			{
				Array.Resize(ref targetVelocity, mapBehaviour.Count);
				Array.Resize(ref velocity, mapBehaviour.Count);
			}
		}

		private void ApplyForceWithSquareDecay(int y, int x, float force, float deltaTime, MapBehaviour mapBehaviour)
		{
			var yPos = mapBehaviour.GetLocalPosition(y);
			var xPos = mapBehaviour.GetLocalPosition(x);
			
			var diff = (yPos - xPos);
			if (diff.magnitude < graphSolverSettings.minMaxInfluenceDistance.x ||
			    diff.magnitude > graphSolverSettings.minMaxInfluenceDistance.y)
			{
				return;
			}
			if (diff == Vector3.zero)
			{
				return;
			}
			
			var dir = diff.normalized;

			var displacement = deltaTime * force * (1f / diff.sqrMagnitude);

			targetVelocity[x] -= dir * displacement;
			targetVelocity[y] += dir * displacement;
			
		}

		private void ApplyForce(int y, int x, float force, float deltaTime, MapBehaviour mapBehaviour)
		{
			var yPos = mapBehaviour.GetLocalPosition(y);
			var xPos = mapBehaviour.GetLocalPosition(x);

			
			var diff = (yPos - xPos);
			if (diff.magnitude < graphSolverSettings.minMaxInfluenceDistance.x ||
			    diff.magnitude > graphSolverSettings.minMaxInfluenceDistance.y)
			{
				return;
			}
			
			diff.z = 0;
			
			if (diff == Vector3.zero)
			{
				return;
			}
			
			var dir = diff.normalized;

			var displacement = force * deltaTime;
			
			targetVelocity[x] -= dir * displacement;
			targetVelocity[y] += dir * displacement;
		}
	}

	[System.Serializable]
	public class GraphSolverSettings
	{
		public Vector2 minMaxInfluenceDistance = new Vector2(0,100);
		public float repulsiveForce = 0.75f;
		public float magneticForce = 0.5f;
		[SerializeField] public float acceleration=0.1f;
	}
}