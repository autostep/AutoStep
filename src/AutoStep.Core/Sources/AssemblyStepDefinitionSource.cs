using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AutoStep.Core.Tracing;

namespace AutoStep.Core.Sources
{
    /// <summary>
    /// The assembly step definition source loads steps from .NET assemblies.
    /// </summary>
    public class AssemblyStepDefinitionSource : IStepDefinitionSource
    {
        private readonly Assembly assembly;
        private readonly ITracer tracer;
        private readonly List<StepDefinition> definitions = new List<StepDefinition>();

        public AssemblyStepDefinitionSource(Type type)
        {
        }

        public AssemblyStepDefinitionSource(Assembly assembly, ITracer tracer)
        {
            this.assembly = assembly;
            this.tracer = tracer;
        }

        public string Uid => assembly.Location;

        public string Name => assembly.Location;

        public DateTime GetLastModifyTime()
        {
            // Last modify will be the last write time of the step definitions.
            return new FileInfo(assembly.Location).LastWriteTimeUtc;
        }

        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            // If we've already loaded them, just return the existing set.
            if (definitions.Count == 0)
            {
                // Search for public types that are decorated with a Steps attribute.
                var allPublicTypes = assembly.GetExportedTypes();

                foreach (var type in allPublicTypes)
                {
                    if (type.GetCustomAttribute<StepsAttribute>(true) is object && !type.IsAbstract)
                    {
                        tracer.TraceInfo("Looking in type '{FullName}' for steps", new { type.FullName });

                        // This type may contain steps.
                        foreach (var method in type.GetMethods())
                        {
                            var definition = method.GetCustomAttribute<StepDefinitionAttribute>(true);

                            if (definition is object)
                            {
                                tracer.TraceInfo("Found step method, declared as '{Type} {Declaration}' on '{Name}'", new { definition.Type, definition.Declaration, method.Name });

                                definitions.Add(new BuiltStepDefinition(method, definition));
                            }
                        }
                    }
                }
            }

            return definitions;
        }
    }
}
