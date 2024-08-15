using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cartographer.Core.Editor
{
	[CustomEditor(typeof(GraphBehaviour))]
	public class GraphBehaviourEditor : UnityEditor.Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			var graphBehaviour = (GraphBehaviour)target;
			
			VisualElement container = new VisualElement();
			var iterator = serializedObject.GetIterator();

			if (iterator.NextVisible(true))
			{
				do
				{
					container.Add(new PropertyField(iterator));
				} while (iterator.NextVisible(false));
			}
			
			VisualElement buttonsContainer = new VisualElement();

			var button = new Button(graphBehaviour.Clear)
			{
				text = "Clear"
			};
			buttonsContainer.Add(button);
			container.Add(buttonsContainer);

			return container;
		}
		
		static List<Vector3> gizmoPoints = new List<Vector3>();

		[DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable)]
		static void DrawGizmos(GraphBehaviour scr, GizmoType gizmoType)
		{

			if ((gizmoType & GizmoType.Selected) != 0 || !scr.showGizmos)
			{
				return;
			}
            
			//Gizmos.DrawIcon(scr.transform.position, "GraphBehaviour-Icon.png");
			gizmoPoints.Clear();
			foreach (var edge in scr.data.Edges)
			{
				gizmoPoints.Add(scr.GetWorldPosition(edge.From));
				gizmoPoints.Add(scr.GetWorldPosition(edge.To));
			}

			for (int i = 0; i < scr.Count; i++)
			{
				Gizmos.DrawWireSphere(scr.GetWorldPosition(i),.25f);
			}
            Gizmos.DrawLineList(gizmoPoints.ToArray());
		}
	}
}