using System;
using System.Collections.Generic;
using AutoStep.Definitions.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Root method table, representing the table in which externally defined methods will be added.
    /// </summary>
    public class RootMethodTable : MethodTable
    {
        private HashSet<Type> serviceProvidingTypes = new HashSet<Type>();

        /// <inheritdoc/>
        public override void SetMethod(InteractionMethod predefinedMethod)
        {
            if (predefinedMethod is ClassBackedInteractionMethod classBacked)
            {
                // This will only add if the type has not already been added.
                serviceProvidingTypes.Add(classBacked.ServiceType);
            }

            base.SetMethod(predefinedMethod);
        }

        /// <summary>
        /// Retrieve all services that provide methods, and should be registered in the execution
        /// container to support DI in those classes.
        /// </summary>
        /// <returns>A set of types.</returns>
        public IEnumerable<Type> GetAllMethodProvidingServices()
        {
            return serviceProvidingTypes;
        }
    }
}
