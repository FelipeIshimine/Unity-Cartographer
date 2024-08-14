using Cartographer.Core;
using Cartographer.Core.Editor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(editorWindowType = typeof(SceneView), defaultDisplay = false, displayName = "Graph Inspector")]
public class GraphToolbar : Overlay, ITransientOverlay
{
	public GraphBehaviourTools graphTool => GraphBehaviourTools.Instance;

	public bool visible =>
		graphTool != null &&
		ToolManager.IsActiveTool(graphTool) &&
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
		graphTool.OnIndexChange -= OnIndexChange;
		graphTool.OnIndexChange += OnIndexChange;
		OnIndexChange(graphTool.SelectedIndex);
	}

	private void Terminate()
	{
		initialized = false;
		if (graphTool)
		{
			graphTool.OnIndexChange -= OnIndexChange;
		}
	}

	public override void OnWillBeDestroyed()
	{
		mainContainer?.Clear();
		if (graphTool)
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

		var propField = new PropertyField
		{
			style =
			{
				flexShrink = 0,
				minWidth = 300
			}
		};
		propField.name = "selected-node-field";
		content.Add(propField);
			
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

		content.Q<Label>("graph-name").text = graphTool.name;
		
		
		if (graphTool && content != null)
		{
			if (index != -1)
			{
				var serializedObject = new SerializedObject(graphTool.graph);
				var prop = serializedObject.FindProperty(nameof(GraphBehaviour.data));
				var contentProperty = prop.FindPropertyRelative(nameof(GraphData.Content));
				
				var selectedNodeField= content.Q<PropertyField>("selected-node-field");
				
				var arrayElementProp = contentProperty.GetArrayElementAtIndex(index);
				selectedNodeField.BindProperty(arrayElementProp);
			}

			var nodesFoldout = content.Q<Foldout>("nodes-foldout");
			nodesFoldout.contentContainer.Clear();
			
			for (int i = 0; i < graphTool.graph.Count; i++)
			{
				var aux = i;
				var nodeData = graphTool.graph.GetContent(i);

				var button = new ToolbarButton(() =>
				{
					if (index != -1 && SceneView.lastActiveSceneView)
					{
						SceneView.lastActiveSceneView.Frame(new Bounds(graphTool.graph.GetWorldPosition(index), Vector3.one*10), false);
					}

					graphTool.Select(aux);
					RefreshContent(aux);
				})
				{
					text = $"{i}:{(nodeData != null ? nodeData.GetType().Name : "Empty")}",
				};

				nodesFoldout.Add(button);
			}
			content.Add(nodesFoldout);
		}
	}

	public override VisualElement CreatePanelContent()
	{
		CreateUI();
		if (graphTool )
		{
			OnIndexChange(graphTool.SelectedIndex);
		}
		return mainContainer;
	}
}