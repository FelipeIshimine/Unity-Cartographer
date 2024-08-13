using System;

namespace Cartographer.Utilities.Attributes
{
	public class DropdownPathAttribute : Attribute
	{
		public readonly string Path;
		public readonly string Name;

		public DropdownPathAttribute(string path)
		{
			Path = $"{path}";
		}

		public DropdownPathAttribute(string path,string name)
		{
			Path = $"{path}";
			Name = name;
		}
	}
}