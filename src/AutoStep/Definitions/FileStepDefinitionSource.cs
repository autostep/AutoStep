using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler;
using AutoStep.Elements;

namespace AutoStep.Definitions
{
    public interface IUpdatableStepDefinitionSource : IStepDefinitionSource
    {
        DateTime GetLastModifyTime();
    }

    internal class FileStepDefinition : StepDefinition
    {
        public FileStepDefinition(IStepDefinitionSource source, StepDefinitionElement element)
            : base(
                  source,
                  element?.Type ?? throw new ArgumentNullException(nameof(element)),
                  element.Declaration!) // The declaration is validated by FileStepDefinitionSource before instantiating.
        {
            Definition = element;
        }

        public override bool IsSameDefinition(StepDefinition def)
        {
            return Type == def.Type && def.Declaration == def.Declaration;
        }
    }

    internal class FileStepDefinitionSource : IUpdatableStepDefinitionSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileStepDefinitionSource"/> class.
        /// </summary>
        /// <param name="file"></param>
        public FileStepDefinitionSource(ProjectFile file)
        {
            File = file;
        }

        public string Uid => File.Path;

        public string Name => File.Path;

        public ProjectFile File { get; }

        public DateTime GetLastModifyTime()
        {
            // The last modify time of the source is the last compile output, not the actual file!
            return File.LastCompileTime;
        }

        public IEnumerable<StepDefinition> GetStepDefinitions()
        {
            return File.LastCompileResult?.Output?.StepDefinitions.Select(d => new FileStepDefinition(this, d)) 
                   ?? Enumerable.Empty<FileStepDefinition>();
        }
    }
}
