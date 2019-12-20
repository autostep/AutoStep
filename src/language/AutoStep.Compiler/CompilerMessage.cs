using System;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Defines a compiler message.
    /// </summary>
    public class CompilerMessage : IEquatable<CompilerMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerMessage"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="level">The message level.</param>
        /// <param name="code">The name of the code.</param>
        /// <param name="message">The message.</param>
        public CompilerMessage(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, string message)
        {
            SourceName = sourceName;
            Level = level;
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerMessage"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="level">The message level.</param>
        /// <param name="code">The name of the code.</param>
        /// <param name="message">The message.</param>
        /// <param name="startLineNo">The line number of the element that the message applies to.</param>
        /// <param name="startColumn">The column position of the element that the message applies to.</param>
        public CompilerMessage(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, string message, int startLineNo, int startColumn)
            : this(sourceName, level, code, message)
        {
            StartLineNo = startLineNo;
            StartColumn = startColumn;
            EndLineNo = startLineNo;
            EndColumn = startColumn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerMessage"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="level">The message level.</param>
        /// <param name="code">The name of the code.</param>
        /// <param name="message">The message.</param>
        /// <param name="startLineNo">The starting line number of the element that the message applies to.</param>
        /// <param name="startColumn">The starting column position of the element that the message applies to.</param>
        /// <param name="endLineNo">The ending line number of the element that the message applies to.</param>
        /// <param name="endColumn">The ending column position of the element that the message applies to.</param>
        public CompilerMessage(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, string message, int startLineNo, int startColumn, int endLineNo, int endColumn)
            : this(sourceName, level, code, message, startLineNo, startColumn)
        {
            EndLineNo = endLineNo;
            EndColumn = endColumn;
        }

        /// <summary>
        /// Gets the source name of the message.
        /// </summary>
        public string? SourceName { get; }

        /// <summary>
        /// Gets the message level.
        /// </summary>
        public CompilerMessageLevel Level { get; }

        /// <summary>
        /// Gets the message code.
        /// </summary>
        public CompilerMessageCode Code { get; }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the starting line number of the element that the message applies to.
        /// </summary>
        public int StartLineNo { get; }

        /// <summary>
        /// Gets the starting column number of the element that the message applies to.
        /// </summary>
        public int StartColumn { get; }

        /// <summary>
        /// Gets the ending line number of the element that the message applies to.
        /// </summary>
        public int EndLineNo { get; }

        /// <summary>
        /// Gets the end column number of the element that the message applies to.
        /// </summary>
        public int EndColumn { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{SourceName}({StartLineNo},{StartColumn},{EndLineNo},{EndColumn}): {Level} ASC{(int)Code:D5}: {Message}";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as CompilerMessage);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(SourceName, StartLineNo, StartColumn, EndLineNo, EndColumn, Message, Code, Level);
        }
    }
}
