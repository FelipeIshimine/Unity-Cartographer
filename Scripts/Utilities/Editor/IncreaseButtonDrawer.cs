using Cartographer.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Core.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(IncreaseButton))]
	public class IncreaseButtonDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var intWithButtonsAttribute = (IncreaseButton)attribute;
			// Create a container for the property elements
			var container = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row
				}
			};
			// Create a field for the integer value

			if (!intWithButtonsAttribute.HideLabel)
			{
				container.Add(new Label(property.displayName)
				{
					style = { flexGrow = 1}
				});
			}
			
			var intField = new IntegerField(string.Empty);
			intField.BindProperty(property);

			// Create buttons
			var addButton = new Button(() => 
			{
				property.intValue++;
				property.serializedObject.ApplyModifiedProperties();
			}) { text = "+" };

			var subtractButton = new Button(() => 
			{
				property.intValue--;
				property.serializedObject.ApplyModifiedProperties();
			}) { text = "-" };

			// Add elements to the container
			container.Add(subtractButton);
			container.Add(intField);
			container.Add(addButton);

			return container;
		}
	}
}