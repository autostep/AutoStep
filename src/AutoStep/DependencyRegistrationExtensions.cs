using System;
using Autofac.Builder;
using AutoStep.Execution.Dependency;

namespace AutoStep
{
    /// <summary>
    /// Extension methods that introduce AutoStep lifetime concepts for registered services.
    /// </summary>
    public static class DependencyRegistrationExtensions
    {
        /// <summary>
        /// The registration should have a single instance per AutoStep test thread, shared with all Features run on that thread.
        /// </summary>
        /// <typeparam name="TLimit">The service limit type.</typeparam>
        /// <typeparam name="TActivatorData">Registration activator data.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The Autofac registration builder.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
               InstancePerThread<TLimit, TActivatorData, TStyle>(
                   this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.InstancePerMatchingLifetimeScope(ScopeTags.ThreadTag);
        }

        /// <summary>
        /// The registration should have a single instance per AutoStep Feature, shared with all Scenarios in the Feature.
        /// </summary>
        /// <typeparam name="TLimit">The service limit type.</typeparam>
        /// <typeparam name="TActivatorData">Registration activator data.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The Autofac registration builder.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
               InstancePerFeature<TLimit, TActivatorData, TStyle>(
                   this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.InstancePerMatchingLifetimeScope(ScopeTags.FeatureTag);
        }

        /// <summary>
        /// The registration should have a single instance per AutoStep Scenario invocation, shared with all Steps in the Scenario. For Scenario Outlines,
        /// there will be a single instance for each row in the Examples.
        /// </summary>
        /// <typeparam name="TLimit">The service limit type.</typeparam>
        /// <typeparam name="TActivatorData">Registration activator data.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The Autofac registration builder.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
               InstancePerScenario<TLimit, TActivatorData, TStyle>(
                   this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.InstancePerMatchingLifetimeScope(ScopeTags.ScenarioTag);
        }

        /// <summary>
        /// The registration should have a single instance per AutoStep Step, shared with all nested steps and any interaction methods.
        /// </summary>
        /// <typeparam name="TLimit">The service limit type.</typeparam>
        /// <typeparam name="TActivatorData">Registration activator data.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The Autofac registration builder.</param>
        /// <returns>The registration builder.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
               InstancePerStep<TLimit, TActivatorData, TStyle>(
                   this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.InstancePerMatchingLifetimeScope(ScopeTags.StepTag);
        }
    }
}
