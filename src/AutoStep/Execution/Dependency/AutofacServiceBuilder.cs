using System;
using Autofac;
using AutoStep.Execution.Events;

namespace AutoStep.Execution.Dependency
{
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

        public void RegisterPerScopeService<TService>()
        {
            Builder.RegisterType<TService>().InstancePerLifetimeScope();
        }

        public void RegisterPerScopeService<TService, TComponent>()
        {
            Builder.RegisterType<TComponent>().As<TComponent>().InstancePerLifetimeScope();
        }

        public void RegisterPerThreadService<TService>()
        {
            Builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        public void RegisterPerThreadService<TService, TComponent>()
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
