using UnityEditor;
using UnityEngine;

namespace Cartographer.Core.Editor
{
	public static class CartographerUtilities
	{
		public static string FolderPath = "Assets/ScriptableObjects/Cartographer";
		public static string FileName = "Cartographer.asset";
		
		public static CartographerData LoadOrCreateData()
		{
			if (!AssetDatabase.IsValidFolder(FolderPath))
			{
				if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
				{
					AssetDatabase.CreateFolder("Assets","ScriptableObjects");
				}
				
				AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Cartographer");
			}

			CartographerData cartographerData =
				AssetDatabase.LoadAssetAtPath<CartographerData>($"{FolderPath}/{FileName}");
			if (!cartographerData)
			{
				cartographerData = ScriptableObject.CreateInstance<CartographerData>();
				AssetDatabase.CreateAsset(cartographerData, $"{FolderPath}/{FileName}");
			}
			return cartographerData;
		}
	}
}