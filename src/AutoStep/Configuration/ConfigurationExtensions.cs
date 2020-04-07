using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace AutoStep.Configuration
{
    public static class ConfigurationExtensions
    {
        private static IConfiguration DefaultConfiguration { get; } = CreateDefaultConfiguration();

        public static T GetRunConfigurationOption<T>(this IConfiguration configuration, string key, T defaultValue)
        {
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
                throw new ProjectConfigurationException("Cannot find the default run configuration. Has a 'default' entry been configured in the allRunConfigs section?");
            }
            else
            {
                throw new ProjectConfigurationException("Cannot find the specified run configuration '{0}'. Has it been configured in the allRunConfigs section?".FormatWith(named));
            }
        }
    }
}
