using UnityEngine;
using System;
using System.Collections.Generic;

namespace Sample
{
	public class ObjectPoolComparer<T> : IEqualityComparer<T>
	where T : class
	{
		public bool Equals(T x, T y)
		{
			return ReferenceEquals(x, y);
		}
		public int GetHashCode(T obj)
		{
			if (obj == null)
            {
                return 0;
            }
				
			return obj.GetHashCode();
		}
	}

	public class ObjectPool<T> : IObjectPool
		where T : class
	{
		protected List<ObjectPoolEntry<T>> mItems = null;
		protected List<int> mIndexesOfReturnedItems = null;
		protected Dictionary<T, int> mItemIndexDict = null;
		protected Action<T> mResetAction = null;
		protected Action<T> mOnetimeInitAction = null;
		protected Action<T> mReturnAction = null;
		protected object mLockObject = null;
		protected string mObjectTypeName = null;
		protected long mTypeSize = 0;

		public virtual Type objectType { get { return typeof(T); } }
		public virtual string objectTypeName
		{
			get
			{
				if (string.IsNullOrEmpty(mObjectTypeName))
                {
                    mObjectTypeName = typeof(T).ToString();
                }
					
				return mObjectTypeName;
			}
		}
		public int totalObjectCount { get { return mItems.Count; } }
		public int instancedCount { get; protected set; }
		public long totalSize { get { return mTypeSize * totalObjectCount; } }
		public long instanceSize { get { return mTypeSize * instancedCount; } }
		public long typeSize { get { return mTypeSize; } }
		public bool isAdminOpened { get; set; }

		public ObjectPool() : this(ObjectPoolManager.initialPoolSize, null, null)
		{
		}

		public ObjectPool(
			int initialBufferSize,
			Action<T> resetAction = null,
			Action<T> onetimeInitAction = null,
			Action<T> returnAction = null)
		{
			mItems = new List<ObjectPoolEntry<T>>(initialBufferSize);
			mIndexesOfReturnedItems = new List<int>(initialBufferSize);
			mItemIndexDict = new Dictionary<T, int>(initialBufferSize, new ObjectPoolComparer<T>());
			mResetAction = resetAction;
			mOnetimeInitAction = onetimeInitAction;
			mReturnAction = returnAction;
			mLockObject = new object();
			mObjectTypeName = null;
			mTypeSize = 0;
		}

		protected virtual void DeleteInstance(T instance)
		{
		}

		protected virtual T NewInstance()
		{
			return System.Activator.CreateInstance<T>();
		}

		public virtual T NewT(int maxUseTime = int.MaxValue)
		{
			return New(maxUseTime) as T;
		}
		public virtual object New(int maxUseTime = int.MaxValue)
		{
            lock (mLockObject)
            {
                T instance;
                instancedCount++;

                int objIdx = -1;
                int currentTickCount = Environment.TickCount;

                if (maxUseTime == int.MaxValue)
                {
                    objIdx = IndexOfUnusedItem();
                }
                else
                {
                    objIdx = IndexOfExpiredItem(currentTickCount);
                }

				if (objIdx >= 0)
				{
					instance = mItems[objIdx].instance;

					mItems[objIdx] = new ObjectPoolEntry<T>(
						instance,
						currentTickCount,
						maxUseTime,
						used: true);

					if (mResetAction != null)
						mResetAction(instance);

					return instance;
				}

				bool calcTypeSize = mTypeSize == 0;
				if (calcTypeSize)
				{
					mTypeSize = GC.GetTotalMemory(forceFullCollection: false);
				}

				instance = NewInstance();

				if (calcTypeSize)
				{
					mTypeSize = GC.GetTotalMemory(forceFullCollection: false) - mTypeSize;
				}

				mItemIndexDict.Add(instance, mItems.Count);
				mItems.Add(new ObjectPoolEntry<T>(
					instance,
					currentTickCount,
					maxUseTime,
					used: true));

				if (mOnetimeInitAction != null)
					mOnetimeInitAction(instance);

				return instance;
			}
		}

		int IndexOfUnusedItem()
		{
			while (mIndexesOfReturnedItems.Count > 0)
			{
				var lastIdx = mIndexesOfReturnedItems.Count-1;
				var itemIdx = mIndexesOfReturnedItems[lastIdx];
				mIndexesOfReturnedItems.RemoveAt(lastIdx);

				if (itemIdx < 0 || mItems.Count <= itemIdx)
                    continue;

				var item = mItems[itemIdx];
				if (item.IsExpired())
				{
					return itemIdx;
				}
			}

			return -1;
		}

		int IndexOfExpiredItem(int currentTickCount)
		{
			for (int i = 0; i < mItems.Count; ++i)
			{
				var item = mItems[i];
				int elapsedTime = ObjectPoolUtil.CalcElapsedTickCount(true, item.creationTime, currentTickCount);

				if (item.IsExpired(elapsedTime))
				{
					return i;
				}
			}

			return -1;
		}

		public void Return(object instance)
		{
			Return(instance as T);
		}

		protected virtual void Return(T instance)
		{
			lock (mLockObject)
			{
				int itemIdx;
				if (!mItemIndexDict.TryGetValue(instance, out itemIdx))
				{
					// Pool에 없는 오브젝트..
					return;
				}

				Return(itemIdx, instance);
			}
		}

		protected virtual void Return(int idx, T instance)
		{
			// 반환되지 않은 리소스인 경우..
			if (mItems[idx].used)
			{
				--instancedCount;
				mItems[idx] = new ObjectPoolEntry<T>(instance, 0, 0, used: false);
				mIndexesOfReturnedItems.Add(idx);

				if (mReturnAction != null)
					mReturnAction(instance);
			}
		}

#if UNITY_EDITOR
		protected bool CheckInstancedCount()
		{
			int usedItemCount = 0;
			for (int i = 0; i < mItems.Count; ++i)
				if (mItems[i].used)
					usedItemCount++;
			Debug.Assert(usedItemCount == instancedCount, "ObjectPool : incorrect instancedCount");

			return usedItemCount == instancedCount;
		}
#endif

		public virtual void ClearUnusedItems()
		{
			lock (mLockObject)
			{
				for (int i = 0; i < mItems.Count; ++i)
				{
					if (!mItems[i].used)
					{
						DeleteInstance(mItems[i].instance);
					}
				}

				if (instancedCount == 0)
				{
					mItems.Clear();
					mIndexesOfReturnedItems.Clear();
					mItemIndexDict.Clear();
				}

//#if UNITY_EDITOR
//				CheckInstancedCount();
//#endif
			}
		}
		
	}

}