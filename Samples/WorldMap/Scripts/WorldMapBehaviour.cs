using Cartographer.Core;
using UnityEngine;

namespace Samples.WorldMap
{
	[RequireComponent(typeof(GraphBehaviour))]
	public class WorldMapBehaviour : MonoBehaviour
	{
		[SerializeField] private GraphBehaviour graphBehaviour;

		private void Reset()
		{
			graphBehaviour = GetComponent<GraphBehaviour>();
		}
	}
}