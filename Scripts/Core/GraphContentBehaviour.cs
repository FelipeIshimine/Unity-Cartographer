using UnityEngine;

namespace Cartographer.Core
{
	[RequireComponent(typeof(MapBehaviour))]
	public abstract class GraphContentBehaviour<T> : MonoBehaviour
	{
		private MapBehaviour mapBehaviour;
		public GraphContentData<T> ContentData;
		private void Awake()
		{
			mapBehaviour = GetComponent<MapBehaviour>();
		}
	}
}