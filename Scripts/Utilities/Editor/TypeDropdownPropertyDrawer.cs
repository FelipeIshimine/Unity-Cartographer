using System;
using System.Collections.Generic;
using System.Reflection;
using Cartographer.Utilities.Attributes;
using Cartographer.Utilities.Editor;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
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
			
			if(property.hasVisibleChildren && property.hasChildren)
			{
				var propertyField = new PropertyField(property, property.displayName)
				{
					style = { flexGrow = 1 },
					name = "Field"
				};
				
				propertyField.TrackPropertyValue(property, x=> propertyField.label =property.displayName);
				//propertyField.BindProperty(property);
				propertyContainer.Add(propertyField);
			}
			else
			{
				var label = new Label(property.displayName);
				label.TrackPropertyValue(property, x=> label.text = property.displayName);
				propertyContainer.Add(label);
			}

			EditorToolbarDropdown typeBtn = null;
			
			if(dropdownAttribute.UseAbsolutePosition)
			{
				typeBtn = new EditorToolbarDropdown(GetButtonLabel(property), () => TypeDropdownClicked(typeBtn, property))
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

			/*
			Debug.Log(typeFullName);
			Debug.Log(typeNamespace);*/

			string resultName;
			if (type.GetCustomAttribute(typeof(TypeDropdownNameAttribute), false) is TypeDropdownNameAttribute nameAttribute)
			{
				resultName = nameAttribute.Name;
			}
			else if (!string.IsNullOrEmpty(typeFullName) && !string.IsNullOrEmpty(typeNamespace))
			{
				resultName = typeFullName.Replace(typeNamespace, string.Empty).Replace(".", string.Empty);
			}
			else if(!string.IsNullOrEmpty(typeFullName))
			{
				resultName = type.FullName;
			}
			else
			{
				resultName = type.Name;
			}
			return resultName;

		}

		private void TypeDropdownClicked(EditorToolbarDropdown typeBtn, SerializedProperty property)
		{
			var targetType = GetTargetType();
			var baseTypeName = GetDisplayName(targetType);

			var types = new List<Type>();

			var unityObjectType = typeof(UnityEngine.Object);
			
			foreach (var type in TypeCache.GetTypesDerivedFrom(targetType))
			{
				if (!type.IsAbstract && !unityObjectType.IsAssignableFrom(type))
				{
					types.Add(type);
				}
			}

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
					labels[i] = GetDisplayName(type);
				}
				else
				{
					labels[i] = path;
				}
			}

			QuickAdvancedDropdown dropdown =
				new QuickAdvancedDropdown(
					$"{baseTypeName} Types",
					labels,
					index => OnTypeSelected(index, typeBtn, property, typesArray));

			var rect = typeBtn.worldBound;
			rect.width = Mathf.Max(200, rect.width);
			dropdown.Show(rect);
		}

		private Type GetTargetType()
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

			return targetType;
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