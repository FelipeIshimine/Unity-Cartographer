using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Cartographer.Core.Editor
{
	[InitializeOnLoad]
	public class CartographerAssetPostprocessor : AssetPostprocessor
	{
		static CartographerAssetPostprocessor()
		{
			Init();
		}
		
		public static void Init()
		{
			var cartographerData = CartographerUtilities.LoadOrCreateData();
			cartographerData.SetNodeTypes(FindOrCreateNodeTypes());
			CartographerWindow.RequestRefresh();
			AssetDatabase.SaveAssetIfDirty(cartographerData);
		}

		public static IEnumerable<NodeType> FindOrCreateNodeTypes()
		{
			foreach (var type in TypeCache.GetTypesDerivedFrom<NodeType>())
			{
				if(type.IsAbstract) continue;
				
				var guids = AssetDatabase.FindAssets($"t:{type.Name}");
				NodeType asset;
				if (guids.Length == 0)
				{
					asset = (NodeType)ScriptableObject.CreateInstance(type);
					AssetDatabase.CreateAsset(asset, $"Assets/ScriptableObjects/Cartographer/{type.Name}.asset");
				}
				else
				{
					asset = AssetDatabase.LoadAssetAtPath<NodeType>($"Assets/ScriptableObjects/Cartographer/{type.Name}.asset");
				}

				yield return asset;
			}
		}


		private static void OnPostprocessAllAssets(string[] importedAssets,
		                                           string[] deletedAssets,
		                                           string[] movedAssets,
		                                           string[] movedFromAssetPaths)
		{
			Init();
		}
	}
	
}