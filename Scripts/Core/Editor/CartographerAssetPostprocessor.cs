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
			CartographerWindow.RequestRefresh();
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