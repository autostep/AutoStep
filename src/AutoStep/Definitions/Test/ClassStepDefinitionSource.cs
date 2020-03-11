using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AutoStep.Definitions.Test
{
    /// <summary>
    /// Represents a class that can have multiple types added to it manually that each provide binding steps.
    /// </summary>
    public class ClassStepDefinitionSource : ClassBackedStepDefinitionSource
    {
        private readonly List<Type> registeredClasses = new List<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassStepDefinitionSource"/> class.
        /// </summary>
        /// <param name="logFactory">The logger factory.</param>
        public ClassStepDefinitionSource(ILoggerFactory logFactory)
            : base(logFactory)
        {
        }

        /// <summary>
        /// Gets a unique, non-human-readable identifier for the source. Two sources with the same UID cannot share steps. For assembly sources
        /// this is the assembly location.
        /// </summary>
        public override string Uid { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        public override string Name => "Class Definitions";

        /// <summary>
        /// Add a class to the step definition source.
        /// </summary>
        /// <typeparam name="TClassType">The class type.</typeparam>
        public void AddClass<TClassType>()
            where TClassType : class
        {
            var type = typeof(TClassType);

            if (type.IsAbstract)
            {
                throw new ArgumentException(DefinitionsMessages.ClassStepDefinitionSource_ProvidedTypeCannotBeAbstract.FormatWith(type.Name));
            }

            if (registeredClasses.Contains(type))
            {
                throw new InvalidOperationException(DefinitionsMessages.ClassStepDefinitionSource_TypeAlreadyProvided.FormatWith(type.Name));
            }

            registeredClasses.Add(typeof(TClassType));

            ResetDefinitions();
        }

        /// <summary>
        /// Adds each of the registered classes to the source.
        /// </summary>
        protected override void ScanDefinitions()
        {
            foreach (var item in registeredClasses)
            {
                TryAddType(item);
            }
        }
    }
}
