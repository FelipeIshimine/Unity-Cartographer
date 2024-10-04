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
				style = { flexDirection = FlexDirection.Column }
			};
			CreatePropertyField(container, property);
			return container;
		}


		private void CreatePropertyField(VisualElement propertyContainer, SerializedProperty property)
		{
			Foldout foldout = new Foldout
			{
				text = property.displayName,
				value = false
			};

			Toolbar toolbar = new Toolbar()
			{
				style = { flexGrow = 0, flexShrink = 0, alignSelf = Align.FlexEnd}
			};
			ToolbarButton typeBtn = null;
			
			typeBtn = new ToolbarButton(() => TypeDropdownClicked(typeBtn, property))
			{
				text = GetButtonLabel(property),
				style =
				{
					position = Position.Relative,
					flexGrow = 0,
					flexShrink = 0,
					paddingLeft = 2,
					paddingRight = 2,
					unityTextAlign = TextAnchor.MiddleCenter
				}
			};

			ToolbarButton clearButton = new ToolbarButton(()=>
			{
				property.managedReferenceValue = null;
				property.serializedObject.ApplyModifiedProperties();
			})
			{
				text = "X",
				style =
				{
				width = 16,
				position = Position.Relative,
				flexGrow = 0,
				marginLeft = 0,
				marginRight = 0,
				paddingLeft = 0,
				paddingRight = 0,
				unityTextAlign = TextAnchor.MiddleCenter
			}
			};
			
			toolbar.Add(typeBtn);
			toolbar.Add(clearButton);
			//propertyContainer.Add(toolbar);
			propertyContainer.Add(foldout);

			var toggle=foldout.Q<Toggle>();
			toggle.Add(toolbar);
			
			void RefreshContent(SerializedProperty x)
			{
				toggle.value = foldout.value = x.isExpanded;
				foldout.contentContainer.Clear();
				foreach (SerializedProperty childrenProperty in x.FindChildrenProperties())
				{
					foldout.Add(new PropertyField(childrenProperty));
				}

				bool show = foldout.contentContainer.childCount > 0;
				toggle.Q<VisualElement>("unity-checkmark").style.visibility = show? Visibility.Visible: Visibility.Hidden;
			}

			foldout.TrackPropertyValue(property, RefreshContent);
            
			RefreshContent(property.Copy());
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

		private void TypeDropdownClicked(ToolbarButton typeBtn, SerializedProperty property)
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

		void OnTypeSelected(int index, Button typeBtn, SerializedProperty property, Type[] typesArray)
		{
			property.managedReferenceValue = Activator.CreateInstance(typesArray[index]);
			property.serializedObject.ApplyModifiedProperties();
			typeBtn.text = GetButtonLabel(property);
			typeBtn.parent.Bind(property.serializedObject);
		}
	}
	
	public static class SerializedPropertyExtensions
	{
		public static IEnumerable<SerializedProperty> FindChildrenProperties(this SerializedProperty parent, int depth = 1) {
			var depthOfParent = parent.depth;
			foreach (object current in parent)
			{
				if (current is not SerializedProperty childProperty)
				{
					continue;
				}
				if (childProperty.depth > depthOfParent + depth) continue;
				yield return childProperty.Copy();
			}
		}
	}
}