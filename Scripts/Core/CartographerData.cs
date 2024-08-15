using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core
{
	public class CartographerData : ScriptableObject
	{
		public static CartographerData Instance { get; private set; }
		
		[SerializeField] internal List<NodeType> nodeTypes = new List<NodeType>();

		public static T GetNodeType<T>() where T : NodeType => Instance.nodeTypes.Find(x => x is T) as T;
 		
		private void OnEnable()
		{
			Instance = this;
		}

		public bool AddNodeTypes(IEnumerable<NodeType> findOrCreateNodeTypes)
		{
			bool wasModified = false;
			foreach (NodeType nodeType in findOrCreateNodeTypes)
			{
				if (!nodeTypes.Contains(nodeType))
				{
					wasModified = true;
					nodeTypes.Add(nodeType);
				}
			}
			nodeTypes.Remove(null);
			if (wasModified)
			{
				nodeTypes.Sort((x,y) => String.Compare(x.name, y.name, StringComparison.Ordinal));
			}
			return wasModified;
		}
	}
}