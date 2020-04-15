using System;
using System.Reflection;
using System.Text;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Represents an interaction method inside a class.
    /// </summary>
    public class ClassBackedInteractionMethod : DefinedInteractionMethod
    {
        private readonly InteractionMethodAttribute methodAttribute;
        private string? cachedProcessedDocumentation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBackedInteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="classType">The containing type.</param>
        /// <param name="method">The method info.</param>
        /// <param name="methodAttribute">The method declaration attribute attached to the method.</param>
        public ClassBackedInteractionMethod(string name, Type classType, MethodInfo method, InteractionMethodAttribute methodAttribute)
            : base(name, method)
        {
            ServiceType = classType;
            this.methodAttribute = methodAttribute;
        }

        /// <summary>
        /// Gets the service type that backs the method.
        /// </summary>
        public Type ServiceType { get; }

        /// <inheritdoc/>
        public override string? GetDocumentation()
        {
            if (cachedProcessedDocumentation is object)
            {
                return cachedProcessedDocumentation;
            }

            if (methodAttribute.Documentation is null)
            {
                return null;
            }

            return cachedProcessedDocumentation = GetProcessedDocumentationBlock(methodAttribute.Documentation);
        }

        private string? GetProcessedDocumentationBlock(string documentation)
        {
            var text = documentation.AsSpan();

            var builder = new StringBuilder();

            // Go through the content of the doc block.

            // First of all, find the first non-whitespace character.
            var currentPos = 0;
            var knownLineStart = 0;
            var determinedSpacing = false;
            var queuedBlankLines = 0;
            var hitText = false;

            ReadOnlySpan<char> TerminateLine(ReadOnlySpan<char> text)
            {
                if (determinedSpacing)
                {
                    var contentToAppend = text.Slice(0, currentPos);

                    if (contentToAppend.Length == 0 || contentToAppend.IsWhiteSpace())
                    {
                        queuedBlankLines++;
                    }
                    else
                    {
                        if (builder.Length > 0)
                        {
                            builder.AppendLine();
                        }

                        while (queuedBlankLines > 0)
                        {
                            builder.AppendLine();

                            queuedBlankLines--;
                        }

                        // Got the content of the line. Append it up until now.
                        builder.Append(text.Slice(0, currentPos));

                        text = text.Slice(currentPos);
                        hitText = false;
                    }
                }

                return text;
            }

            // Get the whitespace characters.
            while (currentPos < text.Length)
            {
                var currentChar = text[currentPos];

                if (currentChar == '\r' || currentChar == '\n')
                {
                    text = TerminateLine(text);

                    if (text[0] == '\r')
                    {
                        // Move on two characters.
                        text = text.Slice(2);
                    }
                    else
                    {
                        text = text.Slice(1);
                    }

                    currentPos = 0;
                }
                else if (!hitText && (!char.IsWhiteSpace(currentChar) || (determinedSpacing && currentPos == knownLineStart)))
                {
                    hitText = true;

                    if (!determinedSpacing)
                    {
                        knownLineStart = currentPos;
                        determinedSpacing = true;
                    }

                    text = text.Slice(currentPos);
                    currentPos = 0;
                }

                currentPos++;
            }

            TerminateLine(text);

            return builder.ToString();
        }

        /// <inheritdoc/>
        protected override object? GetMethodTarget(IServiceProvider scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            // Resolve an instance of the type from the scope.
            return scope.GetRequiredService(ServiceType);
        }
    }
}
