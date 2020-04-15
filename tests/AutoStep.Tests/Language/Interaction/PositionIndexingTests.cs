using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Elements.Parts;
using AutoStep.Elements.StepTokens;
using AutoStep.Language;
using AutoStep.Language.Position;
using AutoStep.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction
{
    public class PositionIndexingTests : InteractionsCompilerTestBase
    {
        public PositionIndexingTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }
        
        [Fact]
        public async Task TraitDeclarationComponents()
        {
            const string TestFile =
            @"
              Trait: clickable + named
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            // Trait name header.
            var pos = positions.Lookup(2, 17);
            pos.CurrentScope.Should().BeOfType<TraitDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.EntryMarker);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);

            pos = positions.Lookup(2, 26);
            pos.CurrentScope.Should().BeOfType<TraitDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);
            pos.Token!.AttachedElement.Should().BeOfType<NameRefElement>();

            pos = positions.Lookup(2, 36);
            pos.CurrentScope.Should().BeOfType<TraitDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);
            pos.Token!.AttachedElement.Should().BeOfType<NameRefElement>();
        }

        [Fact]
        public async Task ComponentDeclarationComponents()
        {
            const string TestFile =
            @"
              Component: button
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            var pos = positions.Lookup(2, 17);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.EntryMarker);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionComponent);

            pos = positions.Lookup(2, 28);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionComponent);
        }

        [Fact]
        public async Task ComponentName()
        {
            const string TestFile =
            @"
              Component: button
                
                name: 'a button'
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            var pos = positions.Lookup(4, 19);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionPropertyName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionName);

            pos = positions.Lookup(4, 26);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionString);
        }

        [Fact]
        public async Task ComponentInherits()
        {
            const string TestFile =
            @"
              Component: button
                
                inherits: field
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            var pos = positions.Lookup(4, 19);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionPropertyName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionInherits);

            pos = positions.Lookup(4, 29);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionString);
            pos.Token.AttachedElement.Should().BeOfType<NameRefElement>();
        }

        [Fact]
        public async Task ComponentTraits()
        {
            const string TestFile =
            @"
              Component: button
                
                traits: named, clickable
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            var pos = positions.Lookup(4, 19);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionPropertyName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);

            pos = positions.Lookup(4, 29);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);
            pos.Token.AttachedElement.Should().BeOfType<NameRefElement>();

            pos = positions.Lookup(4, 36);
            pos.CurrentScope.Should().BeOfType<ComponentDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionTrait);
            pos.Token.AttachedElement.Should().BeOfType<NameRefElement>();
        }

        [Fact]
        public async Task MethodDeclaration()
        {
            const string TestFile =
            @"
              Component: button

                method(arg1, arg2): needs-defining
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            var pos = positions.Lookup(4, 20);
            pos.CurrentScope.Should().BeOfType<MethodDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionMethod);

            pos = positions.Lookup(4, 25);
            pos.CurrentScope.Should().BeOfType<MethodDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionArguments);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.Declaration);
            pos.Token.AttachedElement.Should().BeOfType<MethodDefinitionArgumentElement>();

            pos = positions.Lookup(4, 31);
            pos.CurrentScope.Should().BeOfType<MethodDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionArguments);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.Declaration);
            pos.Token.AttachedElement.Should().BeOfType<MethodDefinitionArgumentElement>();

            pos = positions.Lookup(4, 44);
            pos.CurrentScope.Should().BeOfType<MethodDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionNeedsDefining);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.None);
        }

        [Fact]
        public async Task MethodCalls()
        {
            const string TestFile =
            @"
              Component: button

                method(arg1): call1(arg1) 
                              -> call2('value<arg1>yep', 1)
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            // Trait name header.
            var pos = positions.Lookup(4, 33);
            pos.CurrentScope.Should().BeOfType<MethodCallElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.InteractionMethod);            

            pos = positions.Lookup(4, 38);
            pos.CurrentScope.Should().BeOfType<MethodCallElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionArguments);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionVariable);
            pos.Token.AttachedElement.Should().BeOfType<VariableRefMethodArgumentElement>();

            pos = positions.Lookup(5, 36);
            pos.CurrentScope.Should().BeOfType<MethodCallElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionName);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionMethod);

            pos = positions.Lookup(5, 42);
            pos.CurrentScope.Should().BeOfType<StringMethodArgumentElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionString);
            pos.Token.AttachedElement.Should().BeOfType<TextToken>();            

            pos = positions.Lookup(5, 49);
            pos.CurrentScope.Should().BeOfType<StringMethodArgumentElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionArguments);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionVariable);
            pos.Token.AttachedElement.Should().BeOfType<VariableToken>();

            pos = positions.Lookup(5, 58);
            pos.CurrentScope.Should().BeOfType<MethodCallElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.InteractionArguments);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionLiteral);
            pos.Token.AttachedElement.Should().BeOfType<IntMethodArgumentElement>();
        }

        [Fact]
        public async Task NextMethodCall()
        {
            const string TestFile =
            @"
              Component: button

                method(arg1): call1(arg1) 
                              -> 

            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            // Trait name header.
            var pos = positions.Lookup(5, 34);
            pos.CurrentScope.Should().BeOfType<MethodDefinitionElement>();
            pos.Token.Should().BeNull();
            pos.ClosestPrecedingTokenIndex.Should().Be(0);
            var precedingToken = pos.LineTokens[pos.ClosestPrecedingTokenIndex!.Value];
            precedingToken.Category.Should().Be(LineTokenCategory.InteractionSeparator);
            precedingToken.SubCategory.Should().Be(LineTokenSubCategory.InteractionCallSeparator);
        }

        [Fact]
        public async Task StepDefinition()
        {
            const string TestFile =
            @"
              Trait: clickable

                Step: Given I have {value} in $component$
                    call1(value)
            ";

            var positions = await CompileAndGetPositionIndex(TestFile);

            // Trait name header.
            var pos = positions.Lookup(4, 20);
            pos.CurrentScope.Should().BeOfType<InteractionStepDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.EntryMarker);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.StepDefine);

            pos = positions.Lookup(4, 26);
            pos.CurrentScope.Should().BeOfType<InteractionStepDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.StepTypeKeyword);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.Given);

            pos = positions.Lookup(4, 33);
            pos.CurrentScope.Should().BeOfType<InteractionStepDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.StepText);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.Declaration);
            pos.Token.AttachedElement.Should().BeOfType<WordDefinitionPart>();

            pos = positions.Lookup(4, 37);
            pos.CurrentScope.Should().BeOfType<InteractionStepDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.BoundArgument);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.Declaration);
            pos.Token.AttachedElement.Should().BeOfType<ArgumentPart>();

            pos = positions.Lookup(4, 54);
            pos.CurrentScope.Should().BeOfType<InteractionStepDefinitionElement>();
            pos.Token!.Category.Should().Be(LineTokenCategory.Placeholder);
            pos.Token!.SubCategory.Should().Be(LineTokenSubCategory.InteractionComponentPlaceholder);
            pos.Token.AttachedElement.Should().BeOfType<PlaceholderMatchPart>();

            pos = positions.Lookup(5, 23);
            pos.CurrentScope.Should().BeOfType<MethodCallElement>();
            pos.Scopes[1].Should().BeOfType<InteractionStepDefinitionElement>();
        }
    }
}
