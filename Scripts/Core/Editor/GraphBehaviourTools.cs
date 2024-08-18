using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Cartographer.Core.Editor
{
	[EditorTool("Graph Tool", typeof(GraphBehaviour))]
	public class GraphBehaviourTools : EditorTool, IDrawSelectedHandles
	{
		private GUIContent toolIcon;
		public override GUIContent toolbarIcon
		{
			get
			{
				if (toolIcon == null)
				{
					toolIcon = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>("Packages/ishimine.cartographer/Icons/Map Icon.png"));
				}
				return toolIcon;
			}
		}
		
		
		public event Action<GraphBehaviour> OnMapChange; 
		public event Action<int> OnIndexChange; 
		private const float ARROW_OFFSET = .5f;
		private const float SIDE_OFFSET = .1f;

		private Vector3[] gizmoLines = Array.Empty<Vector3>();

		readonly Dictionary<(int from,int to),int> arrowsCountDict = new();

		public static GraphBehaviourTools Instance { get; private set; }

		private HashSet<EdgeData> connectionsOut = new HashSet<EdgeData>();
		private HashSet<EdgeData> connectionsIn = new HashSet<EdgeData>();

		private int selectedIndex = -1;
		public int SelectedIndex
		{
			get => selectedIndex;
			set
			{
				if(selectedIndex != value)
				{
					selectedIndex = value;
					RefreshConnections();
					OnIndexChange?.Invoke(value);
				}
			}
		}

		private void RefreshConnections()
		{
			connectionsIn.Clear();
			connectionsOut.Clear();
			if (selectedIndex != -1)
			{
				graph.data.FindAllPathsFrom(selectedIndex, ref connectionsOut);
				graph.data.FindAllPathsTo(selectedIndex, ref connectionsIn);
			}
		}

	

		public GraphBehaviour graph;
		public GraphBehaviour GetGraph() => graph;

		public void SetGraph(GraphBehaviour value)
		{
			if (graph != value)
			{
				RefreshConnections();
				SelectedIndex = -1;
				
				if (graph != null)
				{
					graph.OnLoad -= SetGraph;
					graph.data.OnAddEdge -= OnEdgeAdded;
					graph.data.OnRemoveEdge -= OnEdgeRemoved;
				}

				graph = value;
				
				
				if (graph != null)
				{
					graph.OnLoad += SetGraph;
					graph.data.OnAddEdge += OnEdgeAdded;
					graph.data.OnRemoveEdge += OnEdgeRemoved;
				}

				OnMapChange?.Invoke(graph);
			}
		}

		private void OnEdgeAdded(EdgeData edge)
		{
			RefreshConnections();
		}

		private void OnEdgeRemoved(EdgeData edge)
		{
			RefreshConnections();
		}


		static GraphBehaviourTools()
		{
			Selection.selectionChanged -= OnSelectionChange;
			Selection.selectionChanged += OnSelectionChange;
		}

		private static void OnSelectionChange()
		{
			if (Selection.GetFiltered<GraphBehaviour>(SelectionMode.TopLevel).Length > 0)
			{
				ToolManager.SetActiveTool<GraphBehaviourTools>();
			}
		}

		[Shortcut("Activate Map Tool", typeof(SceneView), KeyCode.M)]
		static void PlatformToolShortcut()
		{
			if (Selection.GetFiltered<GraphBehaviour>(SelectionMode.TopLevel).Length > 0)
				ToolManager.SetActiveTool<GraphBehaviourTools>();
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

			
			if (target is GraphBehaviour graphBehaviour)
			{
				SetGraph(graphBehaviour);
				var currentEvent = Event.current;
				
				
				var mousePosition = GetMouseIntersectionPosition(graphBehaviour, currentEvent);

				if (!currentEvent.shift && currentEvent.alt)
				{
					return;	
				}

				/*if (currentEvent.shift)
				{
					currentEvent.Use();
				}*/
				
				if (TryHandleNodeDeletion(graphBehaviour, sceneView, mousePosition))
				{
					return;
				}
				
				
				if (SelectedIndex == -1)
				{
					if (TryHandleNodeCreation(graphBehaviour,mousePosition))
					{
						return;
					}

					if (TryHandleNodeSelection(graphBehaviour,sceneView,mousePosition))
					{
						return;
					}
				}
				else
				{
					if (TryHandleEdgeDeletion(graphBehaviour, sceneView, mousePosition))
					{
						return;
					}
					
					if (TryHandleSelectionDisplacement(graphBehaviour, mousePosition))
					{
						return;
					}
						
					if (TryHandleMerge(graphBehaviour, mousePosition))
					{
						return;
					}
					
					if (TryHandleEdgeCreation(graphBehaviour,mousePosition))
					{
						return;
					}

					if (TryHandleNodeCreationWithEdge(graphBehaviour, mousePosition))
					{
						return;
					}
					
					if (TryHandleNodeSelection(graphBehaviour,sceneView, mousePosition))
					{
						return;
					}
					
					if(TryHandleDeselect(graphBehaviour))
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

		private bool TryHandleMerge(GraphBehaviour graphBehaviour, Vector3 mousePosition)
		{
			var currentEvent = Event.current;
			if (SelectedIndex != -1 &&
			    currentEvent.shift && 
			    currentEvent.alt)
			{
				if(TryFindClosestNode(graphBehaviour, mousePosition, out var targetIndex))
				{
					if (currentEvent.type is EventType.MouseDown && currentEvent.button == 0)
					{
						Undo.RecordObject(graphBehaviour, $"Merge Nodes {SelectedIndex} => {targetIndex}");
						graphBehaviour.data.Merge(SelectedIndex, targetIndex);
					}
					else
					{
						Handles.color = Color.yellow;

						var start = graphBehaviour.GetWorldPosition(SelectedIndex);
						var end = graphBehaviour.GetWorldPosition(targetIndex);

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
					Handles.DrawLines(DrawArrow.CreatePoints(graphBehaviour.GetWorldPosition(SelectedIndex),mousePosition).ToArray());
					Handles.color = Color.white;
				}
			}
			return false;
		}

		private bool TryHandleEdgeDeletion(GraphBehaviour GraphBehaviour, SceneView sceneView, Vector3 mousePosition)
		{
			var current = Event.current;
			if (current.control && 
			    current.shift &&
			    TryFindClosestNode(GraphBehaviour, mousePosition, out var index))
			{
				var pos = GraphBehaviour.GetWorldPosition(index);

				var start = GraphBehaviour.GetWorldPosition(SelectedIndex);
				var end = pos;

				var dir = (end - start).normalized;

				start += dir * ARROW_OFFSET;
				end -= dir * ARROW_OFFSET;

				Handles.color = Color.red;
				Handles.DrawLines(DrawArrow.CreatePoints(start,end).ToArray());
				Handles.color = Color.white;
				
				if (current.type is EventType.MouseDown)
				{
					GraphBehaviour.RemoveEdge(SelectedIndex, index);
				}
				return true;
			}

			return false;
		}

		private bool TryHandleNodeCreationWithEdge(GraphBehaviour GraphBehaviour, Vector3 mousePosition)
		{
			var current = Event.current;

			if (current.control)
			{
				if (current.isMouse &&
				    current.type is EventType.MouseDown &&
				    current.button == 0)
				{
					Undo.RecordObject(GraphBehaviour, "Node Created");
					int index = GraphBehaviour.AddNodeAtWorldPosition(mousePosition);
					GraphBehaviour.Connect(SelectedIndex, index);
					SelectedIndex = index;
					return true;
				}

				var oldMatrix = Handles.matrix;

				Handles.matrix = GraphBehaviour.transform.localToWorldMatrix;
				Handles.color = Color.yellow;
				Handles.DrawWireDisc(GraphBehaviour.transform.InverseTransformPoint(mousePosition),GraphBehaviour.transform.up, ARROW_OFFSET);
				Handles.color = Color.white;
				Handles.matrix = oldMatrix;
				
				
				
				Handles.DrawLines(DrawArrow.CreatePoints(GraphBehaviour.GetWorldPosition(SelectedIndex), mousePosition).ToArray());
				
			}

			return false;
		}

		private bool TryHandleSelectionDisplacement(GraphBehaviour GraphBehaviour, Vector3 mousePosition)
		{
			EditorGUI.BeginChangeCheck();

			var position = Handles.PositionHandle(GraphBehaviour.GetWorldPosition(SelectedIndex), GraphBehaviour.transform.rotation);

			
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(GraphBehaviour, "Node Position Set");
				GraphBehaviour.SetWorldPosition(SelectedIndex, position);
				return true;
			}

			return false;
		}

		private bool TryHandleDeselect(GraphBehaviour GraphBehaviour)
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

		private bool TryHandleNodeDeletion(GraphBehaviour graphBehaviour, SceneView sceneView, Vector3 mousePosition)
		{
			var currentEvent = Event.current;

			if (currentEvent.control &&
			    !currentEvent.shift)
			{
				if(TryFindClosestNode(graphBehaviour, mousePosition, out int targetIndex, 1,false))
				{
					Handles.color = Color.red;
					
					Handles.DrawWireDisc(
						graphBehaviour.GetWorldPosition(targetIndex),
						graphBehaviour.transform.up, 
						ARROW_OFFSET
						);
					
					if (currentEvent.button == 0 &&
					    currentEvent.type is EventType.MouseDown)
					{
						Undo.RecordObject(graphBehaviour, "Delete Node");
						graphBehaviour.Remove(targetIndex);
						currentEvent.Use();

						if (graphBehaviour.Count == SelectedIndex)
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

		private bool TryHandleEdgeCreation(GraphBehaviour graphBehaviour, Vector3 mousePosition)
		{
			var currentEvent = Event.current;
			if (SelectedIndex != -1 &&
			    currentEvent.shift)
			{
				if(TryFindClosestNode(graphBehaviour, mousePosition, out var targetIndex))
				{
					if (currentEvent.type is EventType.MouseDown && currentEvent.button == 0)
					{
						Undo.RecordObject(graphBehaviour, $"Create Edge {SelectedIndex} => {targetIndex}");
						graphBehaviour.Connect(SelectedIndex, targetIndex);
					}
					else
					{
						Handles.color = Color.yellow;

						var start = graphBehaviour.GetWorldPosition(SelectedIndex);
						var end = graphBehaviour.GetWorldPosition(targetIndex);

						var dir = (end - start).normalized;
						Handles.DrawWireDisc(end, graph.transform.up, ARROW_OFFSET);

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
					Handles.DrawLines(DrawArrow.CreatePoints(graphBehaviour.GetWorldPosition(SelectedIndex),mousePosition).ToArray());
					Handles.color = Color.white;
				}
			}
			return false;
		}

		private bool TryFindClosestNode(GraphBehaviour graphBehaviour, Vector3 mousePosition, out int index, float worldMinDistance=1, bool scaleWithCameraDistance = false)
		{
			float minDistance = float.MaxValue;
			index = -1;
			for (int i = 0; i < graphBehaviour.Count; i++)
			{
				if (i == SelectedIndex)
				{
					continue;
				}

				var pos = graphBehaviour.GetWorldPosition(i);

				var distance = Vector3.Distance(pos, mousePosition);
				float selectionDistance = worldMinDistance;

				if (scaleWithCameraDistance)
				{
					selectionDistance *= HandleUtility.GetHandleSize(pos);
				}
				
				if (distance < selectionDistance && distance < minDistance)
				{
					minDistance = distance;
					index = i;
				}
			}
			return index != -1;
		}

		private static Vector3 GetMouseIntersectionPosition(GraphBehaviour graphBehaviour, Event currentEvent)
		{
			Plane plane = new Plane(graphBehaviour.transform.up, graphBehaviour.transform.position);
			Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			plane.Raycast(ray, out var clickDistance);
			var mousePosition = ray.GetPoint(clickDistance);
			return mousePosition;
		}

		private bool TryHandleNodeSelection(GraphBehaviour graphBehaviour, SceneView sceneView, Vector3 mousePosition)
		{
			if (SelectedIndex != -1)
			{
				Handles.color = Color.yellow;
				var pos = graphBehaviour.GetWorldPosition(SelectedIndex);
				Handles.DrawWireDisc(pos, sceneView.camera.transform.forward, ARROW_OFFSET);
				Handles.color = Color.white;
			}

			if (TryFindClosestNode(graphBehaviour, mousePosition, out var targetIndex, 1, false))
			{
				if (Handles.Button(
					    graphBehaviour.GetWorldPosition(targetIndex),
					    sceneView.camera.transform.rotation,
					    ARROW_OFFSET,
					    .3f,
					    Handles.CircleHandleCap
				    ))
				{
					SelectedIndex = targetIndex;
					return true;
				}
				Handles.DrawWireDisc(graphBehaviour.GetWorldPosition(targetIndex), sceneView.camera.transform.forward, ARROW_OFFSET);
			}
			return false;
		}

		private static bool TryHandleNodeCreation(GraphBehaviour graphBehaviour, Vector3 mousePosition)
		{
			var currentEvent = Event.current;
			if (currentEvent.control)
			{
				if(currentEvent.type is EventType.MouseDown && 
				   currentEvent.button == 0)
				{
					Undo.RecordObject(graphBehaviour, "Node Created");
					graphBehaviour.AddNodeAtWorldPosition(mousePosition);
					return true;
				}
				else
				{
					var oldMatrix = Handles.matrix;

					Handles.matrix = graphBehaviour.transform.localToWorldMatrix;
					Handles.color = Color.yellow;
					Handles.DrawWireDisc(graphBehaviour.transform.InverseTransformPoint(mousePosition),graphBehaviour.transform.up, ARROW_OFFSET);
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
				if (o is GraphBehaviour behaviour)
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
				var from = graph.GetWorldPosition(edgeData.From);
				var to = graph.GetWorldPosition(edgeData.To);
				var dir = (to - from).normalized;
				dir = Vector3.Cross(dir, graph.transform.up);
				selectionLines.Add(from + dir*.025f);
				selectionLines.Add(to + dir*.025f);
				
				/*selectionLines.Add(graph.GetWorldPosition(edgeData.From));
				selectionLines.Add(graph.GetWorldPosition(edgeData.To));*/
			}
			Handles.DrawLines(selectionLines.ToArray());
			
			Handles.color = new Color(1f,.6f,.4f);
			selectionLines = new List<Vector3>(connectionsIn.Count * 2);
			foreach (EdgeData edgeData in connectionsIn)
			{
				var from = graph.GetWorldPosition(edgeData.From);
				var to = graph.GetWorldPosition(edgeData.To);
				var dir = (to - from).normalized;
				dir = Vector3.Cross(dir, graph.transform.up);
				selectionLines.Add(from - dir*.025f);
				selectionLines.Add(to - dir*.025f);
				
				/*var dir = graph.transform.up * .1f;
				selectionLines.Add(graph.GetWorldPosition(edgeData.From)+dir);
				selectionLines.Add(graph.GetWorldPosition(edgeData.To)+dir);*/
			}
			Handles.DrawLines(selectionLines.ToArray());
			
		}

		private void DrawArrows(GraphBehaviour map)
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

		private void DrawNumberHandles(GraphBehaviour map)
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
					if (selectedIndex == i)
					{
						Handles.Label(pos + Vector3.up, $"[{i}] {data}", textStyle);
					}
					else
					{
						Handles.Label(pos, $"[{i}] {data}", textStyle);
					}
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


		internal void Select(int aux) => SelectedIndex=aux;
		
		
		
	}
	
	
	
}