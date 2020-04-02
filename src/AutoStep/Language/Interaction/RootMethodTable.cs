using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    public class RootMethodTable : MethodTable
    {
        private HashSet<Type> serviceProvidingTypes = new HashSet<Type>();

        public override void Set(InteractionMethod predefinedMethod)
        {
            if (predefinedMethod is ClassBackedInteractionMethod classBacked)
            {
                // This will only add if the type has not already been added.
                serviceProvidingTypes.Add(classBacked.ServiceType);
            }

            base.Set(predefinedMethod);
        }

        public IEnumerable<Type> GetAllMethodProvidingServices()
        {
            return serviceProvidingTypes;
        }
    }
}
