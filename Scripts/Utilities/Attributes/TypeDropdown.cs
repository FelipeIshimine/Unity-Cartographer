using UnityEngine;

namespace Cartographer.Utilities.Attributes
{
	public class TypeDropdownAttribute : PropertyAttribute
	{
		public readonly bool UseAbsolutePosition = true;

		public TypeDropdownAttribute(bool useAbsolutePosition = true)
		{
			this.UseAbsolutePosition = useAbsolutePosition;
		}
	}
}