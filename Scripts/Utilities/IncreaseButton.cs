using UnityEngine;

namespace Cartographer.Utilities
{
	public class IncreaseButton : PropertyAttribute
	{
		public readonly bool HideLabel;

		public IncreaseButton(bool hideLabel = false)
		{
			HideLabel = hideLabel;
		}
	}
}