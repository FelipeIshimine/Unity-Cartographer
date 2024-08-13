using UnityEngine;

namespace Cartographer.Core
{
	public abstract class NodeType : ScriptableObject
	{
		public abstract NodeData CreateContent();
	}

	public abstract class NodeType<T, TB> : NodeType where T : NodeType<T, TB> where TB : NodeData<TB, T>, new()
	{
		public override NodeData CreateContent() => new TB();
	}
}