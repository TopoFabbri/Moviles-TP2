using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    internal sealed class GoPool<TResetable> where TResetable : MonoBehaviour, IGoResetable, new()
    {
        private readonly Dictionary<int, Stack<TResetable>> pool = new();
        private readonly Dictionary<GameObject, int> prefabsToIds = new();

        public TResetable Get(TResetable prefab, params object[] parameters)
        {
            if (!prefabsToIds.ContainsKey(prefab.gameObject))
            {
                int counter = 0;
                while (prefabsToIds.ContainsValue(counter))
                    counter++;
                
                prefabsToIds.Add(prefab.gameObject, counter);
            }
            
            Stack<TResetable> stack = GetStack(prefabsToIds[prefab.gameObject]);
            
            TResetable value = stack.Count <= 0 ? Object.Instantiate(prefab) : stack.Pop();
            
            value.Assign(parameters);
            value.PrefabId = prefabsToIds[prefab.gameObject];
            return value;
        }

        private Stack<TResetable> GetStack(int prefabId)
        {
            if (!pool.TryGetValue(prefabId, out Stack<TResetable> stack))
                pool.Add(prefabId, stack = new Stack<TResetable>());
            
            return stack;
        }

        public void Release(TResetable obj)
        {
            Stack<TResetable> stack = GetStack(obj.PrefabId);
            
            obj.Restart();
            stack.Push(obj);
        }
    }
}