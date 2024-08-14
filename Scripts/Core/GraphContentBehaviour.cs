using UnityEngine;

namespace Cartographer.Core
{
	[RequireComponent(typeof(GraphBehaviour))]
	public abstract class GraphContentBehaviour<T> : MonoBehaviour
	{
		private GraphBehaviour graphBehaviour;
		public GraphContentData<T> ContentData;
		private void Awake()
		{
			graphBehaviour = GetComponent<GraphBehaviour>();
		}
	}
}