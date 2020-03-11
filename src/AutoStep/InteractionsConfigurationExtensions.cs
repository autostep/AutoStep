using System.Reflection;
using AutoStep.Definitions.Interaction;
using AutoStep.Language.Interaction;

namespace AutoStep
{
    /// <summary>
    /// Extension methods to assist with configuration the interaction system.
    /// </summary>
    public static class InteractionsConfigurationExtensions
    {
        /// <summary>
        /// Add or replace a method in the configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="method">The method to add or override.</param>
        public static void AddOrReplaceMethod(this IInteractionsConfiguration config, InteractionMethod method)
        {
            if (config is null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            if (method is null)
            {
                throw new System.ArgumentNullException(nameof(method));
            }

            config.RootMethodTable.Set(method);
        }

        /// <summary>
        /// Scans a class for methods that are decorated with <see cref="InteractionMethodAttribute"/>
        /// and adds them as interaction methods to the method table.
        /// </summary>
        /// <typeparam name="TMethodsClass">The class to scan.</typeparam>
        /// <param name="config">The configuration.</param>
        public static void AddMethods<TMethodsClass>(this IInteractionsConfiguration config)
            where TMethodsClass : class
        {
            if (config is null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            var methods = typeof(TMethodsClass).GetMethods();

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<InteractionMethodAttribute>();

                if (attr is object)
                {
                    config.RootMethodTable.Set(new ClassBackedInteractionMethod(attr.Name, typeof(TMethodsClass), method));
                }
            }
        }
    }
}
