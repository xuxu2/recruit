using UnityEngine;
using System;
using System.Collections.Generic;

namespace Sample
{
	public static class ObjectPoolManager
	{
		public static int initialPoolSize = 10;
		private static Dictionary<Type, IObjectPool> mObjectPoolDict = new Dictionary<Type, IObjectPool>();
		private static Dictionary<Type, Dictionary<int, IObjectArrayPool>> mObjectArrayPoolDict = new Dictionary<Type, Dictionary<int, IObjectArrayPool>>();
		private static List<IObjectPool> mObjectPoolList = new List<IObjectPool>();

		private static System.Text.StringBuilder sb = new System.Text.StringBuilder();

		private static object mLockObject = new object();

		public static IObjectPool CheckObjectPool<T>(Type objectType)
			where T : IObjectPool, new()
		{
			lock (mLockObject)
			{
				IObjectPool objectPool;
				if (!mObjectPoolDict.TryGetValue(objectType, out objectPool))
				{
					objectPool = new T();

					mObjectPoolDict.Add(objectType, objectPool);
					mObjectPoolList.Add(objectPool);

					return objectPool;
				}

				if (objectPool.GetType() != typeof(T))
				{
					Debug.LogError("ObjectPoolManager.CheckObjectPool() error : registered ObjectPool (" +
					                objectPool.GetType() + ") is not type (" + typeof(T) + ") !!!");
				}

				return objectPool;
			}
		}
		
		public static int GetInstancedCount<T>()
			where T : class, new()
		{
			var pool = GetObjectPool<T>(add:false);

			if (pool == null)
				return 0;
			return pool.instancedCount;
		}

		public static int GetTotalObjectCount<T>()
			where T : class, new()
		{
			var pool = GetObjectPool<T>(add:false);

			if (pool == null)
				return 0;
			return pool.totalObjectCount;
		}

		public static IObjectPool GetObjectPool<T>(bool add = true)
			where T : class, new()
		{
			return GetObjectPool(typeof(T), add);
		}

		public static IObjectPool GetObjectPool(Type objectType, bool add = true)
		{
			lock (mLockObject)
			{
				IObjectPool objectPool;
				if (!mObjectPoolDict.TryGetValue(objectType, out objectPool))
				{
					if (!add)
						return null;

					Type poolType = typeof(ObjectPool<>);
					Type genericType = poolType.MakeGenericType(objectType);
					objectPool = (IObjectPool) Activator.CreateInstance(genericType);
					mObjectPoolDict.Add(objectType, objectPool);
					mObjectPoolList.Add(objectPool);
				}

				return objectPool;
			}
		}

		public static IObjectArrayPool GetObjectArrayPool<T>(int length, bool add = true)
		{
			return GetObjectArrayPool(typeof(T), length, add);
		}

		public static IObjectArrayPool GetObjectArrayPool(Type objectType, int length, bool add = true)
		{
			lock (mLockObject)
			{
				Dictionary<int, IObjectArrayPool> poolDict;
				if (!mObjectArrayPoolDict.TryGetValue(objectType, out poolDict))
				{
					if (!add)
						return null;

					poolDict = new Dictionary<int, IObjectArrayPool>();
					mObjectArrayPoolDict.Add(objectType, poolDict);
				}

				IObjectArrayPool arrayPool;
				if (!poolDict.TryGetValue(length, out arrayPool))
				{
					if (!add)
						return null;

					Type poolType = typeof(ObjectArrayPool<>);
					Type genericType = poolType.MakeGenericType(objectType);
					arrayPool = (IObjectArrayPool) Activator.CreateInstance(genericType);
					arrayPool.arrayLength = length;

					poolDict.Add(length, arrayPool);
					mObjectPoolList.Add(arrayPool);
				}

				return arrayPool;
			}
		}

		public static bool AddObjectPool(IObjectPool objectPool)
		{
			lock (mLockObject)
			{
				if (mObjectPoolDict.ContainsKey(objectPool.objectType))
					return false;

				mObjectPoolDict.Add(objectPool.objectType, objectPool);
				mObjectPoolList.Add(objectPool);

				return true;
			}
		}

		public static T New<T>(int maxUseTime = int.MaxValue)
			where T : class, new()
		{
			ObjectPool<T> objectPool = GetObjectPool<T>() as ObjectPool<T>;
			return objectPool.NewT(maxUseTime);
		}

		public static CollectionT NewCollection<CollectionT, ItemT>(int maxUseTime = int.MaxValue)
			where CollectionT : class, ICollection<ItemT>, new()
		{
			ObjectPool<CollectionT> objectPool = GetObjectPool<CollectionT>() as ObjectPool<CollectionT>;
			var collection = objectPool.NewT(maxUseTime);
			collection.Clear();
			return collection;
		}

		public static HashSet<T> NewHashSet<T>(int maxUseTime = int.MaxValue)
		{
			return NewCollection<HashSet<T>, T>(maxUseTime);
		}

		public static List<T> NewList<T>(int maxUseTime = int.MaxValue)
		{
			return NewCollection<List<T>, T>(maxUseTime);
		}

		public static Dictionary<Key, Value> NewDict<Key, Value>(int maxUseTime = int.MaxValue)
		{
			return NewCollection<Dictionary<Key, Value>, KeyValuePair<Key, Value>>(maxUseTime);
		}

		public static T[] NewArray<T>(int length, int maxUseTime = int.MaxValue)
		{
			ObjectArrayPool<T> arrayPool = GetObjectArrayPool<T>(length) as ObjectArrayPool<T>;
			return arrayPool.NewT(maxUseTime);
		}

		public static object New(Type objectType, int maxUseTime = int.MaxValue)
		{
			IObjectPool objectPool = GetObjectPool(objectType);
			return objectPool.New(maxUseTime);
		}

		public static void Return<ItemT>(ICollection<ItemT> collection)
		{
			if (collection == null)
				return;
			if (!collection.IsReadOnly)
				collection.Clear();
			Return((object)collection);
		}

		public static void Return(object obj)
		{
			if (obj == null)
				return;

			IObjectPool objectPool = null;
			Type objectType = obj.GetType();

			if (objectType.IsArray)
				objectPool = GetObjectArrayPool(objectType.GetElementType(), ((Array)obj).Length, add:false);
			else
				objectPool = GetObjectPool(objectType, add:false);

			if (objectPool == null)
				return;

			objectPool.Return(obj);
		}

		public static void ClearUnusedPools()
		{
			for (int i = 0; i < mObjectPoolList.Count; ++i)
			{
				IObjectPool pool = mObjectPoolList[i];
				if (pool == null)
					continue;

				pool.ClearUnusedItems();
			}
		}

		private static int ComparePoolByTotalSize(IObjectPool p1, IObjectPool p2)
		{
			var size = p2.totalSize - p1.totalSize;

			if (size > 0)
            {
                return 1;
            }				
			else if (size < 0)
            {
                return -1;
            }
            
			return 0;
		}
		private static int ComparePoolByInstanceSize(IObjectPool p1, IObjectPool p2)
		{
			var size = p2.instanceSize - p1.instanceSize;

            if (size > 0)
            {
                return 1;
            }
            else if (size < 0)
            {
                return -1;
            }

            return 0;
        }
		private static int ComparePoolByTotalCount(IObjectPool p1, IObjectPool p2)
		{
			return p2.totalObjectCount - p1.totalObjectCount;
		}
		private static int ComparePoolByInstanceCount(IObjectPool p1, IObjectPool p2)
		{
			return p2.instancedCount - p1.instancedCount;
		}		

	}
}