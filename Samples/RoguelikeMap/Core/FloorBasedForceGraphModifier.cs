using Cartographer.Utilities;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[RequireComponent(typeof(RoguelikeMapBehaviour))]
	public class FloorBasedForceGraphModifier : MonoBehaviour
	{
		public Optional<int> stepsLimit = new Optional<int>(100);

		private int steps = 0;

		[SerializeField] private bool resolveInOneFrame = true;
		[SerializeField] private RoguelikeMapBehaviour roguelikeMapBehaviour;
		[SerializeField] private GraphSolverSettings solverSettings;
		private RoguelikeGraphForceSolver solver;

		private void Reset()
		{
			roguelikeMapBehaviour = GetComponent<RoguelikeMapBehaviour>();
		}

		private void OnValidate()
		{
			solver = new RoguelikeGraphForceSolver(solverSettings,roguelikeMapBehaviour); 
		}

		void FixedUpdate()
		{
			solver ??= new RoguelikeGraphForceSolver(solverSettings,roguelikeMapBehaviour);
			if (stepsLimit)
			{
				if (resolveInOneFrame)
				{
					for (int i = 0; i < stepsLimit; i++)
					{
						solver.Step(Time.fixedDeltaTime);
					}
					enabled = false;
					return;
				}
				if (steps++ == stepsLimit)
				{
					enabled = false;
				}
			}

			solver.Step(Time.fixedDeltaTime);
		}
	}
}