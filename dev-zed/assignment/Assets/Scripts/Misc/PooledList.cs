using UnityEngine;
using System;

namespace Sample
{
	public struct PooledList<T> : IPooledArray<T, PooledList<T>>
	{
		public const int MIN_CAPACITY = 0;

		PooledArray<T> mPooledArray;
		int mSize;

		public int size { get { return mSize; } }
		public int capacity { get { return mPooledArray.capacity; } }

		public T this[int index] // remark : https://stackoverflow.com/a/17106940
        {
			get { return mPooledArray[index]; }
			set { mPooledArray[index] = value; }
		}

		public PooledList(int capacity)
		{
			mPooledArray = new PooledArray<T>(Mathf.Max(MIN_CAPACITY, capacity));
			mSize = 0;
		}

		public void Clear()
		{
			mPooledArray.Clear();
			mSize = 0;
		}

		void Reallocate(int size, int capacity)
		{
			var newPooledArray = new PooledArray<T>(PooledArray<T>.CalcCapacity(capacity));
			Array.Copy(this.mPooledArray.array, 0, newPooledArray.array, 0, size);
			mPooledArray.Clear();
			mPooledArray = newPooledArray;
		}

		public void Add(T item)
		{
			if (mPooledArray.capacity <= mSize)
			{
				Reallocate(mSize, mSize + 1);
			}

			mPooledArray[mSize] = item;
			mSize++;
		}

		public bool Remove(T item)
		{
			int idx = this.IndexOf(item);
			if (idx >= 0)
			{
				this.RemoveAt(idx);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			if (index >= this.mSize)
			{
				throw new ArgumentOutOfRangeException("index", index, "PooledList.RemoveAt");
			}
			this.mSize--;
			if (index < this.mSize)
			{
				Array.Copy(this.mPooledArray.array, index + 1, this.mPooledArray.array, index, this.mSize - index);
			}
			this.mPooledArray[this.mSize] = default(T);
		}

		public int IndexOf(T value)
		{
			return Array.IndexOf<T>(mPooledArray.array, value, 0, mSize);
		}

		public void MoveTo(out PooledArray<T> destination)
		{
			destination = mPooledArray;
			mPooledArray = new PooledArray<T>();
			mSize = 0;
		}
	}
}