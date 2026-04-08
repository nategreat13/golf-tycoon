using System;
using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new();

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"ServiceLocator: Overwriting existing service {type.Name}");
            }
            services[type] = service;
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            Debug.LogError($"ServiceLocator: Service {type.Name} not registered");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            services.Remove(typeof(T));
        }

        public static void Clear()
        {
            services.Clear();
        }
    }
}
