using Cartographer.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Utilities.Editor
{
	[CustomPropertyDrawer(typeof(Optional<>))]
	public class OptionalPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var valueProperty = property.FindPropertyRelative("value");
			return EditorGUI.GetPropertyHeight(valueProperty);
		}

		public override void OnGUI(
			Rect position,
			SerializedProperty property,
			GUIContent label
		)
		{
			var valueProperty = property.FindPropertyRelative("value");
			var enabledProperty = property.FindPropertyRelative("enabled");

			EditorGUI.BeginProperty(position, label, property);
			position.width -= 24;
			EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
			EditorGUI.PropertyField(position, valueProperty, label, true);
			EditorGUI.EndDisabledGroup();

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			position.x += position.width + 24;
			position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
			position.x -= position.width;
			EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}


		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var valueProperty = property.FindPropertyRelative("value");
			var enabledProperty = property.FindPropertyRelative("enabled");

			VisualElement container = new VisualElement()
			{
				style = { flexDirection = FlexDirection.Row }
			};

			var valueProp = new PropertyField(valueProperty, property.displayName){style = { flexGrow = 1}};
			valueProp.SetEnabled(enabledProperty.boolValue);
			container.Add(valueProp);
	    
			var toggleField = new Toggle();
			container.Add(toggleField);
			toggleField.BindProperty(enabledProperty);

			void OnToggle(ChangeEvent<bool> evt) => valueProp.SetEnabled(evt.newValue);
			toggleField.RegisterValueChangedCallback(OnToggle);
			return container;
		}
	}
	
	
	[CustomPropertyDrawer(typeof(OptionalReference<>))]
	public class OptionalReferencePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var valueProperty = property.FindPropertyRelative("value");
			return EditorGUI.GetPropertyHeight(valueProperty);
		}

		public override void OnGUI(
			Rect position,
			SerializedProperty property,
			GUIContent label
		)
		{
			var valueProperty = property.FindPropertyRelative("value");
			var enabledProperty = property.FindPropertyRelative("enabled");

			EditorGUI.BeginProperty(position, label, property);
			position.width -= 24;
			EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
			EditorGUI.PropertyField(position, valueProperty, label, true);
			EditorGUI.EndDisabledGroup();

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			position.x += position.width + 24;
			position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
			position.x -= position.width;
			EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);
			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}


		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var valueProperty = property.FindPropertyRelative("value");
			var enabledProperty = property.FindPropertyRelative("isEnabled");

			VisualElement container = new VisualElement()
			{
				style = { flexDirection = FlexDirection.Row }
			};

			var valueProp = new PropertyField(valueProperty, property.displayName){style = { flexGrow = 1}};
			valueProp.SetEnabled(enabledProperty.boolValue);
			container.Add(valueProp);
	    
			var toggleField = new Toggle(){style = { alignSelf = Align.FlexStart}};
			container.Add(toggleField);
			toggleField.BindProperty(enabledProperty);

			void OnToggle(ChangeEvent<bool> evt) => valueProp.SetEnabled(evt.newValue);
			toggleField.RegisterValueChangedCallback(OnToggle);
			return container;
		}
	}
}
