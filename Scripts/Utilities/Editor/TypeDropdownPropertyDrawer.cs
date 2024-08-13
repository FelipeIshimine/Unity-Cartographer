using System;
using System.Collections.Generic;
using Cartographer.Utilities.Attributes;
using Cartographer.Utilities.Editor;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Core.Attributes.Editor
{
	[CustomPropertyDrawer(typeof(TypeDropdownAttribute), true)]
	public class TypeDropdownDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (SerializationUtility.HasManagedReferencesWithMissingTypes(property.serializedObject.targetObject))
			{
				SerializationUtility.ClearAllManagedReferencesWithMissingTypes(property.serializedObject.targetObject);
			}

			var container = new VisualElement()
			{
				style = { flexDirection = FlexDirection.Row }
			};
			CreatePropertyField(container, property);
			return container;
		}


		private void CreatePropertyField(VisualElement propertyContainer, SerializedProperty property)
		{
			TypeDropdownAttribute dropdownAttribute = (TypeDropdownAttribute)attribute;
			
			if(property.hasVisibleChildren)
			{
				var propertyField = new PropertyField(property, property.displayName)
				{
					style = { flexGrow = 1 },
					name = "Field"
				};
				propertyContainer.Add(propertyField);
			}
			else
			{
				propertyContainer.Add(new Label(property.displayName));
			}

			EditorToolbarDropdown typeBtn = null;
			
			if(dropdownAttribute.UseAbsolutePosition)
			{
				typeBtn = new EditorToolbarDropdown(GetButtonLabel(property),
					() => TypeDropdownClicked(typeBtn, property))
				{
					style =
					{
						height = 18,
						position = Position.Absolute,
						left = new StyleLength(StyleKeyword.Auto),
						right = 0,
					}
				};
			}
			else
			{
				typeBtn = new EditorToolbarDropdown(GetButtonLabel(property),
					() => TypeDropdownClicked(typeBtn, property))
				{
					style =
					{
						height = 18,
						position = Position.Relative,
						flexGrow = 1
					}
				};
			}
			propertyContainer.Add(typeBtn);
		}

		private string GetButtonLabel(SerializedProperty p)
		{
			var managedReferenceValue = p.managedReferenceValue;
			if (managedReferenceValue != null)
			{
				var type = managedReferenceValue.GetType();
				var value = GetDisplayName(type);
				var baseType = type.BaseType;
				if (baseType != null)
				{
					value = value.Replace(baseType.Name, string.Empty);
				}

				return value;
			}

			return "-Select Type-";
		}

		private string GetDisplayName(Type type)
		{
			if (type == null)
			{
				return "NULL";
			}

			var typeFullName = type.FullName;
			var typeNamespace = type.Namespace;

			if (string.IsNullOrEmpty(typeFullName) || string.IsNullOrEmpty(typeNamespace))
			{
				return type.Name;
			}

			return typeFullName.Replace(typeNamespace, string.Empty).Replace(".", string.Empty);
		}

		private void TypeDropdownClicked(EditorToolbarDropdown typeBtn, SerializedProperty property)
		{
			Type targetType;
			if (fieldInfo.FieldType.IsConstructedGenericType)
			{
				targetType = fieldInfo.FieldType.GetGenericArguments()[0];
			}
			else
			{
				targetType = fieldInfo.FieldType;
			}

			var types = new List<Type>();

			var unityObjectType = typeof(UnityEngine.Object);
			
			foreach (var type in TypeCache.GetTypesDerivedFrom(targetType))
			{
				if (!type.IsAbstract && !unityObjectType.IsAssignableFrom(type))
				{
					types.Add(type);
				}
			}

			var baseTypeName = GetDisplayName(targetType);
			var typesArray = types.ToArray();

			string[] labels = new string [typesArray.Length];

			for (int i = 0; i < labels.Length; i++)
			{
				var type = typesArray[i];

				string path = null;

				foreach (object customAttribute in type.GetCustomAttributes(typeof(DropdownPathAttribute), false))
				{
					if (customAttribute is DropdownPathAttribute dropdownPathAttribute)
					{
						path = dropdownPathAttribute.Path;
					}
				}

				if (string.IsNullOrEmpty(path))
				{
					labels[i] = GetDisplayName(type).Replace(baseTypeName, string.Empty);
				}
				else
				{
					labels[i] = path;
				}
			}

			QuickAdvancedDropdown dropdown =
				new QuickAdvancedDropdown(
					"Types",
					labels,
					index => OnTypeSelected(index, typeBtn, property, typesArray));

			dropdown.Show(typeBtn.worldBound);
		}

		void OnTypeSelected(int index, EditorToolbarDropdown typeBtn, SerializedProperty property, Type[] typesArray)
		{
			property.managedReferenceValue = Activator.CreateInstance(typesArray[index]);
			property.serializedObject.ApplyModifiedProperties();
			typeBtn.text = GetButtonLabel(property);
			typeBtn.parent.Bind(property.serializedObject);
		}

		static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
				{
					return true;
				}

				toCheck = toCheck.BaseType;
			}

			return false;
		}

	}
}