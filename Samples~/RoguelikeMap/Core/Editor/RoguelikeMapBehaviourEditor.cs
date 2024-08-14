using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cartographer.RoguelikeMap.Core.Editor
{
	[CustomEditor(typeof(RoguelikeMapBehaviour))]
	public class RoguelikeMapBehaviourEditor : UnityEditor.Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			VisualElement container = new VisualElement();
			var iterator = serializedObject.GetIterator();
			if (iterator.NextVisible(true))
			{
				do
				{
					container.Add(new PropertyField(iterator));
				} while (iterator.NextVisible(false));
			}

			RoguelikeMapBehaviour roguelikeMapBehaviour = (RoguelikeMapBehaviour)serializedObject.targetObject;

			Button button = new Button(()=> roguelikeMapBehaviour.Load())
			{
				text = "Load"
			};
			
			container.Add(button);
			
			return container;
			
			
		}
	}
}