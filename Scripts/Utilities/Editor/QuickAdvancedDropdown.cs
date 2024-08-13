using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Cartographer.Utilities.Editor
{
	public class QuickAdvancedDropdown : AdvancedDropdown
	{
		private readonly string title;
		
		private readonly List<string> labels;
		private readonly Dictionary<string,int> dictionaries = new Dictionary<string, int>();
		
		private readonly Action<int> callback;
		private readonly char splitCharacter;

		private readonly Dictionary<string, AdvancedDropdownItem> pathToDropdownItems =
			new Dictionary<string, AdvancedDropdownItem>();
	
		public QuickAdvancedDropdown(string title, string[] labels, Action<int> callback, char splitCharacter = '/') : base(new AdvancedDropdownState())
		{
			this.title = title;
			for (int i = 0; i < labels.Length; i++)
			{
				dictionaries[labels[i]] = i;
			}
			this.labels = new List<string>(labels);
			this.labels.Sort((x,y)=>String.Compare(x, y, StringComparison.Ordinal));
			this.callback = callback;
			this.splitCharacter = splitCharacter;
		}
		
		
		protected override AdvancedDropdownItem BuildRoot()
		{
			AdvancedDropdownItem root = new AdvancedDropdownItem(title);
		
			for (int i = 0; i < labels.Count; i++)
			{
				var split = labels[i].Split(splitCharacter);

				var previous = root;
				for (var j = 0; j < split.Length; j++)
				{
					var s = split[j];
					if (!pathToDropdownItems.TryGetValue(s, out var item) || j == split.Length-1)
					{
						item = pathToDropdownItems[s] = new AdvancedDropdownItem(s)
						{
							id = dictionaries[s]
						};
						previous.AddChild(item);
					}
					previous = item;
				}
			}
			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item) => callback.Invoke(item.id);
	}
}