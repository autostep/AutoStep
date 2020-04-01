using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.Projects.Configuration
{
    public class ProjectConfiguration
    {
        public const string DefaultTestGlob = "**/*.as";
        public const string DefaultInteractionGlob = "**/*.asi";

        public static readonly ProjectConfiguration Default = new ProjectConfiguration
        {
            Tests = new[] { DefaultTestGlob },
            Interactions = new[] { DefaultInteractionGlob },
        };

        public static async ValueTask<ProjectConfiguration> Load(string filePath, CancellationToken cancelToken)
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return await Load(stream, cancelToken);
        }

        public static async ValueTask<ProjectConfiguration> Load(Stream configurationFileStream, CancellationToken cancelToken)
        {
            // This will thrown on parse errors.
            try
            {
                var doc = await JsonSerializer.DeserializeAsync<ProjectConfiguration>(
                    configurationFileStream,
                    new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                    },
                    cancelToken);

                // Populate default globs.
                if (doc.Tests is null || doc.Tests.Length == 0)
                {
                    doc.Tests = new[] { DefaultTestGlob };
                }

                if (doc.Interactions is null || doc.Interactions.Length == 0)
                {
                    doc.Interactions = new[] { DefaultInteractionGlob };
                }

                return doc;
            }
            catch (JsonException ex)
            {
                // Wrap the JSON Exception in our own loading exception.
                throw new ProjectConfigurationException($"Failed to Load Project Configuration. File Error on line {ex.LineNumber}, {ex.Message}", ex);
            }
        }

        public Dictionary<string, ProjectExtensionConfiguration> Extensions { get; } = new Dictionary<string, ProjectExtensionConfiguration>();

        /// <summary>
        /// Contains the set of test globs.
        /// </summary>
        public string[] Tests { get; set; }

        public string[] Interactions { get; set; }

        public Dictionary<string, ProjectRunConfiguration> Runs { get; } = new Dictionary<string, ProjectRunConfiguration>();

    }
}
