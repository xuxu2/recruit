using System;

namespace Sample
{
	public interface IObjectArrayPool : IObjectPool
	{
		int arrayLength { get; set; }
	}

	public class ObjectArrayPool<T> : ObjectPool<T[]>, IObjectArrayPool
	{
		public override string objectTypeName
		{
			get
			{
				if (string.IsNullOrEmpty(mObjectTypeName))
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
					sb.AppendFormat("{0}[{1}]", typeof(T), arrayLength);
					mObjectTypeName = sb.ToString();
				}
				return mObjectTypeName;
			}
		}

		public int arrayLength { get; set; }

		public ObjectArrayPool() : base()
		{
		}

		public ObjectArrayPool(
			int initialBufferSize,
			Action<T[]> resetAction = null, 
			Action<T[]> onetimeInitAction = null,
			Action<T[]> returnAction = null) : base(initialBufferSize, resetAction, onetimeInitAction, returnAction)
		{
		}

		protected override T[] NewInstance()
		{
			return new T[arrayLength];
		}

		protected override void DeleteInstance(T[] instance)
		{
			Array.Clear(instance, 0, instance.Length);
		}

		public override object New(int maxUseTime = int.MaxValue)
		{
			var instance = base.New(maxUseTime) as T[];
			Array.Clear(instance, 0, instance.Length);
			return instance;
		}
	}
}