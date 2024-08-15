using System;
using UnityEngine;

namespace Cartographer.Utilities.Attributes
{
	public class TypeDropdownAttribute : PropertyAttribute
	{
		public readonly bool UseAbsolutePosition;
		public TypeDropdownAttribute(bool useAbsolutePosition = true)
		{
			UseAbsolutePosition = useAbsolutePosition;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class TypeDropdownNameAttribute : Attribute
	{
		public readonly string Name;
		public TypeDropdownNameAttribute(string name)
		{
			Name = name;
		}
	}
}