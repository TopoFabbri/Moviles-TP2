using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tools.ServiceProvider
{
    /// <summary>
    /// Provides a very simple, global service locator for registering and resolving
    /// application-wide services that implement <see cref="IService"/>.
    /// </summary>
    public static class ServiceProvider
    {
        private static readonly ConcurrentDictionary<Type, object> Services = new();

        /// <summary>
        /// Resolves an instance of the requested service type.
        /// </summary>
        /// <typeparam name="T">The service type to resolve. Must implement <see cref="IService"/>.</typeparam>
        /// <returns>
        /// An existing instance if it was already registered, an instance created via a registered
        /// factory, or a new instance created via a parameterless constructor.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no registration or factory exists for an interface or abstract type,
        /// or when an instance cannot be created via <see cref="Activator.CreateInstance(Type,bool)"/>.
        /// </exception>
        public static T GetService<T>() where T : class, IService
        {
            if (Services.TryGetValue(typeof(T), out object? existing))
                return (T)existing;

            Type type = typeof(T);
            if (type.IsInterface || type.IsAbstract)
                throw new InvalidOperationException($"No registration/factory for {type.FullName}. Register it before resolving.");

            T instance = (T)(Activator.CreateInstance(type, nonPublic: true) ??
                             throw new InvalidOperationException($"Failed to create {type.FullName}. Ensure it has a parameterless ctor (it can be non-public)."));

            return (T)Services.GetOrAdd(type, instance);
        }

        /// <summary>
        /// Attempts to retrieve a registered service instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The service type to retrieve. Must implement <see cref="IService"/>.</typeparam>
        /// <param name="service">
        /// When this method returns, contains the resolved service instance if found; otherwise <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a service of type <typeparamref name="T"/> was found; otherwise <c>false</c>.
        /// </returns>
        public static bool TryGetService<T>(out T? service) where T : class, IService
        {
            if (Services.TryGetValue(typeof(T), out object? obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Attempts to add a concrete service instance to the provider.
        /// </summary>
        /// <typeparam name="T">The service type. Must implement <see cref="IService"/>.</typeparam>
        /// <param name="service">The service instance to register.</param>
        /// <returns>
        /// <c>true</c> if the service was added; <c>false</c> if a service of this type is already registered.
        /// </returns>
        public static bool TryAddService<T>(T service) where T : class, IService => Services.TryAdd(typeof(T), service);

        /// <summary>
        /// Replaces the currently registered service instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The service type. Must implement <see cref="IService"/>.</typeparam>
        /// <param name="service">The new service instance to register.</param>
        /// <param name="disposeOld">
        /// If <c>true</c>, disposes the previously registered instance (if any) when it implements <see cref="IDisposable"/>.
        /// </param>
        public static void ReplaceService<T>(T service, bool disposeOld = false) where T : class, IService
        {
            Type key = typeof(T);
            if (disposeOld && Services.TryGetValue(key, out object? old) && old is IDisposable d) d.Dispose();
            Services[key] = service;
        }

        /// <summary>
        /// Removes the registered service instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The service type. Must implement <see cref="IService"/>.</typeparam>
        /// <param name="dispose">
        /// If <c>true</c>, disposes the removed instance when it implements <see cref="IDisposable"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a service of type <typeparamref name="T"/> was removed; otherwise <c>false</c>.
        /// </returns>
        public static bool RemoveService<T>(bool dispose = false) where T : class, IService
        {
            Type key = typeof(T);
            if (Services.TryRemove(key, out object? old))
            {
                if (dispose && old is IDisposable d) d.Dispose();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all registered services and factories from the provider.
        /// </summary>
        /// <param name="dispose">
        /// If <c>true</c>, disposes each removed service instance that implements <see cref="IDisposable"/>.
        /// </param>
        public static void Clear(bool dispose = false)
        {
            foreach (KeyValuePair<Type, object> kv in Services)
            {
                if (Services.TryRemove(kv.Key, out object? old) && dispose && old is IDisposable d)
                    d.Dispose();
            }
        }
    }

    /// <summary>
    /// Marker interface for types that can be managed by <see cref="ServiceProvider"/>.
    /// Implement this interface to make a type eligible for registration and resolution.
    /// </summary>
    public interface IService
    {
    }
}