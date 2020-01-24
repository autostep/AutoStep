using System;
using Autofac;
using AutoStep.Execution.Events;
using AutoStep.Logging;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Dependency
{
    /// <summary>
    /// Provides a wrapper around the Autofac Container Builder, used to provide an <see cref="IServicesBuilder"/>.
    /// </summary>
    internal class AutofacServiceBuilder : IServicesBuilder
    {
        private readonly ContainerBuilder builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacServiceBuilder"/> class.
        /// </summary>
        public AutofacServiceBuilder()
        {
            builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(LoggerWrapper<>)).As(typeof(ILogger<>));
        }

        /// <inheritdoc/>
        public void RegisterPerResolveService<TService>()
        {
            builder.RegisterType<TService>().InstancePerDependency();
        }

        /// <inheritdoc/>
        public void RegisterPerResolveService(Type consumer)
        {
            builder.RegisterType(consumer).InstancePerDependency();
        }

        /// <inheritdoc/>
        public void RegisterSingleInstance<TService>(TService instance)
            where TService : class
        {
            builder.RegisterInstance(instance).ExternallyOwned();
        }

        /// <inheritdoc/>
        public void RegisterPerFeatureService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        /// <inheritdoc/>
        public void RegisterPerFeatureService<TService>()
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        /// <inheritdoc/>
        public void RegisterPerScenarioService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        /// <inheritdoc/>
        public void RegisterPerScenarioService<TService>()
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        /// <inheritdoc/>
        public void RegisterPerScopeService<TService>()
        {
            builder.RegisterType<TService>().InstancePerLifetimeScope();
        }

        /// <inheritdoc/>
        public void RegisterPerScopeService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TComponent>().InstancePerLifetimeScope();
        }

        /// <inheritdoc/>
        public void RegisterPerThreadService<TService>()
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        /// <inheritdoc/>
        public void RegisterPerThreadService<TService, TComponent>()
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        /// <inheritdoc/>
        public void RegisterEventHandler(IEventHandler eventHandler)
        {
            // Only a single instance of event handlers.
            builder.RegisterInstance(eventHandler);
        }

        /// <inheritdoc/>
        public IServiceScope BuildRootScope()
        {
            return new AutofacServiceScope(ScopeTags.Root, builder.Build());
        }
    }
}
