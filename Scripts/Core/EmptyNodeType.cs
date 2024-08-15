using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.Core
{
	[CreateAssetMenu(menuName = "Cartographer/Create EmptyNodeType", fileName = "EmptyNodeType", order = 0)]
	public class EmptyNodeType : NodeType<EmptyNodeType, EmptyNodeType.Data>
	{
		[System.Serializable, TypeDropdownName("Empty")]
		public class Data : NodeData<Data, EmptyNodeType>
		{
		}
	}

}