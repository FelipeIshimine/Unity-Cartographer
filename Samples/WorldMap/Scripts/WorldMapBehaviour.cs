using Cartographer.Core;
using UnityEngine;

namespace Cartographer.WorldMap
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