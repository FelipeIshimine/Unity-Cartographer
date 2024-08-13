using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cartographer.Core.Editor
{
	[EditorTool("Map Tool", typeof(MapBehaviour))]
	public class MapBehaviourTools : EditorTool, IDrawSelectedHandles
	{
		public event Action<MapBehaviour> OnMapChange; 
		public event Action<int> OnIndexChange; 
		private const float ARROW_OFFSET = .5f;
		private const float SIDE_OFFSET = .1f;

		private int selectedIndex = -1;
		private Vector3[] gizmoLines = Array.Empty<Vector3>();

		readonly Dictionary<(int from,int to),int> arrowsCountDict = new();

		public static MapBehaviourTools Instance { get; private set; }

		private readonly HashSet<EdgeData> connectionsOut = new HashSet<EdgeData>();
		private readonly HashSet<EdgeData> connectionsIn = new HashSet<EdgeData>();
		
		public int SelectedIndex
		{
			get => selectedIndex;
			set
			{
				if(selectedIndex != value)
				{
					selectedIndex = value;
					connectionsIn.Clear();
					connectionsOut.Clear();

					if (selectedIndex != -1)
					{
						FindAllConnectionsOut(selectedIndex);
						FindAllConnectionsIn(selectedIndex);
					}
					OnIndexChange?.Invoke(value);
				}
			}
		}

		private void FindAllConnectionsIn(int i)
		{
			foreach (var edge in Map.FindAllEdgeIn(i))
			{
				if (connectionsIn.Add(edge))
				{
					FindAllConnectionsIn(edge.From);
				}
			}
			
		}

		private void FindAllConnectionsOut(int i)
		{
			foreach (var edge in Map.FindAllEdgeOut(i))
			{
				if (connectionsOut.Add(edge))
				{
					FindAllConnectionsOut(edge.To);
				}
			}
		}

		public MapBehaviour map;
		public MapBehaviour Map
		{
			get => map;
			set
			{
				if(map != value)
				{
					SelectedIndex = -1;
					if (map != null)
					{
						map.OnLoad -= SetMap;
					}
					map = value;
					if (map != null)
					{
						map.OnLoad += SetMap;
					}
					OnMapChange?.Invoke(map);
				}
			}
		}

		private void SetMap(MapBehaviour obj)
		{
			Map = null;
			Map = obj;
		}


		static MapBehaviourTools()
		{
			Selection.selectionChanged += OnSelectionChange;
		}

		private static void OnSelectionChange()
		{
			if (Selection.GetFiltered<MapBehaviour>(SelectionMode.TopLevel).Length > 0)
			{
				ToolManager.SetActiveTool<MapBehaviourTools>();
			}
		}

		[Shortcut("Activate Map Tool", typeof(SceneView), KeyCode.M)]
		static void PlatformToolShortcut()
		{
			if (Selection.GetFiltered<MapBehaviour>(SelectionMode.TopLevel).Length > 0)
				ToolManager.SetActiveTool<MapBehaviourTools>();
			else
				Debug.Log("No map selected!");
		}


		private void OnEnable()
		{
			gizmoLines = Array.Empty<Vector3>();
			SelectedIndex = -1;
			Instance = this;
		}

		private void OnDisable()
		{
			gizmoLines = null;
			Instance = null;
		}

		public override void OnToolGUI(EditorWindow window)
		{
			if (window is not SceneView sceneView)
			{
				return;
			}

			
			if (target is MapBehaviour mapBehaviour)
			{
				Map = mapBehaviour;
				var currentEvent = Event.current;
				
				
				var mousePosition = GetMouseIntersectionPosition(mapBehaviour, currentEvent);

				if (!currentEvent.shift && currentEvent.alt)
				{
					return;	
				}

				/*if (currentEvent.shift)
				{
					currentEvent.Use();
				}*/
				
				if (TryHandleNodeDeletion(mapBehaviour, sceneView, mousePosition))
				{
					return;
				}
				
				
				if (SelectedIndex == -1)
				{
				
					
					if (TryHandleNodeCreation(mapBehaviour,mousePosition))
					{
						return;
					}

					
					
					if (TryHandleNodeSelection(mapBehaviour,sceneView,mousePosition))
					{
						return;
					}
				}
				else
				{
					if (TryHandleEdgeDeletion(mapBehaviour, sceneView, mousePosition))
					{
						return;
					}
					
					if (TryHandleSelectionDisplacement(mapBehaviour, mousePosition))
					{
						return;
					}
						
					if (TryHandleMerge(mapBehaviour, mousePosition))
					{
						return;
					}
					
					if (TryHandleEdgeCreation(mapBehaviour,mousePosition))
					{
						return;
					}

					if (TryHandleNodeCreationWithEdge(mapBehaviour, mousePosition))
					{
						return;
					}
					
					if (TryHandleNodeSelection(mapBehaviour,sceneView, mousePosition))
					{
						return;
					}
					
					if(TryHandleDeselect(mapBehaviour))
					{
						return;
					}

				}
				
			

				if (currentEvent.isMouse && !currentEvent.alt)
				{
					if (currentEvent.button is 0 or 1)
					{
						currentEvent.Use();
					}
				}
				
				
			}
			
			//HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

		}

		private bool TryHandleMerge(MapBehaviour mapBehaviour, Vector3 mousePosition)
		{
			var currentEvent = Event.current;
			if (SelectedIndex != -1 &&
			    currentEvent.shift && 
			    currentEvent.alt)
			{
				if(TryFindClosestNode(mapBehaviour, mousePosition, out var targetIndex))
				{
					if (currentEvent.type is EventType.MouseDown && currentEvent.button == 0)
					{
						Undo.RecordObject(mapBehaviour, $"Merge Nodes {SelectedIndex} => {targetIndex}");
						mapBehaviour.data.Merge(SelectedIndex, targetIndex);
					}
					else
					{
						Handles.color = Color.yellow;

						var start = mapBehaviour.GetWorldPosition(SelectedIndex);
						var end = mapBehaviour.GetWorldPosition(targetIndex);

						var dir = (end - start).normalized;

						if (dir != Vector3.zero)
						{

							start += dir * ARROW_OFFSET;
							end -= dir * ARROW_OFFSET;
							
							Handles.DrawLines(
								DrawArrow.CreatePoints(
									start,
									end
								).ToArray());
							Handles.color = Color.white;
							
						}
					}

					return true;
				}
				else
				{
					Handles.color = Color.yellow;
					Handles.DrawLines(DrawArrow.CreatePoints(mapBehaviour.GetWorldPosition(SelectedIndex),mousePosition).ToArray());
					Handles.color = Color.white;
				}
			}
			return false;
		}

		private bool TryHandleEdgeDeletion(MapBehaviour mapBehaviour, SceneView sceneView, Vector3 mousePosition)
		{
			var current = Event.current;
			if (current.control && 
			    current.shift &&
			    TryFindClosestNode(mapBehaviour, mousePosition, out var index))
			{
				var pos = mapBehaviour.GetWorldPosition(index);

				var start = mapBehaviour.GetWorldPosition(SelectedIndex);
				var end = pos;

				var dir = (end - start).normalized;

				start += dir * ARROW_OFFSET;
				end -= dir * ARROW_OFFSET;

				Handles.color = Color.red;
				Handles.DrawLines(DrawArrow.CreatePoints(start,end).ToArray());
				Handles.color = Color.white;
				
				if (current.type is EventType.MouseDown)
				{
					mapBehaviour.RemoveEdge(SelectedIndex, index);
				}
				return true;
			}

			return false;
		}

		private bool TryHandleNodeCreationWithEdge(MapBehaviour mapBehaviour, Vector3 mousePosition)
		{
			var current = Event.current;

			if (current.control)
			{
				if (current.isMouse &&
				    current.type is EventType.MouseDown &&
				    current.button == 0)
				{
					Undo.RecordObject(mapBehaviour, "Node Created");
					int index = mapBehaviour.AddNodeAtWorldPosition(mousePosition);
					mapBehaviour.Connect(SelectedIndex, index);
					SelectedIndex = index;
					return true;
				}

				var oldMatrix = Handles.matrix;

				Handles.matrix = mapBehaviour.transform.localToWorldMatrix;
				Handles.color = Color.yellow;
				Handles.DrawWireDisc(mapBehaviour.transform.InverseTransformPoint(mousePosition),mapBehaviour.transform.up, ARROW_OFFSET);
				Handles.color = Color.white;
				Handles.matrix = oldMatrix;
				
				
				
				Handles.DrawLines(DrawArrow.CreatePoints(mapBehaviour.GetWorldPosition(SelectedIndex), mousePosition).ToArray());
				
			}

			return false;
		}

		private bool TryHandleSelectionDisplacement(MapBehaviour mapBehaviour, Vector3 mousePosition)
		{
			EditorGUI.BeginChangeCheck();

			var position = Handles.PositionHandle(mapBehaviour.GetWorldPosition(SelectedIndex), mapBehaviour.transform.rotation);

			
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(mapBehaviour, "Node Position Set");
				mapBehaviour.SetWorldPosition(SelectedIndex, position);
				return true;
			}

			return false;
		}

		private bool TryHandleDeselect(MapBehaviour mapBehaviour)
		{
			var currentEvent = Event.current;
			
			if (!currentEvent.shift &&
			    !currentEvent.control &&
				currentEvent.type is EventType.MouseUp
			    && currentEvent.button == 0)
			{
				SelectedIndex = -1;
				currentEvent.Use();
				return true;
			}
			return false;
		}

		private bool TryHandleNodeDeletion(MapBehaviour mapBehaviour, SceneView sceneView, Vector3 mousePosition)
		{
			var currentEvent = Event.current;

			if (currentEvent.control &&
			    !currentEvent.shift)
			{
				if(TryFindClosestNode(mapBehaviour, mousePosition, out int targetIndex))
				{
					Handles.color = Color.red;
					
					Handles.DrawWireDisc(
						mapBehaviour.GetWorldPosition(targetIndex),
						mapBehaviour.transform.up, 
						ARROW_OFFSET
						);
					
					if (currentEvent.button == 0 &&
					    currentEvent.type is EventType.MouseDown)
					{
						Undo.RecordObject(mapBehaviour, "Delete Node");
						mapBehaviour.Remove(targetIndex);
						currentEvent.Use();

						if (mapBehaviour.Count == SelectedIndex)
						{
							SelectedIndex = targetIndex;
						}
						
					}
					Handles.color = Color.white;
					
					return true;
				}
			}
			return false;
		}

		private bool TryHandleEdgeCreation(MapBehaviour mapBehaviour, Vector3 mousePosition)
		{
			var currentEvent = Event.current;
			if (SelectedIndex != -1 &&
			    currentEvent.shift)
			{
				if(TryFindClosestNode(mapBehaviour, mousePosition, out var targetIndex))
				{
					if (currentEvent.type is EventType.MouseDown && currentEvent.button == 0)
					{
						Undo.RecordObject(mapBehaviour, $"Create Edge {SelectedIndex} => {targetIndex}");
						mapBehaviour.Connect(SelectedIndex, targetIndex);
					}
					else
					{
						Handles.color = Color.yellow;

						var start = mapBehaviour.GetWorldPosition(SelectedIndex);
						var end = mapBehaviour.GetWorldPosition(targetIndex);

						var dir = (end - start).normalized;

						if (dir != Vector3.zero)
						{

							start += dir * ARROW_OFFSET;
							end -= dir * ARROW_OFFSET;
							
							Handles.DrawLines(
								DrawArrow.CreatePoints(
									start,
									end
								).ToArray());
							Handles.color = Color.white;
							
						}
					}

					return true;
				}
				else
				{
					Handles.color = Color.yellow;
					Handles.DrawLines(DrawArrow.CreatePoints(mapBehaviour.GetWorldPosition(SelectedIndex),mousePosition).ToArray());
					Handles.color = Color.white;
				}
			}
			return false;
		}

		private bool TryFindClosestNode(MapBehaviour mapBehaviour, Vector3 mousePosition, out int index, float worldMinDistance=1)
		{
			float minDistance = float.MaxValue;
			index = -1;
			for (int i = 0; i < mapBehaviour.Count; i++)
			{
				if (i == SelectedIndex)
				{
					continue;
				}

				var pos = mapBehaviour.GetWorldPosition(i);

				var distance = Vector3.Distance(pos, mousePosition);
				float selectionDistance = HandleUtility.GetHandleSize(pos) * worldMinDistance;
						
				if (distance < selectionDistance && distance < minDistance)
				{
					minDistance = distance;
					index = i;
				}
			}
			return index != -1;
		}

		private static Vector3 GetMouseIntersectionPosition(MapBehaviour mapBehaviour, Event currentEvent)
		{
			Plane plane = new Plane(mapBehaviour.transform.up, mapBehaviour.transform.position);
			Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			plane.Raycast(ray, out var clickDistance);
			var mousePosition = ray.GetPoint(clickDistance);
			return mousePosition;
		}

		private bool TryHandleNodeSelection(MapBehaviour mapBehaviour, SceneView sceneView, Vector3 mousePosition)
		{
			if (SelectedIndex != -1)
			{
				Handles.color = Color.yellow;
				var pos = mapBehaviour.GetWorldPosition(SelectedIndex);
				Handles.DrawWireDisc(pos, sceneView.camera.transform.forward, ARROW_OFFSET);
				Handles.color = Color.white;
			}

			if (TryFindClosestNode(mapBehaviour, mousePosition, out var targetIndex))
			{
				if (Handles.Button(
					    mapBehaviour.GetWorldPosition(targetIndex),
					    sceneView.camera.transform.rotation,
					    ARROW_OFFSET,
					    .3f,
					    Handles.CircleHandleCap
				    ))
				{
					SelectedIndex = targetIndex;
					return true;
				}
				Handles.DrawWireDisc(mapBehaviour.GetWorldPosition(targetIndex), sceneView.camera.transform.forward, ARROW_OFFSET);
			}
			return false;
		}

		private static bool TryHandleNodeCreation(MapBehaviour mapBehaviour, Vector3 mousePosition)
		{
			var currentEvent = Event.current;
			if (currentEvent.control)
			{
				if(currentEvent.type is EventType.MouseDown && 
				   currentEvent.button == 0)
				{
					Undo.RecordObject(mapBehaviour, "Node Created");
					mapBehaviour.AddNodeAtWorldPosition(mousePosition);
					return true;
				}
				else
				{
					var oldMatrix = Handles.matrix;

					Handles.matrix = mapBehaviour.transform.localToWorldMatrix;
					Handles.color = Color.yellow;
					Handles.DrawWireDisc(mapBehaviour.transform.InverseTransformPoint(mousePosition),mapBehaviour.transform.up, ARROW_OFFSET);
					Handles.color = Color.white;
					Handles.matrix = oldMatrix;
				}
			}
			return false;
		}


		public void OnDrawHandles()
		{
			foreach (var o in targets)
			{
				if (o is MapBehaviour behaviour)
				{
					Handles.color = Color.yellow;
					Handles.DrawWireDisc(behaviour.transform.position, behaviour.transform.up, ARROW_OFFSET-.1f);
					Handles.color = Color.white;
					DrawNumberHandles(behaviour);
					DrawArrows(behaviour);
				}
			}

			Handles.color = Color.cyan;
			List<Vector3> selectionLines = new List<Vector3>(connectionsOut.Count * 2);
			foreach (EdgeData edgeData in connectionsOut)
			{
				selectionLines.Add(map.GetWorldPosition(edgeData.From));
				selectionLines.Add(map.GetWorldPosition(edgeData.To));
			}
			Handles.DrawLines(selectionLines.ToArray());
			
			Handles.color = new Color(1f,.5f,.2f);
			selectionLines = new List<Vector3>(connectionsIn.Count * 2);
			foreach (EdgeData edgeData in connectionsIn)
			{
				selectionLines.Add(map.GetWorldPosition(edgeData.From));
				selectionLines.Add(map.GetWorldPosition(edgeData.To));
			}
			Handles.DrawLines(selectionLines.ToArray());
			
		}

		private void DrawArrows(MapBehaviour map)
		{
			arrowsCountDict.Clear();
			Handles.color = Color.white;
			if (gizmoLines.Length != map.EdgesCount*6)
			{
				Array.Resize(ref gizmoLines, map.EdgesCount*6);
			}

			int offsetIndex = 0;
			for (var index = 0; index < map.EdgesCount; index++)
			{
				var edge = map.GetEdge(index);

				var key = (edge.From, edge.To);
				arrowsCountDict.TryGetValue(key, out var count);
				
				var start = map.GetWorldPosition(edge.From);
				var end = map.GetWorldPosition(edge.To);
				
				var diff = (end - start);
				var mag = (end - start).sqrMagnitude;
				var dir = diff.normalized;


				int side = edge.From < edge.To ? 1 : -1;

				start += SIDE_OFFSET * Vector3.Cross(dir, map.transform.up);
				end += SIDE_OFFSET * Vector3.Cross(dir, map.transform.up);
				
				if(count == 0)
				{
					start += dir * Mathf.Min(ARROW_OFFSET, mag);
					end -= dir * Mathf.Min(ARROW_OFFSET, mag);

					foreach (var (from, to) in DrawArrow.CreateSegments(start, end))
					{
						gizmoLines[offsetIndex++] = from;
						gizmoLines[offsetIndex++] = to;
					}
				}
				else
				{
					Vector3 midPoint = Vector3.Lerp(start, end,ARROW_OFFSET);
					midPoint += Vector3.Cross(dir, map.transform.up) *  SIDE_OFFSET * side * count;
					start += dir * Mathf.Min(ARROW_OFFSET, mag);
					Handles.DrawLine(start, midPoint);

					dir = (end - midPoint).normalized;
					
					end -= dir * Mathf.Min(ARROW_OFFSET, mag);
		
					foreach (var (from, to) in DrawArrow.CreateSegments(midPoint, end))
					{
						gizmoLines[offsetIndex++] = from;
						gizmoLines[offsetIndex++] = to;
					}
				}
				arrowsCountDict[key] = count + 1;
			}

			if (gizmoLines.Length > 0)
			{
				Handles.DrawLines(gizmoLines);
			}
		}

		private void DrawNumberHandles(MapBehaviour map)
		{
			GUIStyle textStyle = new GUIStyle()
			{
				alignment = TextAnchor.MiddleCenter,
				normal = new GUIStyleState()
				{
					textColor = Color.white
				},
				hover =  new GUIStyleState()
				{
					textColor = Color.white,
				} 
			};
			
			for (int i = 0; i < map.Count; i++)
			{
				Handles.color = SelectedIndex == i ? Color.yellow : Color.white;
				var pos = map.GetWorldPosition(i);

				if (map.TryGetContent(i, out NodeData data))
				{
					Handles.Label(pos, $"{data}", textStyle);
				}
				else
				{
					Handles.Label(pos, i.ToString(),textStyle);
					Handles.DrawWireDisc(pos, map.transform.up, ARROW_OFFSET/2f);
				}
			}
		}

		// Called when the active tool is set to this tool instance. Global tools are persisted by the ToolManager,
		// so usually you would use OnEnable and OnDisable to manage native resources, and OnActivated/OnWillBeDeactivated
		// to set up state. See also `EditorTools.{ activeToolChanged, activeToolChanged }` events.
		public override void OnActivated()
		{
			SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Map Tool"), .1f);
		}

		// Called before the active tool is changed, or destroyed. The exception to this rule is if you have manually
		// destroyed this tool (ex, calling `Destroy(this)` will skip the OnWillBeDeactivated invocation).
		public override void OnWillBeDeactivated()
		{
			SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Map Tool"), .1f);
		}
		
		[Overlay(editorWindowType = typeof(SceneView), defaultDisplay = false, displayName = "Node Content")]
		public class MapToolbar : Overlay, ITransientOverlay
		{
			public bool visible => 
				Instance != null && 
				ToolManager.IsActiveTool(Instance) && 
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
				Instance.OnIndexChange -= OnIndexChange;
				Instance.OnIndexChange += OnIndexChange;
				OnIndexChange(Instance.selectedIndex);
			}

			private void Terminate()
			{
				initialized = false;
				if (Instance)
				{
					Instance.OnIndexChange -= OnIndexChange;
				}
			}

			public override void OnWillBeDestroyed()
			{
				mainContainer?.Clear();
				if (Instance)
				{
					Terminate();
				}
				base.OnWillBeDestroyed();
			}

			private void OnIndexChange(int index)
			{
				content?.Clear();
				if (Instance &&  content != null)
				{
					if(index != -1 )
					{
						var serializedObject = new SerializedObject(Instance.map);

						var prop = serializedObject.FindProperty(nameof(MapBehaviour.data));
                        
						if(prop != null)
						{
							var contentProperty = prop.FindPropertyRelative(nameof(MapData.content));
							if (contentProperty != null)
							{
								var property = contentProperty.GetArrayElementAtIndex(index);
								var propElement = new PropertyField(property, index.ToString());
								propElement.BindProperty(property);
								content.Add(propElement);
							}
							else
							{
								Debug.Log("Content Prop not found");
							}
						}
						else
						{
							Debug.Log("Prop not found");
						}
					}
					else
					{
						for (int i = 0; i < Instance.map.Count; i++)
						{
							var aux = i;
							var nodeData = Instance.map.GetContent(i);

							var button = new ToolbarButton(() => Instance.Select(aux))
							{
								text = $"{i}:{(nodeData != null ? nodeData.GetType().Name : "Empty")}",
							};
							
							content.Add(button);
						}
					}
				}
			}
	
			public override VisualElement CreatePanelContent()
			{
				
				mainContainer = new VisualElement();
				content = new ScrollView(ScrollViewMode.Vertical);
				content.style.maxHeight = 200;
				mainContainer.Add(content);
				content.style.minWidth = 200;
				if (Instance)
				{
					content.Add(new Label("None"));
				}
				else
				{
					content.Add(new Label("Null"));
				}

				if (Instance)
				{
					OnIndexChange(Instance.SelectedIndex);
				}
				
				return mainContainer;
			}
		}

		private void Select(int aux) => SelectedIndex=aux;
	}
}
