using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AutoStep.Tests.Configuration
{
    public class ConfigurationExtensionTests
    {
        [Fact]
        public void GetConfiguredRunSectionReturnsNullIfNoRunConfigSet()
        {
            var config = MakeConfig();

            config.GetConfiguredRunSection().Should().BeNull();
        }

        [Fact]
        public void GetConfiguredRunSectionThrowsIfSetRunConfigNotPresent()
        {
            var config = MakeConfig(("runConfig", "custom"));

            config.Invoking(c => c.GetConfiguredRunSection()).Should().Throw<ProjectConfigurationException>();
        }

        [Fact]
        public void GetConfiguredRunSectionReturnsRunSection()
        {
            var config = MakeConfig(
                ("runConfig", "custom"), 
                ("runConfigs:custom:option", "1"));

            var section = config.GetConfiguredRunSection();

            section.Should().NotBeNull();

            section.GetValue<string>("option").Should().Be("1");
        }

        [Fact]
        public void GetRunValueReturnsGlobalValue()
        {
            var config = MakeConfig(("option", "1"));

            config.GetRunValue("option", "2").Should().Be("1");
        }

        [Fact]
        public void GetRunValueRespectsDefault()
        {
            var config = MakeConfig();

            config.GetRunValue("option", "2").Should().Be("2");
        }

        [Fact]
        public void GetRunValueUsesRunSection()
        {
            var config = MakeConfig(
                ("option", "0"),
                ("runConfig", "custom"),
                ("runConfigs:custom:option", "1"));

            config.GetRunValue("option", "2").Should().Be("1");
        }

        [Fact]
        public void GetRunSectionReturnsGlobalSection()
        {
            var config = MakeConfig(
                ("section:option", "0"),
                ("runConfigs:custom:section:option", "1"));

            var section = config.GetRunSection("section");
            section.Exists().Should().BeTrue();

            section.GetValue("option", "2").Should().Be("0");
        }

        [Fact]
        public void GetRunSectionReturnsRunSection()
        {
            var config = MakeConfig(
                ("section:option", "0"),
                ("runConfig", "custom"),
                ("runConfigs:custom:section:option", "1"));

            var section = config.GetRunSection("section");
            section.Exists().Should().BeTrue();

            section.GetValue("option", "2").Should().Be("1");
        }

        private IConfiguration MakeConfig(params (string key, string value)[] pairs)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(pairs.Select(x => new KeyValuePair<string, string>(x.key, x.value)));
            return configBuilder.Build();
        }
    }
}
