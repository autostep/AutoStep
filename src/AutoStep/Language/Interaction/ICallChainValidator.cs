using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    internal interface ICallChainValidator
    {
        void ValidateCallChain(string? sourceFileName, IMethodCallSource definition, MethodTable methodTable, InteractionConstantSet constants, bool requireMethodDefinitions, List<CompilerMessage> messages);
    }
}