using System;

namespace AutoStep.Compiler
{
    public class CompilerMessage : IEquatable<CompilerMessage>
    {
        public string File { get; set; }

        public int LineNo { get; set; }

        public int Column { get; set; }

        public string Message { get; set; }

        public CompilerMessageCode Code { get; set; }

        public CompilerMessageLevel Level { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as CompilerMessage);
        }

        public bool Equals(CompilerMessage? other)
        {
            return other != null &&
                   File == other.File &&
                   LineNo == other.LineNo &&
                   Column == other.Column &&
                   Message == other.Message &&
                   Code == other.Code &&
                   Level == other.Level;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(File, LineNo, Column, Message, Code, Level);
        }
    }
}
