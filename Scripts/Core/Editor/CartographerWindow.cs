using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cartographer.Core.Editor
{
	public class CartographerWindow : EditorWindow
	{
		private UnityEditor.Editor editor;
		private CartographerData data;
		private static bool shouldRefresh;

		[MenuItem("Window/Cartographer")]
		public static void Open()
		{
			GetWindow<CartographerWindow>().Show();
		}

		private void OnEnable()
		{
			rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/ishimine.cartographer/UI/CartographerEditorStyle.uss"));
		}

		private void CreateGUI()
		{
			shouldRefresh = false;
			rootVisualElement.Clear();
			data = CartographerUtilities.LoadOrCreateData();
			
			
            
			//UnityEditor.Editor.CreateCachedEditor(data, null, ref editor);
			//var visual = editor.CreateInspectorGUI();
			//visual.Bind(new SerializedObject(data));

			ScrollView scrollView = new ScrollView();
			VisualElement content = new VisualElement();
			
			scrollView.Add(content);

			foreach (NodeType nodeType in data.nodeTypes)
			{
				VisualElement nodeTypeContainer = new VisualElement();
				nodeTypeContainer.AddToClassList("node-type-container");
				
				var foldout = new Foldout();
				foldout.AddToClassList("node-type-container-foldout");
				
				foldout.contentContainer.Clear();

				foldout.text = nodeType.NodeName;
			
				UnityEditor.Editor.CreateCachedEditor(nodeType, null, ref editor);
				var inspectorElement = editor.CreateInspectorGUI();
				inspectorElement.Bind(new SerializedObject(nodeType));
				foldout.contentContainer.Add(inspectorElement);

				nodeTypeContainer.Add(foldout);
				
				content.Add(nodeTypeContainer);
			}
			
			rootVisualElement.Add(scrollView);
			
		}

		private void OnInspectorUpdate()
		{
			if (shouldRefresh)
			{
				CreateGUI();
			}
		}

		public static void RequestRefresh() => shouldRefresh = true;
	}
}