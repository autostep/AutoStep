using System;

namespace AutoStep.Compiler
{
    public class CompilerMessage : IEquatable<CompilerMessage>
    {
        public CompilerMessage(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, string message)
        {
            SourceName = sourceName;
            Level = level;
            Code = code;
            Message = message;
        }

        public CompilerMessage(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, string message, int startLineNo, int startColumn)
            : this(sourceName, level, code, message)
        {
            StartLineNo = startLineNo;
            StartColumn = startColumn;
            EndLineNo = startLineNo;
            EndColumn = startColumn;
        }

        public CompilerMessage(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, string message, int startLineNo, int startColumn, int endLineNo, int endColumn)
            : this(sourceName, level, code, message, startLineNo, startColumn)
        {
            EndLineNo = endLineNo;
            EndColumn = endColumn;
        }

        public string? SourceName { get; }

        public CompilerMessageLevel Level { get; }

        public CompilerMessageCode Code { get; }

        public string Message { get; }

        public int StartLineNo { get; }

        public int StartColumn { get; }

        public int EndLineNo { get; }

        public int EndColumn { get; }

        public override string ToString()
        {
            return $"{SourceName}({StartLineNo},{StartColumn},{EndLineNo},{EndColumn}): {Level} ASC{(int)Code:D5}: {Message}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CompilerMessage);
        }

        public bool Equals(CompilerMessage? other)
        {
            var match = other != null &&
                   SourceName == other.SourceName &&
                   StartLineNo == other.StartLineNo &&
                   StartColumn == other.StartColumn &&
                   EndLineNo == other.EndLineNo &&
                   EndColumn == other.EndColumn &&
                   Message == other.Message &&
                   Code == other.Code &&
                   Level == other.Level;

            return match;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SourceName, StartLineNo, StartColumn, EndLineNo, EndColumn, Message, Code, Level);
        }
    }
}
