using UnityEngine;

namespace Cartographer.Core
{[System.Serializable]
	public struct EdgeData
	{
		public int From;
		public int To;
		public EdgeData(int from, int to)
		{
			From = from;
			To = to;
		}

		public static implicit operator Vector2Int(EdgeData edgeData) => new(edgeData.From, edgeData.To);
		public static implicit operator EdgeData(Vector2Int vector) => new(vector.x, vector.y);
	}
}