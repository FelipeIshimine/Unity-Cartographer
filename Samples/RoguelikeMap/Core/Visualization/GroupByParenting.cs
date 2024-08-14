using System.Collections.Generic;
using Cartographer.Core;
using UnityEngine;

namespace Cartographer.RoguelikeMap.Core
{
	[System.Serializable]
	public class GroupByParenting : NodesPositioning
	{
		public Vector2 separation = new Vector2(1, 1);
		
		[SerializeField] private float groupMargin = 1;

		
		public override void Process(RoguelikeMapBehaviour behaviour)
		{
			List<List<int[]>> floors = new ();

			for (int i = 0; i < behaviour.FloorCount; i++)
			{
				floors.Add(new List<int[]>(CalculateGroups(behaviour.GraphBehaviour, behaviour.Data.GetFloor(i))));
			}

			for (var floorIndex = 0; floorIndex < floors.Count; floorIndex++)
			{
				var groups = floors[floorIndex];

				float totalWidth = 0;
				float[] groupWidths = new float[groups.Count];
				
				for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
				{
					var group = groups[groupIndex];
					float groupWidth = groupWidths[groupIndex] = separation.x * (group.Length - 1) + groupMargin * 2;
					totalWidth += groupWidth;
				}

				float position = -totalWidth / 2f;
				
				for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
				{
					position += groupMargin;
					var group = groups[groupIndex];
					for (int nodeIndex = 0; nodeIndex < group.Length; nodeIndex++)
					{
						int node = group[nodeIndex];
						behaviour.GraphBehaviour.SetLocalPosition(
							node,
							new Vector3(
								position,
								0,
								separation.y * floorIndex));
						
						position += separation.x;
					}
					position += groupMargin;
				}
			}
		}

		private IEnumerable<int[]> CalculateGroups(GraphBehaviour graph, IReadOnlyList<int> floor)
		{
			List<int> groupContainer = new List<int>(floor.Count);
			HashSet<int> parentAndChildren = new HashSet<int>();
			for (int i = 0; i < floor.Count; i++)
			{
				int node = floor[i];
				if (parentAndChildren.Count == 0)
				{
					//Parents
					foreach (var edge in graph.FindAllEdgeIn(node))
					{
						parentAndChildren.Add(edge.From);
					}
					
					//Children
					foreach (var edge in graph.FindAllEdgeOut(node))
					{
						parentAndChildren.Add(edge.To);
					}
					groupContainer.Add(node);
				}
				else
				{
					bool breakGroup = false;
					foreach (var edge in graph.FindAllEdgeIn(node))
					{
						if (!parentAndChildren.Contains(edge.From))
						{
							breakGroup = true;
							break;
						}
					}
					
					if(!breakGroup)
					{
						foreach (var edge in graph.FindAllEdgeOut(node))
						{
							if (!parentAndChildren.Contains(edge.To))
							{
								breakGroup = true;
							}
						}
					}

					if (breakGroup)
					{
						parentAndChildren.Clear();
						i--;
						yield return groupContainer.ToArray();
						groupContainer.Clear();
					}
					else
					{
						groupContainer.Add(node);
					}
				}
			}

			if (groupContainer.Count > 0)
			{
				yield return groupContainer.ToArray();
			}
		}
	}
}