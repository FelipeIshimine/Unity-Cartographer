using System.Collections.Generic;
using Cartographer.Core;
using UnityEngine;

namespace Sample.Shop
{
	[System.Serializable]
	public class ShopNodeBlueprint : NodeBlueprint<ShopNodeData>
	{
		[SerializeField] public List<ShopProduct> products = new List<ShopProduct>();
		protected override ShopNodeData OnBuild()
		{
			return new ShopNodeData() { Products = products };
		}
	}
}