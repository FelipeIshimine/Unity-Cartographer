using UnityEditor;
using UnityEditor.UIElements;
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
	}
}