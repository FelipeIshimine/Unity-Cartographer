using UnityEngine;

namespace Cartographer.Core
{
	public abstract class NodeType : ScriptableObject, ISerializationCallbackReceiver
	{
		[field:SerializeField] public string NodeName { get; set; }
		public abstract NodeData CreateContent();

		public void OnBeforeSerialize()
		{
			if (string.IsNullOrEmpty(NodeName))
			{
				NodeName = GetType().Name;
			}
		}

		public void OnAfterDeserialize()
		{
		}
	}

	public abstract class NodeType<T, TB> : NodeType where T : NodeType<T, TB> where TB : NodeData<TB, T>, new()
	{
		public override NodeData CreateContent() => new TB();
	}
}