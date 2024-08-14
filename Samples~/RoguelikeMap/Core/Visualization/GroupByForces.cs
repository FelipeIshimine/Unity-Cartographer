using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public class GroupByForces : NodesPositioning
	{
		public GraphSolverSettings Settings;
		public int solverSteps = 200;
		private DefaultPositioning defaultPositioning;
		public override void Process(RoguelikeMapBehaviour behaviour)
		{
			defaultPositioning ??= new DefaultPositioning();
			defaultPositioning.Process(behaviour);
			var solver = new RoguelikeGraphForceSolver(Settings, behaviour);
			for (int i = 0; i < solverSteps; i++)
			{
				solver.Step(Time.fixedDeltaTime);
			}
		}
	}
}