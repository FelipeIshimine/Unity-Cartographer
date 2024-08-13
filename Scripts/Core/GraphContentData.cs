using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cartographer.Core
{
	[System.Serializable]
	public abstract class GraphContentData<T> : GraphContentData, IList<T>
	{
		[SerializeField] private List<T> serializedData = new();
		public IEnumerator<T> GetEnumerator() => serializedData.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)serializedData).GetEnumerator();

		public override void Swap(int from, int to) => (serializedData[from], serializedData[to]) = (serializedData[to], serializedData[from]);

		public override void SetSize(int count)
		{
			int difference = count - serializedData.Count;
			
			if (difference > 0)
			{
				for (int i = 0; i < difference; i++)
				{
                    AddNew();
				}
			}
			else if(difference < 0)
			{
				for (int i = 0; i > difference; i--)
				{
					RemoveLast();
				}
			}
			
			
			
		}

		public override void AddNew() => serializedData.Add(default);
		public override void RemoveLast() => serializedData.RemoveAt(serializedData.Count-1);

		public void Add(T item) => serializedData.Add(item);

		public void Clear() => serializedData.Clear();

		public bool Contains(T item) => serializedData.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => serializedData.CopyTo(array, arrayIndex);

		public bool Remove(T item) => serializedData.Remove(item);

		public int Count => serializedData.Count;

		public bool IsReadOnly => false;

		public int IndexOf(T item) => serializedData.IndexOf(item);

		public void Insert(int index, T item) => serializedData.Insert(index, item);

		public void RemoveAt(int index) => serializedData.RemoveAt(index);

		public T this[int index]
		{
			get => serializedData[index];
			set => serializedData[index] = value;
		}
	}


	public abstract class GraphContentData
	{
		public abstract void Swap(int from, int to);
		public abstract void RemoveLast();
		public abstract void AddNew();
		public abstract void SetSize(int count);
	}

	[System.Serializable] public class NodeIsFree : GraphContentData<bool> { }
	[System.Serializable] public class NodeWeight : GraphContentData<float> { }
}