using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core.Editor
{
	public class CartographerData : ScriptableObject
	{
		[SerializeField] internal List<NodeType> nodeTypes = new List<NodeType>();
		
		public void SetNodeTypes(IEnumerable<NodeType> findOrCreateNodeTypes)
		{
			foreach (NodeType nodeType in findOrCreateNodeTypes)
			{
				if (!nodeTypes.Contains(nodeType))
				{
					nodeTypes.Add(nodeType);
				}
			}
			nodeTypes.Remove(null);
			nodeTypes.Sort((x,y) => String.Compare(x.name, y.name, StringComparison.Ordinal));
		}
	}
}