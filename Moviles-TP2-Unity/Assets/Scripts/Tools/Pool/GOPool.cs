using System.Collections.Generic;
using UnityEngine;

namespace Tools.Pool
{
    internal sealed class GoPool<TResetable> where TResetable : MonoBehaviour, IResetable, new()
    {
        private readonly Stack<TResetable> pool = new();

        public TResetable Get(TResetable prefab, params object[] parameters)
        {
            TResetable value = pool.Count <= 0 ? Object.Instantiate(prefab) : pool.Pop();
            
            value.Assign(parameters);
            return value;
        }
        
        public void Release(TResetable obj)
        {
            obj.Reset();
            pool.Push(obj);
        }
    }
}