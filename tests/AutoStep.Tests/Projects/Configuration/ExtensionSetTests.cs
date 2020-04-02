using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoStep.Projects.Configuration;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests.Projects.Configuration
{
    public class ExtensionSetTests
    {
        [Fact]
        public async Task ConfigureServicesInvokesLoadedExtension()
        {
            var order = new List<int>();

            var projConfig = new ProjectConfiguration
            {
                Extensions = new Dictionary<string, ProjectExtensionConfiguration>
                {
                    { "ext1", new ProjectExtensionConfiguration() { Name = "ext1" } },
                    { "ext2", new ProjectExtensionConfiguration() { Name = "ext2" } }
                }
            };

            var mockExt1 = new Mock<IProjectExtension>();
            var mockExt2 = new Mock<IProjectExtension>();

            mockExt1.Setup(x => x.ConfigureExecutionServices(It.IsAny<ProjectExtensionConfiguration>(), null!, null!)).Callback(() => order.Add(1));
            mockExt2.Setup(x => x.ConfigureExecutionServices(It.IsAny<ProjectExtensionConfiguration>(), null!, null!)).Callback(() => order.Add(2));

            var pipeline = await ExtensionSet.Create(projConfig, (cfg, cancel) =>
            {
                return Task.FromResult(cfg.Name switch
                {
                    "ext1" => mockExt1.Object,
                    "ext2" => mockExt2.Object,
                    _ => throw new Exception()
                });
            }, CancellationToken.None);

            pipeline.ConfigureExtensionServices(null!, null!);
            
            order[0].Should().Be(1);
            order[1].Should().Be(2);
        }    
    }
}
