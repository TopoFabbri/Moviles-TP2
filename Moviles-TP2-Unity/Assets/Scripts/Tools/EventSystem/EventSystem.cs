using System;
using System.Collections.Generic;
using Tools.Pool;
using Tools.ServiceProvider;

namespace Tools.EventSystem
{
    public class EventSystem : IService
    {
        private readonly Dictionary<Type, List<Delegate>> eventListeners = new();
        private ConcurrentPool Pool { get; } = new();
        
        public void Raise<TEvent>(params object[] parameters) where TEvent : IEvent, new()
        {
            TEvent eventInstance = Pool.Get<TEvent>(parameters);
            eventInstance.Assign(parameters);
            
            if (eventListeners.TryGetValue(eventInstance.GetType(), out List<Delegate> delegates))
            {
                foreach (Delegate del in delegates)
                    ((Action<TEvent>)del)?.Invoke(eventInstance);
            }
                
            Pool.Release(eventInstance);
        }

        public void Subscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            if (!eventListeners.ContainsKey(typeof(TEvent)))
                eventListeners.Add(typeof(TEvent), new List<Delegate>());
            
            eventListeners[typeof(TEvent)].Add(action);
        }
        
        public void Unsubscribe<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            if (eventListeners.TryGetValue(typeof(TEvent), out List<Delegate> delegates))
                delegates.Remove(action);
        }
    }
}