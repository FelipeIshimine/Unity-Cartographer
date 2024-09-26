using System;
using Cartographer.Utilities.Attributes;
using UnityEngine;

namespace Cartographer.Utilities
{
	[Serializable]
	public struct Optional<T>
	{
		[SerializeField] private bool enabled;
		[SerializeField] private T value;

		public bool Enabled
		{
			get => enabled;
			set => enabled = value;
		}

		public T Value => value;

		public Optional(T initialValue)
		{
			enabled = true;
			value = initialValue;
		}
		public Optional(T initialValue, bool initialEnable)
		{
			enabled = initialEnable;
			value = initialValue;
		}
    
		public static implicit operator T(Optional<T> optional) => optional.value;
		public static implicit operator bool(Optional<T> optional) => optional.enabled;

		public void SetValue(T nValue) => value = nValue;
		private void SetValueThenEnable(T nValue)
		{
			SetValue(nValue);
			enabled = true;
		}
	}



	[Serializable]
	public class OptionalReference<T>
	{
		[SerializeField] public bool isEnabled;
		[SerializeReference,TypeDropdown] public T value;

		public bool IsEnabled
		{
			get => isEnabled;
			set => isEnabled = value;
		}

		public T Value => value;

		public OptionalReference(T initialValue)
		{
			isEnabled = true;
			value = initialValue;
		}
		public OptionalReference(T initialValue, bool initialEnable)
		{
			isEnabled = initialEnable;
			value = initialValue;
		}
    
		public static implicit operator T(OptionalReference<T> optional) => optional.value;
		public static implicit operator bool(OptionalReference<T> optional) => optional.isEnabled;

		public void SetValue(T nValue) => value = nValue;
		private void SetValueThenEnable(T nValue)
		{
			SetValue(nValue);
			isEnabled = true;
		}
	}
}