using UnityEngine;
using System;
using System.Collections.Generic;

namespace BuildingSystem.Core.Events
{
    [CreateAssetMenu(fileName = "BuildingEventChannel", menuName = "Building System/Event Channel")]
    public class BuildingEventChannel : ScriptableObject
    {
        private readonly Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler) where T : IBuildingEvent
        {
            var eventType = typeof(T);
            if (!eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = new List<Delegate>();
            }
            eventHandlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IBuildingEvent
        {
            var eventType = typeof(T);
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType].Remove(handler);
            }
        }

        public void Publish<T>(T buildingEvent) where T : IBuildingEvent
        {
            var eventType = typeof(T);
            if (eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in eventHandlers[eventType])
                {
                    (handler as Action<T>)?.Invoke(buildingEvent);
                }
            }
        }

        private void OnDisable()
        {
            eventHandlers.Clear();
        }
    }

    // Base interfaces and events
    public interface IBuildingEvent { }
}