using System;
using System.Collections.Concurrent;

namespace Tools.Pool
{
    public class ConcurrentPool
    {
        private readonly ConcurrentDictionary<Type, ConcurrentStack<IResetable>> pool = new();

        public TResetable Get<TResetable>(params object[] parameters) where TResetable : IResetable, new()
        {
            if (!pool.ContainsKey(typeof(TResetable)))
                pool.TryAdd(typeof(TResetable), new ConcurrentStack<IResetable>());

            TResetable value;

            if (pool[typeof(TResetable)].Count > 0)
            {
                pool[typeof(TResetable)].TryPop(out IResetable resetable);
                value = (TResetable)resetable;
            }
            else
            {
                value = new TResetable();
            }

            value.Assign(parameters);
            return value;
        }

        public void Release<TResetable>(TResetable obj) where TResetable : IResetable, new()
        {
            obj.Restart();
            pool[typeof(TResetable)].Push(obj);
        }
    }
}