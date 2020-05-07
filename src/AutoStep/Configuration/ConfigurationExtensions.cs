using System;
using Microsoft.Extensions.Configuration;

namespace AutoStep.Configuration
{
    /// <summary>
    /// Extension methods to help accessing common configuration properties.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets a run configuration option, falling back to a global option, then to a default value if no option has been specified.
        /// </summary>
        /// <typeparam name="T">The type of value to access.</typeparam>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="key">The key to the configuration value (from the run configuration / root), specified with ':' separators.</param>
        /// <param name="defaultValue">The default value, if no value could be found.</param>
        /// <returns>The resolved value.</returns>
        public static T GetRunValue<T>(this IConfiguration configuration, string key, T defaultValue)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var activeRunConfig = configuration.GetConfiguredRunSection();

            if (activeRunConfig is null)
            {
                return configuration.GetValue(key, defaultValue);
            }
            else
            {
                return activeRunConfig.GetValue(key, configuration.GetValue(key, defaultValue));
            }
        }

        /// <summary>
        /// Gets the configuration section for the chosen run configuration, or null if no run configuration has been set.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="key">A key denoting the subsection.</param>
        /// <returns>The run config section.</returns>
        public static IConfigurationSection? GetRunSection(this IConfiguration configuration, string key)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var runSection = GetConfiguredRunSection(configuration);

            if (runSection is null)
            {
                return configuration.GetSection(key);
            }
            else
            {
                return runSection.GetSection(key);
            }
        }

        /// <summary>
        /// Gets the configuration section for the chosen run configuration, or null if no run configuration has been set.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The run config section.</returns>
        public static IConfigurationSection? GetConfiguredRunSection(this IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var named = configuration.GetValue<string?>("runConfig", null);

            if (string.IsNullOrEmpty(named))
            {
                return null;
            }
            else
            {
                var foundSection = configuration.GetSection("runConfigs:" + named);

                if (!foundSection.Exists())
                {
                    throw new ProjectConfigurationException(ConfigurationExtensionsMessages.RunConfigurationNotFound.FormatWith(named));
                }

                return foundSection;
            }
        }
    }
}
