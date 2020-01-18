using System;
using Autofac;

namespace AutoStep.Execution
{
    internal class ScopeTags
    {
        public const string RunTag = "__asRun";
        public const string ThreadTag = "__asThread";
        public const string FeatureTag = "__asFeature";
        public const string ScenarioTag = "__asScenario";
        public const string StepTag = "__asStep";
    }

    internal class AutofacServiceBuilder : IServicesBuilder
    {

        public AutofacServiceBuilder(ContainerBuilder builder)
        {
            Builder = builder;
        }

        public ContainerBuilder Builder { get; }

        public void RegisterConsumer<TService>()
        {
            Builder.RegisterType<TService>().InstancePerDependency();
        }

        public void RegisterConsumer(Type consumer)
        {
            Builder.RegisterType(consumer).InstancePerDependency();
        }

        public void RegisterSingleInstance<TService>(TService instance)
            where TService : class
        {
            Builder.RegisterInstance(instance);
        }

        public void RegisterPerFeatureService<TService, TComponent>()
        {
            Builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        public void RegisterPerFeatureService<TService>()
        {
            Builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        public void RegisterPerScenarioService<TService, TComponent>()
        {
            Builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        public void RegisterPerScenarioService<TService>()
        {
            Builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        public void RegisterPerStepService<TService>()
        {
            Builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.StepTag);
        }

        public void RegisterPerStepService<TService, TComponent>()
        {
            Builder.RegisterType<TComponent>().As<TComponent>().InstancePerMatchingLifetimeScope(ScopeTags.StepTag);
        }

        public void RegisterPerThread<TService>()
        {
            Builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        public void RegisterPerThread<TService, TComponent>()
        {
            Builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        public void RegisterEventHandler(IEventHandler eventHandler)
        {
            // Only a single instance of event handlers.
            Builder.RegisterInstance(eventHandler);
        }
    }
}
