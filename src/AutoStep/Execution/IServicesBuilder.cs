using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Execution
{
    public interface IServicesBuilder
    {
        void RegisterConsumer<TService>();

        void RegisterConsumer(Type consumer);

        void RegisterPerFeatureService<TService, TComponent>();
        void RegisterPerFeatureService<TService>();
        void RegisterPerScenarioService<TService, TComponent>();
        void RegisterPerScenarioService<TService>();
        void RegisterPerStepService<TService>();
        void RegisterPerStepService<TService, TComponent>();
        void RegisterPerThread<TService>();
        void RegisterPerThread<TService, TComponent>();
        void RegisterSingleInstance<TService>(TService instance) where TService : class;

        void RegisterEventHandler(IEventHandler eventHandler);
    }
}
