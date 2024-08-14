using UnityEngine;

namespace Cartographer.RoguelikeMap.Core.Processors
{
	[System.Serializable]
	public class ConnectAll : IRoguelikeMapProcessor
	{
		[SerializeField] private bool allowOverlappingEdges = false;
		[SerializeField] public bool limitEdgeByCloseness;
		[SerializeField] public bool useFewestConnections;

		public ConnectAll()
		{
		}

		public ConnectAll(bool allowOverlappingEdges, bool limitEdgeByCloseness)
		{
			this.limitEdgeByCloseness = limitEdgeByCloseness;
			this.allowOverlappingEdges = allowOverlappingEdges;
		}

		public void Process(ref RoguelikeMapData data)
		{
			new ConnectAllWithRandomChild(allowOverlappingEdges,limitEdgeByCloseness)
			{
				limitToOne = useFewestConnections
			}.Process(ref data);
			new ConnectAllWithRandomParent(allowOverlappingEdges,limitEdgeByCloseness)
			{
				limitToOne = useFewestConnections
			}.Process(ref data);
		}
	}
}