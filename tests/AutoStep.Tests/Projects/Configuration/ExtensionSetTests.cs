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
        public void ConfigureServicesInvokesLoadedExtension()
        {
            var order = new List<int>();

            var mockExt1 = new Mock<IProjectExtension>();
            var mockExt2 = new Mock<IProjectExtension>();

            mockExt1.Setup(x => x.ConfigureExecutionServices(It.IsAny<ProjectExtensionConfiguration>(), null!, null!)).Callback(() => order.Add(1));
            mockExt2.Setup(x => x.ConfigureExecutionServices(It.IsAny<ProjectExtensionConfiguration>(), null!, null!)).Callback(() => order.Add(2));

            var extensionSet = new ExtensionSet();
            extensionSet.Add(new ProjectExtensionConfiguration() { Name = "ext1" }, mockExt1.Object); 
            extensionSet.Add(new ProjectExtensionConfiguration() { Name = "ext2" }, mockExt2.Object);

            extensionSet.ConfigureExtensionServices(null!, null!);
            
            order[0].Should().Be(1);
            order[1].Should().Be(2);
        }    
    }
}
