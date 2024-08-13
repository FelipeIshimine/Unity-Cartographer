using System;
using System.Collections.Generic;
using Random = System.Random;

namespace Cartographer.Core
{
	public static class ListExtensions
	{
		public static T Last<T>(this List<T> source) => source[^1];
		public static T First<T>(this List<T> source) => source[0];

		public static void ReplaceAt<T>(this List<T> source, int index,T nElement)
		{
			source.RemoveAt(index);
			source.Insert(index,nElement);
		}
    
		public static int InsertSorted<T>(this List<T> source, T nElement)
		{
			var index = source.BinarySearch(nElement);
			if (index < 0) index = ~index;
			source.Insert(index, nElement);
			return index;
		}
		public static int InsertSorted<T>(this List<T> source, T nElement, IComparer<T> comparer)
		{
			var index = source.BinarySearch(nElement,comparer);
			if (index < 0) index = ~index;
			source.Insert(index, nElement);
			return index;
		}

		public static int IndexOf<T>(this IReadOnlyList<T> source, T target)
		{
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if (target.Equals(source[i]))
					return i;
			}
			return -1;
		}
    
    
		public static T FindLowest<T>(this IReadOnlyList<T> source, Func<T, float> testing)
		{
			float lowest = int.MaxValue;
			T current = default(T);
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if (testing.Invoke(source[i]) < lowest)
				{
					lowest = testing.Invoke(source[i]);
					current = source[i];
				}
			}
			return current;
		}
    
		public static T FindLowest<T>(this List<T> source, Func<T, float> testing)
		{
			float lowest = int.MaxValue;
			T current = default(T);
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if (testing.Invoke(source[i]) < lowest)
				{
					lowest = testing.Invoke(source[i]);
					current = source[i];
				}
			}
			return current;
		}
    
		public static T FindHighest<T>(this List<T> source, Func<T, float> testing)
		{
			float highest = float.MinValue;
			T current = default(T);
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if (testing.Invoke(source[i]) > highest)
				{
					highest = testing.Invoke(source[i]);
					current = source[i];
				}
			}
			return current;
		}
    
		public static List<T> ExtractAll<T>(this List<T> source, Func<T, bool> validation)
		{
			List<T> values = new List<T>();
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if(validation.Invoke(source[i]))
				{
					values.Add(source[i]);
					source.RemoveAt(i);
				}
			}
			return values;
		}
    
		public static T TakeAt<T>(this List<T> source, int index, bool removePosition = true) 
		{
			var value = source[index];
			if (removePosition) 
				source.RemoveAt(index);
			else
				source[index] = default;
			return value;
		}

		public static T PopLast<T>(this List<T> source)
		{
			T element = source[^1];
			source.RemoveAt(source.Count-1);
			return element;
		}

		public static void Shuffle<T>(this List<T> source)
		{
			for (int i = source.Count - 1; i >= 0; i--)
			{
				int j = UnityEngine.Random.Range(0, i);
				(source[i], source[j]) = (source[j], source[i]);
			}
		}
    
		public static void ThreadSafeShuffle<T>(this List<T> source, Random random)
		{
			for (int i = source.Count - 1; i >= 0; i--)
			{
				int j = random.Next(0, i);
				(source[i], source[j]) = (source[j], source[i]);
			}
		}

		public static int FindMinIndex<T>(this List<T> source, Func<T, float> function)
		{
			float min = float.MaxValue;
			int current = -1;
			for (int i = 0; i < source.Count; i++)
			{
				float value = function.Invoke(source[i]); 
				if (min > value)
				{
					min = value;
					current = i;
				}
			}
			return current;
		}
    
		public static T FindMin<T>(this List<T> source, Func<T, float> function) => source[FindMinIndex(source, function)];


		public static int ReplaceWhen<T>(this List<T> source, Predicate<T> validation, T value)
		{
			int count = 0;
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if (validation.Invoke(source[i]))
				{
					count++;
					source[i] = value;
				}
			}
			return count;
		}
    
		public static int SwapValues<T>(this List<T> source, T valueA, T valueB) where T : IEquatable<T>
		{
			int count = 0;
			for (int i = source.Count - 1; i >= 0; i--)
			{
				var value = source[i];
				if (value.Equals(valueA))
				{
					source[i] = valueB;
					count++;
				}
				else if (value.Equals(valueB))
				{
					source[i] = valueA;
					count++;
				}
			}
			return count;
		}
	}
}