using System.Collections.Generic;
using Cartographer.Core;
using UnityEngine;

namespace Sample.Combat
{
	[System.Serializable]
	public class RandomCombatNodeBlueprint : NodeBlueprint<CombatNodeData>, ISerializationCallbackReceiver
	{
		public Vector2Int minMaxProductsRange = new Vector2Int(1,10);
		[SerializeField] public List<EnemyData> pool = new List<EnemyData>();
		protected override CombatNodeData OnBuild()
		{
			List<EnemyData> selectedProducts = new List<EnemyData>();

			int count = Random.Range(minMaxProductsRange.x, minMaxProductsRange.y);

			for (int i = 0; i < count; i++)
			{
				selectedProducts.Add(pool[Random.Range(minMaxProductsRange.x,minMaxProductsRange.y)]);
			}
			
			return new CombatNodeData() { Enemies = selectedProducts };
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