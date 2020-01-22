using System;
using AutoStep.Execution.Events;

namespace AutoStep.Execution.Dependency
{
    public interface IServicesBuilder
    {
        void RegisterConsumer<TService>();

        void RegisterConsumer(Type consumer);

        void RegisterPerFeatureService<TService, TComponent>();

        void RegisterPerFeatureService<TService>();

        void RegisterPerScenarioService<TService, TComponent>();

        void RegisterPerScenarioService<TService>();

        void RegisterPerScopeService<TService>();

        void RegisterPerScopeService<TService, TComponent>();

        void RegisterPerThreadService<TService>();

        void RegisterPerThreadService<TService, TComponent>();

        void RegisterSingleInstance<TService>(TService instance) 
            where TService : class;

        void RegisterEventHandler(IEventHandler eventHandler);
    }
}
