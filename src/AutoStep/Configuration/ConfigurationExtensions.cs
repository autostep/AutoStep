using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace AutoStep.Configuration
{
    /// <summary>
    /// Extension methods to help accessing common configuration properties.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private static IConfiguration DefaultConfiguration { get; } = CreateDefaultConfiguration();

        /// <summary>
        /// Gets a run configuration option, falling back to a global option, then to a default value if no option has been specified.
        /// </summary>
        /// <typeparam name="T">The type of value to access.</typeparam>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="key">The key to the configuration value (from the run configuration / root), specified with ':' separators.</param>
        /// <param name="defaultValue">The default value, if no value could be found.</param>
        /// <returns>The resolved value.</returns>
        public static T GetRunConfigurationOption<T>(this IConfiguration configuration, string key, T defaultValue)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var activeRunConfig = configuration.GetSelectedRunConfiguration();

            // Try the run-specific level, then fall back to the global, and finally the default.
            return activeRunConfig.GetValue(key, configuration.GetValue(key, defaultValue));
        }

        private static IConfiguration CreateDefaultConfiguration()
        {
            var defaultBuilder = new ConfigurationBuilder();

            defaultBuilder.AddInMemoryCollection(ToKeyValuePairs(
                ("allRunConfigs:default:name", "Default")));

            return defaultBuilder.Build();
        }

        private static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(params (string Key, string Value)[] pairs)
        {
            return pairs.Select(p => new KeyValuePair<string, string>(p.Key, p.Value));
        }

        private static IConfigurationSection GetSelectedRunConfiguration(this IConfiguration configuration)
        {
            var named = configuration.GetValue("runconfig", "default");

            var allConfigs = configuration.GetSection("allRunConfigs");

            if (!allConfigs.Exists())
            {
                allConfigs = DefaultConfiguration.GetSection("allRunConfigs");
            }

            var runConfigSection = allConfigs.GetSection(named);

            if (runConfigSection.Exists())
            {
                return runConfigSection;
            }

            if (named == "default")
            {
                throw new ProjectConfigurationException(ConfigurationExtensionsMessages.DefaultConfigurationNotFound);
            }
            else
            {
                throw new ProjectConfigurationException(ConfigurationExtensionsMessages.RunConfigurationNotFound.FormatWith(named));
            }
        }
    }
}
