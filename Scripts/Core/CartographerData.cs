using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core
{
	public class CartographerData : ScriptableObject
	{
		public static CartographerData Instance { get; private set; }
 		
		private void OnEnable()
		{
			Instance = this;
		}

	}
}