using UnityEngine;
using System;
using System.Collections.Generic;

namespace Sample
{
	public interface IPooledArray<ItemT, PooledContainerT>// : IDisposable
		where PooledContainerT : IPooledArray<ItemT, PooledContainerT>
	{
		int size { get; }
		int capacity { get; }
		ItemT this[int index] { get; set; }
		void Clear();
	}

	public struct PooledArray<T> : IPooledArray<T, PooledArray<T>>
	{
		static readonly T[] EMPTY_ARRAY = new T[0];
		private static readonly int[] PRIMES_TABLE = new int[]
		{
			11,
			19,
			37,
			73,
			109,
			163,
			251,
			367,
			557,
			823,
			1237,
			1861,
			2777,
			4177,
			6247,
			9371,
			14057,
			21089,
			31627,
			47431,
			71143,
			106721,
			160073,
			240101,
			360163,
			540217,
			810343,
			1215497,
			1823231,
			2734867,
			4102283,
			6153409,
			9230113,
			13845163
		};

		const int MAX_CAPACITY = 2146435071;
		T[] mArray;
		int mSize;

		public T[] array
		{
			get
			{
				if (mArray == null ||
					mArray.Length == 0)
					return EMPTY_ARRAY;
				return mArray;
			}
		}
		public int capacity
		{
			get
			{
				if (mArray == null)
					return 0;
				return mArray.Length;
			}
		}
		public int size { get { return mSize; } }

        public T this[int index]		
		{
			get { return mArray[index]; }
			set { mArray[index] = value; }
		}

		public PooledArray(int size)
		{
			mArray = null;
			mSize = 0;
			Allocate(size);
		}

		public void Clear()
		{
			if (mArray != null)
				ObjectPoolManager.Return(mArray);

			mArray = null;
			mSize = 0;
		}

		public static PooledArray<T> CreateFrom(T[] source)
		{
			var p = new PooledArray<T>(source.Length);
			Array.Copy(source, 0, p.array, 0, source.Length);
			return p;
		}

		public static PooledArray<T> CreateFrom(List<T> source)
		{
			var p = new PooledArray<T>(source.Count);
			source.CopyTo(p.array);
			return p;
		}

		public static int CalcCapacity(int size)
		{
			for (int i = 0; i < PRIMES_TABLE.Length; ++i)
			{
				if (PRIMES_TABLE[i] >= size)
					return PRIMES_TABLE[i];
			}

			var maxPrime = PRIMES_TABLE[PRIMES_TABLE.Length-1];

			int result = size;

			try
			{
				var quotient = Mathf.FloorToInt((float)size / (float)maxPrime);
				result = checked((quotient + 1) * maxPrime);
				Debug.Assert(result >= size, "PooledArray.CalcCapacity : wrong calculation.");
				result = Mathf.Max(size, result);
			}
			catch (System.OverflowException)
			{
				result = Mathf.Max(size, MAX_CAPACITY);
			}

			return result;

			//var c = Mathf.NextPowerOfTwo(size);
			//if (c > MAX_CAPACITY)
			//{
			//	c = MAX_CAPACITY;
			//}
			//return c;
		}

		public void CopyTo(ref int[] target)
		{
			if (target == null ||
				target.Length < size)
				target = new int[size];
			Array.Copy(array, 0, target, 0, size);
		}

		public PooledArray<int> CopyTo()
		{
			var result = new PooledArray<int>(size);
			CopyTo(ref result.mArray);
			return result;
		}

		public void Reallocate(int size)
		{
			if (mSize > size)
				throw new ArgumentOutOfRangeException();

			var newCapacity = CalcCapacity(size);
			if (this.capacity < newCapacity)
			{
				var newArray = ObjectPoolManager.NewArray<T>(newCapacity);
				if (mArray != null)
				{
					Array.Copy(this.mArray, 0, newArray, 0, mSize);
					ObjectPoolManager.Return(this.mArray);
				}
				mArray = newArray;
			}

			mSize = size;
		}

		void Allocate(int size)
		{
			Clear();

			mSize = Mathf.Max(0, size);

			if (mSize <= 0)
			{
				mArray = null;
			}
			else
			{
				mArray = ObjectPoolManager.NewArray<T>(CalcCapacity(mSize));
			}
		}
	}
}