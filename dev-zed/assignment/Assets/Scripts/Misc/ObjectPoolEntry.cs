using System;

namespace Sample
{
	public interface IObjectPool 
	{
		Type objectType { get; }
		string objectTypeName { get; }
		int totalObjectCount { get; }
		int instancedCount { get; }
		long totalSize { get; }
		long instanceSize { get; }
		bool isAdminOpened { get; set; }
		long typeSize { get; }

		object New(int maxUseTime = int.MaxValue);
		void Return(object obj);
		void ClearUnusedItems();
	}

	public struct ObjectPoolEntry<T>
	{
		public T instance;
		public int creationTime;
		public int timeLimit;
		public bool used;

		public ObjectPoolEntry(T instance, int creationTime, int timeLimit, bool used)
		{
			this.instance = instance;
			this.creationTime = creationTime;
			this.timeLimit = timeLimit;
			this.used = used;
		}

		public bool IsExpired()
		{
			return !used;
		}
		public bool IsExpired(int elapsedTime)
		{
			return !used || timeLimit <= elapsedTime;
		}
	}
}