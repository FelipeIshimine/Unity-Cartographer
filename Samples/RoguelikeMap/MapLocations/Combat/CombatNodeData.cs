using System.Collections.Generic;
using Cartographer.Core;

namespace Sample.Combat
{
	[System.Serializable]
	public class CombatNodeData : NodeData<CombatNodeData,CombatNodeType>
	{
		public List<EnemyData> Enemies = new List<EnemyData>();
	}
}