using System;
using Autofac;
using AutoStep.Execution.Events;
using AutoStep.Tracing;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Dependency
{
    internal class AutofacServiceBuilder : IServicesBuilder
    {
        private readonly ContainerBuilder builder;

        public AutofacServiceBuilder()
        {
            builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(LoggerWrapper<>)).As(typeof(ILogger<>));
        }

        public void RegisterConsumer<TService>()
        {
            builder.RegisterType<TService>().InstancePerDependency();
        }

        public void RegisterConsumer(Type consumer)
        {
            builder.RegisterType(consumer).InstancePerDependency();
        }

        public void RegisterSingleInstance<TService>(TService instance)
            where TService : class
        {
            builder.RegisterInstance(instance);
        }

        public void RegisterPerFeatureService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        public void RegisterPerFeatureService<TService>()
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        public void RegisterPerScenarioService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        public void RegisterPerScenarioService<TService>()
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        public void RegisterPerScopeService<TService>()
        {
            builder.RegisterType<TService>().InstancePerLifetimeScope();
        }

        public void RegisterPerScopeService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TComponent>().InstancePerLifetimeScope();
        }

        public void RegisterPerThreadService<TService>()
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        public void RegisterPerThreadService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        public void RegisterEventHandler(IEventHandler eventHandler)
        {
            // Only a single instance of event handlers.
            builder.RegisterInstance(eventHandler);
        }

        public IServiceScope BuildRootScope()
        {
            return new AutofacServiceScope(ScopeTags.Root, builder.Build());
        }
    }
}
