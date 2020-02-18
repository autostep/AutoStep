﻿using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace ExtensionMethodsGenerator
{
    /// <summary>
    /// Generate a CallbackDefinitionExtensions class for all of the callback definition options
    /// in AutoStep callbacks.
    /// </summary>
    class Program
    {
        static void Main()
        {
            var codeCompileUnit = new CodeCompileUnit();

            var myNamespace = new CodeNamespace("AutoStep.Definitions");

            codeCompileUnit.Namespaces.Add(myNamespace);

            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            myNamespace.Imports.Add(new CodeNamespaceImport("System.Threading.Tasks"));
            myNamespace.Imports.Add(new CodeNamespaceImport("AutoStep.Execution.Dependency"));

            var classDecl = new CodeTypeDeclaration("CallbackDefinitionExtensions");

            myNamespace.Types.Add(classDecl);

            // Mark as generated code.
            classDecl.CustomAttributes.Add(new CodeAttributeDeclaration(
                "System.CodeDom.Compiler.GeneratedCode",
                new CodeAttributeArgument(new CodePrimitiveExpression("ExtensionMethodGenerator")),
                new CodeAttributeArgument(new CodePrimitiveExpression("1.0"))));

            classDecl.Attributes = MemberAttributes.Public | MemberAttributes.Static;

            /// Add a summary comment
            AddDocComments(classDecl.Comments,
                "<summary>",
                "Defines extension methods for registering callback variants.",
                "This class is automatically generated - do not modify directly.",
                "</summary>");

            AddDefinedSteps(classDecl, "Given", "StepType.Given");
            AddDefinedSteps(classDecl, "When", "StepType.When");
            AddDefinedSteps(classDecl, "Then", "StepType.Then");

            // Output the code.
            var codeDom = CodeDomProvider.CreateProvider("CSharp");

            var stringWriter = new StringWriter();

            // Create an IndentedTextWriter, constructed with
            // a StreamWriter to the source file.
            var tw = new IndentedTextWriter(stringWriter, "    ");

            // Generate source code using the code generator.
            codeDom.GenerateCodeFromCompileUnit(codeCompileUnit, tw, new CodeGeneratorOptions
            {
                BlankLinesBetweenMembers = true,
                BracingStyle = "C"
            });

            // Add the static class marker, and remove the strange extra blank lines that get inserted.
            var fullContent = stringWriter.ToString();

            fullContent = fullContent.Replace("public class", "public static class");

            // Now we need to go through each line, and only output those that don't result in 
            // duplicated whitespace.
            // Workaround for .NET defect (https://github.com/dotnet/runtime/issues/31614).
            var stringReader = new StringReader(fullContent);
            string line;
            bool lastLineEndsWithParen = false;
            while((line = stringReader.ReadLine()) != null)
            {
                if(string.IsNullOrWhiteSpace(line) && lastLineEndsWithParen)
                {
                    continue;
                }

                lastLineEndsWithParen = line.EndsWith(')');
                Console.Out.WriteLine(line);
            }
        }

        private static void AddDefinedSteps(CodeTypeDeclaration classDecl, string methodName, string stepType)
        {
            // For each step type, we need:
            //  - A method that takes an Action
            //  - A method that takes a Func<ValueTask>
            //  - Multiple methods that take an Action<>, with 1 to 10 typed parameters.
            //  - Multiple methods that take a Func<, ValueTask>, with 1 to 10 typed parameters.

            for(int idx = 0; idx <= 10; idx++)
            {
                AddMethod(classDecl, methodName, stepType, false, idx);
                AddMethod(classDecl, methodName, stepType, true, idx);
            }
        }

        private static void AddMethod(CodeTypeDeclaration classDecl, string methodName, string stepType, bool isValueTask, int typeArgumentCount)
        {
            var typeParameters = new CodeTypeParameter[typeArgumentCount];

            for(int idx = 0; idx < typeArgumentCount; idx++)
            {
                typeParameters[idx] = new CodeTypeParameter($"T{idx + 1}");
            }

            var method = new CodeMemberMethod();
            method.Name = methodName;
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.TypeParameters.AddRange(typeParameters);
            method.ReturnType = new CodeTypeReference("CallbackDefinitionSource");

            // Add the docs.
            AddDocComments(method.Comments,
                "<summary>",
                $"Register a '{methodName}' step definition, with a callback to be invoked when",
                "that step is used in a test.");

            if(typeArgumentCount > 0)
            {
                AddDocComments(method.Comments,
                    "Type parameters for the callback are the type of values to bind step arguments to.",
                    "A type argument of <see cref=\"IServiceScope\" /> will cause the current scope to be injected into the method.");
            }

            AddDocComments(method.Comments, "</summary>");

            for (var typeParamIdx = 0; typeParamIdx < typeParameters.Length; typeParamIdx++)
            {
                var typeParam = typeParameters[typeParamIdx];
                AddDocComments(method.Comments,
                    $"<typeparam name=\"{typeParam.Name}\">Method argument type {typeParamIdx + 1}.</typeparam>");
            }

            AddDocComments(method.Comments,
                "<param name=\"source\">The callback source to add to.</param>",
                "<param name=\"declaration\">The step declaration body.</param>",
                "<param name=\"callback\">The callback to invoke when the step executes.</param>",
                "<returns>The same callback source, to allow continuations.</returns>");

            var extensionPointParameter = new CodeParameterDeclarationExpression("this CallbackDefinitionSource", "source");
            var declarationParameter = new CodeParameterDeclarationExpression(typeof(string), "declaration");

            CodeTypeReference callbackType;

            if (isValueTask)
            {
                callbackType = new CodeTypeReference("Func");
                
                foreach(var tParam in typeParameters)
                {
                    callbackType.TypeArguments.Add(tParam.Name);
                }

                callbackType.TypeArguments.Add("ValueTask");
            }
            else
            {
                callbackType = new CodeTypeReference("Action");

                foreach (var tParam in typeParameters)
                {
                    callbackType.TypeArguments.Add(tParam.Name);
                }
            }

            var callbackParameter = new CodeParameterDeclarationExpression(callbackType, "callback");

            method.Parameters.Add(extensionPointParameter);
            method.Parameters.Add(declarationParameter);
            method.Parameters.Add(callbackParameter);

            var callbackNullCheck = new CodeExpressionStatement(new CodeSnippetExpression("callback = callback.ThrowIfNull(nameof(callback))"));
            var sourceNullCheck = new CodeExpressionStatement(new CodeSnippetExpression("source = source.ThrowIfNull(nameof(source))"));

            method.Statements.Add(callbackNullCheck);
            method.Statements.Add(sourceNullCheck);

            method.Statements.Add(new CodeExpressionStatement(new CodeSnippetExpression($"source.Add(new DelegateBackedStepDefinition(source, callback.Target, callback.Method, {stepType}, declaration))")));

            var returnExpr = new CodeVariableReferenceExpression("source");
            method.Statements.Add(new CodeMethodReturnStatement(returnExpr));

            classDecl.Members.Add(method);
        }

        private static void AddDocComments(CodeCommentStatementCollection commentCollection, params string[] lines)
        {
            foreach(var commentLine in lines)
            {
                commentCollection.Add(new CodeCommentStatement(commentLine, true));
            }
        }
    }
}