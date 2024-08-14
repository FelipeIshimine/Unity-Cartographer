using System.Collections.Generic;
using Cartographer.Core;

namespace Sample.Shop
{
	public class ShopNodeData : NodeData<ShopNodeData,ShopNodeType>
	{
		public List<ShopProduct> Products = new List<ShopProduct>();
	}
}