using System;
using Autofac;
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
        }

        /// <summary>
        /// Configures the service builder to provide loggers as injected services.
        /// </summary>
        /// <param name="logFactory">The log factory to get loggers from.</param>
        public void ConfigureLogging(ILoggerFactory logFactory)
        {
            builder.RegisterInstance(logFactory);
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
        }

        /// <inheritdoc/>
        public void RegisterPerResolveService<TService>()
            where TService : class
        {
            builder.RegisterType<TService>().InstancePerDependency();
        }

        /// <inheritdoc/>
        public void RegisterPerResolveService<TService, TComponent>()
            where TService : class
            where TComponent : TService
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerDependency();
        }

        /// <inheritdoc/>
        public void RegisterPerResolveService(Type consumer)
        {
            builder.RegisterType(consumer).InstancePerDependency();
        }

        /// <inheritdoc/>
        public void RegisterInstance<TService>(TService instance)
            where TService : class
        {
            builder.RegisterInstance(instance).ExternallyOwned();
        }

        /// <inheritdoc/>
        public void RegisterSingleton<TService>()
            where TService : class
        {
            builder.RegisterType<TService>().AsSelf().SingleInstance();
        }

        /// <inheritdoc/>
        public void RegisterSingleton<TService, TComponent>()
            where TService : class
            where TComponent : TService
        {
            builder.RegisterType<TComponent>().As<TService>().SingleInstance();
        }

        /// <inheritdoc/>
        public void RegisterPerFeatureService<TService, TComponent>()
            where TService : class
            where TComponent : TService
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        /// <inheritdoc/>
        public void RegisterPerFeatureService<TService>()
            where TService : class
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        /// <inheritdoc/>
        public void RegisterPerScenarioService<TService, TComponent>()
            where TService : class
            where TComponent : TService
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        /// <inheritdoc/>
        public void RegisterPerScenarioService<TService>()
            where TService : class
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        /// <inheritdoc/>
        public void RegisterPerStepService<TService>()
            where TService : class
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.StepTag);
        }

        /// <inheritdoc/>
        public void RegisterPerStepService<TService, TComponent>()
            where TService : class
            where TComponent : TService
        {
            builder.RegisterType<TComponent>().As<TComponent>().InstancePerMatchingLifetimeScope(ScopeTags.StepTag);
        }

        /// <inheritdoc/>
        public void RegisterPerThreadService<TService>()
            where TService : class
        {
            builder.RegisterType<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        /// <inheritdoc/>
        public void RegisterPerThreadService<TService, TComponent>()
            where TService : class
            where TComponent : TService
        {
            builder.RegisterType<TComponent>().As<TService>().InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        /// <inheritdoc/>
        public IAutoStepServiceScope BuildRootScope()
        {
            return new AutofacServiceScope(ScopeTags.Root, newScope => builder.Build());
        }
    }
}
