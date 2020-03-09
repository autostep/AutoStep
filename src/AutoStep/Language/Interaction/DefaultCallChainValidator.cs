using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Definitions.Interaction;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    internal class DefaultCallChainValidator : ICallChainValidator
    {
        public void ValidateCallChain(string? sourceFileName, IMethodCallSource definition, MethodTable methodTable, InteractionConstantSet constants, bool requireMethodDefinitions, List<CompilerMessage> messages)
        {
            var variableSet = definition.GetInitialMethodChainVariables();

            foreach (var call in definition.MethodCallChain)
            {
                // Validate the arguments.
                for (var callArgIdx = 0; callArgIdx < call.Arguments.Count; callArgIdx++)
                {
                    var callArg = call.Arguments[callArgIdx];

                    if (callArg is VariableRefMethodArgumentElement varArg)
                    {
                        var msg = variableSet.ValidateVariable(sourceFileName, varArg);

                        if (msg is object)
                        {
                            messages.Add(msg);
                        }
                    }
                    else if (callArg is VariableArrayRefMethodArgument varArrArg)
                    {
                        var msg = variableSet.ValidateVariable(sourceFileName, varArrArg);

                        if (msg is object)
                        {
                            messages.Add(msg);
                        }
                    }
                    else if (callArg is ConstantMethodArgumentElement constantArg)
                    {
                        if (!constants.ContainsConstant(constantArg.ConstantName))
                        {
                            // Not a valid constant.
                            messages.Add(CompilerMessageFactory.Create(sourceFileName, constantArg, CompilerMessageLevel.Error, CompilerMessageCode.InteractionConstantNotDefined, constantArg.ConstantName));
                        }
                    }
                }

                // Look up the method in the method table.
                if (methodTable.TryGetMethod(call.MethodName, out var foundMethod))
                {
                    // Method is in the method table
                    if (foundMethod is FileDefinedInteractionMethod fileMethod)
                    {
                        if (requireMethodDefinitions && fileMethod.NeedsDefining)
                        {
                            // Error.
                            // File-based method needs a definition.
                            messages.Add(CompilerMessageFactory.Create(
                                sourceFileName,
                                call,
                                CompilerMessageLevel.Error,
                                CompilerMessageCode.InteractionMethodRequiredButNotDefined,
                                fileMethod.MethodDefinition.SourceName ?? string.Empty,
                                fileMethod.MethodDefinition.SourceLine));
                        }

                        if (ReferenceEquals(fileMethod.MethodDefinition, definition))
                        {
                            // Circular reference detection.
                            messages.Add(CompilerMessageFactory.Create(
                                sourceFileName,
                                call,
                                CompilerMessageLevel.Error,
                                CompilerMessageCode.InteractionMethodCircularReference));
                        }
                    }

                    // Match the provided arguments against the bound method.
                    if (foundMethod.ArgumentCount != call.Arguments.Count)
                    {
                        // Argument count mismatch.
                        messages.Add(CompilerMessageFactory.Create(
                            sourceFileName,
                            call,
                            CompilerMessageLevel.Error,
                            CompilerMessageCode.InteractionMethodArgumentMismatch,
                            foundMethod.ArgumentCount,
                            call.Arguments.Count));
                    }

                    // Let this method update the set of available variables for the next one.
                    foundMethod.CompilerMethodCall(call.Arguments, variableSet);
                }
                else if (requireMethodDefinitions)
                {
                    // Error.
                    // Method does not exist (and 'needs-defining' is not allowed).
                    messages.Add(CompilerMessageFactory.Create(
                        sourceFileName,
                        call,
                        CompilerMessageLevel.Error,
                        CompilerMessageCode.InteractionMethodNotAvailable,
                        call.MethodName));
                }
                else
                {
                    // Error.
                    // Method does not exist.
                    messages.Add(CompilerMessageFactory.Create(
                        sourceFileName,
                        call,
                        CompilerMessageLevel.Error,
                        CompilerMessageCode.InteractionMethodNotAvailablePermitUndefined,
                        call.MethodName));
                }
            }
        }
    }
}
