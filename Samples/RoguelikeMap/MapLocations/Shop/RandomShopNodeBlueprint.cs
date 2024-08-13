using System.Collections.Generic;
using Cartographer.Core;
using UnityEngine;

namespace Sample.Shop
{
	[System.Serializable]
	public class RandomShopNodeBlueprint : NodeBlueprint<ShopNodeData>, ISerializationCallbackReceiver
	{
		public Vector2Int minMaxProductsRange = new Vector2Int(1,10);
		[SerializeField] public List<ShopProduct> pool = new List<ShopProduct>();
		protected override ShopNodeData OnBuild()
		{
			List<ShopProduct> selectedProducts = new List<ShopProduct>();

			int count = Random.Range(minMaxProductsRange.x, minMaxProductsRange.y);

			for (int i = 0; i < count; i++)
			{
				selectedProducts.Add(pool[Random.Range(minMaxProductsRange.x,minMaxProductsRange.y)]);
			}
			
			return new ShopNodeData() { Products = selectedProducts };
		}

		public void OnBeforeSerialize()
		{
			minMaxProductsRange.x = Mathf.Max(minMaxProductsRange.x, 0);
			minMaxProductsRange.y = Mathf.Max(minMaxProductsRange.x, minMaxProductsRange.y);
		}

		public void OnAfterDeserialize()
		{
		}
	}
}