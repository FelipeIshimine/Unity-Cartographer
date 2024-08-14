using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cartographer.Core.Editor
{
	[Overlay(editorWindowType = typeof(SceneView), defaultDisplay = false, displayName = "Graph Inspector")]
	public class GraphToolbar : Overlay, ITransientOverlay
	{
		public static GraphBehaviourTools GraphTool => GraphBehaviourTools.Instance;

		public bool visible =>
			GraphTool != null &&
			ToolManager.IsActiveTool(GraphTool) &&
			Initialized;

		private VisualElement mainContainer;
		private VisualElement content;

		private bool initialized;
		public bool Initialized
		{
			get
			{
				if (!initialized)
				{
					Initialize();
				}

				return initialized;
			}
		}

		private void Initialize()
		{
			initialized = true;
			GraphTool.OnIndexChange -= OnIndexChange;
			GraphTool.OnIndexChange += OnIndexChange;
			OnIndexChange(GraphTool.SelectedIndex);
		}

		private void Terminate()
		{
			initialized = false;
			if (GraphTool)
			{
				GraphTool.OnIndexChange -= OnIndexChange;
			}
		}

		public override void OnWillBeDestroyed()
		{
			mainContainer?.Clear();
			if (GraphTool)
			{
				Terminate();
			}

			base.OnWillBeDestroyed();
		}

		private void OnIndexChange(int index) => RefreshContent(index);

		private void CreateUI()
		{
			mainContainer = new VisualElement();
			content = new ScrollView(ScrollViewMode.Vertical);
			content.style.maxHeight = 300;
			content.style.minWidth = 100;
			mainContainer.Add(content);
		
			content.Add(new Label("None"){name = "graph-name"});

			VisualElement selectionContainer = new VisualElement()
			{
				name = "selection-container"
			};
			
			var propField = new PropertyField
			{
				style =
				{
					flexShrink = 0,
					minWidth = 300
				}
			};
			propField.name = "selected-node-field";
			
			selectionContainer.Add(propField);
			
			content.Add(selectionContainer);
			
			Foldout listFoldout = new Foldout
			{
				name = "nodes-foldout",
				text = "Nodes"
			};

			listFoldout.style.maxHeight = 100;

			content.Add(listFoldout);
		}
	
	
		private void RefreshContent(int index)
		{
			if (content == null)
			{
				Debug.Log("Content is null");
				return;
			}

			content.Q<Label>("graph-name").text = GraphTool.name;
		
		
			if (GraphTool && content != null)
			{
				if (index != -1)
				{
					var serializedObject = new SerializedObject(GraphTool.graph);
					var prop = serializedObject.FindProperty(nameof(GraphBehaviour.data));
					var contentProperty = prop.FindPropertyRelative(nameof(GraphData.Content));
				
					var selectedNodeField= content.Q<PropertyField>("selected-node-field");
				
					var arrayElementProp = contentProperty.GetArrayElementAtIndex(index);
					selectedNodeField.BindProperty(arrayElementProp);
				}

				var nodesFoldout = content.Q<Foldout>("nodes-foldout");
				nodesFoldout.contentContainer.Clear();
			
				for (int i = 0; i < GraphTool.graph.Count; i++)
				{
					var aux = i;
					var nodeData = GraphTool.graph.GetContent(i);

					var button = new ToolbarButton(() =>
					{
						GraphTool.Select(aux);
						RefreshContent(aux);
						nodesFoldout.value = true;
						if (aux != -1 && SceneView.lastActiveSceneView)
						{
							SceneView.lastActiveSceneView.Frame(new Bounds(GraphTool.graph.GetWorldPosition(aux), Vector3.one*10), false);
						}
					})
					{
						text = $"{i}:{(nodeData != null ? nodeData.GetType().Name : "Empty")}",
					};

					nodesFoldout.value = index == -1;
					nodesFoldout.Add(button);
				}
				content.Add(nodesFoldout);
			}
		}

		public override VisualElement CreatePanelContent()
		{
			CreateUI();
			if (GraphTool )
			{
				OnIndexChange(GraphTool.SelectedIndex);
			}
			return mainContainer;
		}
	}
}