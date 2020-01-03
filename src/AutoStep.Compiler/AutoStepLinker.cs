using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler.Parser;
using AutoStep.Core;
using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Links compiled autostep content.
    /// </summary>
    /// <remarks>
    /// Linker can hold state, to be able to know what has changed.
    /// The output of the compiler can be fed repeatedly into the linker, to update the references.
    /// </remarks>
    public class AutoStepLinker
    {
        private IAutoStepCompiler compiler;

        public AutoStepLinker(IAutoStepCompiler compiler)
        {
            this.compiler = compiler;
        }

        public StepDefinitionFromBodyResult GetStepDefinitionElementFromStatementBody(StepType stepType, string statementBody)
        {
            if (statementBody is null)
            {
                throw new ArgumentNullException(nameof(statementBody));
            }

            // Trim first, we don't want to worry about the whitespace at the end.
            statementBody = statementBody.Trim();

            var errors = new List<CompilerMessage>();
            var success = false;

            // Compile the text, specifying a starting lexical mode of 'statement'.
            var parseContext = compiler.CompileEntryPoint(statementBody, null, p => p.statementBody(), out var tokenStream, out var parserErrors, AutoStepLexer.statement);

            if (parserErrors.Any(x => x.Level == CompilerMessageLevel.Error))
            {
                errors.AddRange(parserErrors);
            }

            // Now we need a visitor.
            var stepReferenceVisitor = new StepReferenceVisitor(null, tokenStream);

            // Construct a 'reference' step.
            var stepReference = stepReferenceVisitor.BuildStep(stepType, null, parseContext);

            StepDefinitionElement? definition = null;

            if (stepReference != null)
            {
                definition = new StepDefinitionElement { SourceLine = 1, SourceColumn = 1 };

                definition.UpdateFromStepReference(stepReference);

                success = true;

                if (definition.Arguments is object)
                {
                    // At this point, we'll validate the provided 'arguments' to the step. All the arguments should just be variable names.
                    foreach (var declaredArgument in stepReference.Arguments)
                    {
                        if (declaredArgument.Type == ArgumentType.Empty)
                        {
                            errors.Add(CompilerMessageFactory.Create(null, declaredArgument, CompilerMessageLevel.Error, CompilerMessageCode.StepVariableNameRequired));
                            success = false;
                        }
                        else if (declaredArgument.Value is null)
                        {
                            // If the value cannot be immediately determined, it means there is some dynamic component (e.g. insertion variables or example inserts).
                            // Everything else is allowed.
                            var argumentName = declaredArgument.RawArgument;

                            if (declaredArgument.Type == ArgumentType.Interpolated)
                            {
                                argumentName = ":" + argumentName;
                            }

                            errors.Add(CompilerMessageFactory.Create(null, declaredArgument, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition, argumentName));
                            success = false;
                        }
                    }
                }
            }

            return new StepDefinitionFromBodyResult(success, errors, definition);
        }

        public void AddStepDefinitionSource(IStepDefinitionSource source)
        {

        }

        public void Link(BuiltFile file)
        {

        }
    }
}
