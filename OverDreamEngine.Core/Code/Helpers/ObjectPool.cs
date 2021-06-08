using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ODEngine.Helpers
{

    public interface IPoolable
    {
        void ResetObject();
    }

    public class ObjectPool<T> : IDisposable where T : IPoolable
    {
        private readonly ConcurrentBag<T> objects;
        private readonly Func<T> objectGenerator;

        public ObjectPool(Func<T> objectGenerator, int startCount = 64)
        {
            this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            objects = new ConcurrentBag<T>();
            for (int i = 0; i < startCount; i++)
            {
                Return(this.objectGenerator());
            }
        }

        public T Get() => objects.TryTake(out T item) ? item : objectGenerator();

        public void Return(T item)
        {
            item.ResetObject();
            objects.Add(item);
        }

        public void Dispose()
        {
            var arr = objects.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].ResetObject();
            }
        }

    }

    namespace Pool
    {

        public class ListPoolable<T> : List<T>, IPoolable
        {
            public readonly static ObjectPool<ListPoolable<T>> lists = new ObjectPool<ListPoolable<T>>(() => new ListPoolable<T>());

            public ListPoolable() : base(64) { }

            public void ResetObject()
            {
                Clear();
            }
        }
    }
}
