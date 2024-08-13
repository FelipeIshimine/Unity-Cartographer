using System.Collections.Generic;
using Cartographer.Core;
using Sample.Shop;

namespace Sample.Combat
{
	[System.Serializable]
	public class CombatNodeBlueprint : NodeBlueprint<CombatNodeData>
	{
		public List<EnemyData> Enemies = new List<EnemyData>();
		protected override CombatNodeData OnBuild() => new() { Enemies = Enemies };
	}
}