using JaszCore.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace JaszCore.Common
{
    public static class ServiceLocator
    {
        private static readonly ConcurrentDictionary<Type, object> singletonRegistry = new();
        private static readonly ConcurrentDictionary<Type, Type> implRegistry = new();

        public static void Register<TInterface, TImplementation>() where TImplementation : TInterface
        {
            var key = typeof(TInterface);
            var impl = typeof(TImplementation);

            if (!key.IsInterface)
                throw new InvalidOperationException($"{key.Name} must be an interface.");

            ForgetKey(key);
            implRegistry[key] = impl;
        }

        public static void Register<TInterface>(TInterface instance)
        {
            var key = typeof(TInterface);

            if (!key.IsInterface)
                throw new InvalidOperationException($"{key.Name} must be an interface.");

            if (instance == null)
                throw new ArgumentNullException(nameof(instance), $"Cannot register null instance for {key.Name}.");

            ForgetKey(key);
            singletonRegistry[key] = instance;
        }

        public static void UnRegister<TInterface>()
        {
            var key = typeof(TInterface);
            ForgetKey(key);
        }

        public static TInterface TryGet<TInterface>()
        {
            var key = typeof(TInterface);
            return singletonRegistry.TryGetValue(key, out var result) ? (TInterface)result : default;
        }

        public static TInterface Get<TInterface>()
        {
            var key = typeof(TInterface);

            if (singletonRegistry.TryGetValue(key, out var instance))
                return (TInterface)instance;

            if (implRegistry.TryGetValue(key, out var implType))
                return CreateAndMaybeCache<TInterface>(key, implType, singleton: true);

            var serviceAttr = key.GetCustomAttributes(typeof(ServiceAttribute), false)
                                 .Cast<ServiceAttribute>()
                                 .FirstOrDefault();

            if (serviceAttr != null)
                return CreateAndMaybeCache<TInterface>(key, serviceAttr.AttributeType, serviceAttr.Singleton);

            throw new InvalidOperationException($"ServiceLocator: No registered or attributed implementation found for {key.Name}.");
        }

        private static void ForgetKey(Type key)
        {
            singletonRegistry.TryRemove(key, out _);
            implRegistry.TryRemove(key, out _);
        }

        private static TInterface CreateAndMaybeCache<TInterface>(Type key, Type implType, bool singleton)
        {
            if (!typeof(TInterface).IsAssignableFrom(implType))
                throw new InvalidOperationException($"{implType.Name} does not implement {key.Name}.");

            var instance = CreateInstance<TInterface>(implType);

            if (singleton)
                singletonRegistry.TryAdd(key, instance);

            return instance;
        }

        private static T CreateInstance<T>(Type implType)
        {
            var constructors = implType.GetConstructors();

            foreach (var ctor in constructors.OrderByDescending(c => c.GetParameters().Length))
            {
                var parameters = ctor.GetParameters();
                var resolvedParams = new object[parameters.Length];
                var canResolve = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        resolvedParams[i] = Get(parameters[i].ParameterType);
                    }
                    catch
                    {
                        canResolve = false;
                        break;
                    }
                }

                if (canResolve)
                    return (T)ctor.Invoke(resolvedParams);
            }

            // Fallback: parameterless constructor
            if (implType.GetConstructor(Type.EmptyTypes) != null)
                return (T)Activator.CreateInstance(implType);

            throw new InvalidOperationException($"Unable to resolve or instantiate {implType.Name}. No suitable constructor found.");
        }

        private static object Get(Type type)
        {
            var method = typeof(ServiceLocator).GetMethod(nameof(Get), BindingFlags.Public | BindingFlags.Static);
            var generic = method.MakeGenericMethod(type);
            return generic.Invoke(null, null);
        }
    }
}