using System.IO;
using System.Text;
using System.Threading;
using AutoStep.Projects.Configuration;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Projects.Configuration
{
    public class ProjectConfigurationTests
    {
        [Fact]
        public void LoadWithMinimalExtensionInfo()
        {
            const string TestJson = "{ \"extensions\": { \"AutoStep.Web\":{} } }";

            var config = LoadConfig(TestJson);

            config.Extensions.Should().HaveCount(1);
            config.Extensions["AutoStep.Web"].Should().NotBeNull();
        }

        private ProjectConfiguration LoadConfig(string content)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return ProjectConfiguration.Load(stream, CancellationToken.None).Result;
        }
    }
}
