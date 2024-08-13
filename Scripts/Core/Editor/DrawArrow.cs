using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core.Editor
{

	public static class DrawArrow
	{
		public static void ForGizmo(Vector3 pos,
		                            Vector3 direction,
		                            float arrowHeadLength = 0.25f,
		                            float arrowHeadAngle = 20.0f)
		{
			Gizmos.DrawRay(pos, direction);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		public static void ForGizmo(Vector3 pos,
		                            Vector3 direction,
		                            Color color,
		                            float arrowHeadLength = 0.25f,
		                            float arrowHeadAngle = 20.0f)
		{
			Gizmos.color = color;
			Gizmos.DrawRay(pos, direction);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		public static void ForDebug(Vector3 pos,
		                            Vector3 direction,
		                            float arrowHeadLength = 0.25f,
		                            float arrowHeadAngle = 20.0f)
		{
			Debug.DrawRay(pos, direction);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Debug.DrawRay(pos + direction, right * arrowHeadLength);
			Debug.DrawRay(pos + direction, left * arrowHeadLength);
		}

		public static void ForDebug(Vector3 pos,
		                            Vector3 direction,
		                            Color color,
		                            float arrowHeadLength = 0.25f,
		                            float arrowHeadAngle = 20.0f)
		{
			Debug.DrawRay(pos, direction, color);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
			Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
		}


		public static IEnumerable<(Vector3 from, Vector3 to)> CreateSegments(Vector3 from, Vector3 to, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			yield return (from, to);

			var dir = to - from;
			if (dir == Vector3.zero)
			{
				yield return (to,to);
				yield return (to,to);
				yield break;
			}
			Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
				new Vector3(0, 0, 1);

			yield return (to,to + right * arrowHeadLength);
			yield return (to,to + left * arrowHeadLength);
		}
		
		public static IEnumerable<Vector3> CreatePoints(Vector3 from, Vector3 to, float arrowHeadLength = .25f, float arrowHeadAngle = 20f)
		{
			yield return from;
			yield return to;


			if(to != from)
			{
				var dir = to - from;

				Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
					new Vector3(0, 0, 1);
				Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
					new Vector3(0, 0, 1);

				yield return to;
				yield return to + right * arrowHeadLength;
				yield return to;
				yield return to + left * arrowHeadLength;
			}
			else
			{
				yield return to;
				yield return to;
				yield return to;
				yield return to;
			}
		}
		
	}
}