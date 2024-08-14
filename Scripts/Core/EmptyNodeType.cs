using UnityEngine;

namespace Cartographer.Core
{
	[CreateAssetMenu(menuName = "Cartographer/Create EmptyNodeType", fileName = "EmptyNodeType", order = 0)]
	public class EmptyNodeType : NodeType<EmptyNodeType, EmptyNodeData>
	{
	}

	[System.Serializable]
	public class EmptyNodeData : NodeData<EmptyNodeData, EmptyNodeType>
	{
	}
}