using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Core;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// The FileVisitor is an implementation of an Antlr Visitor that traverses the Antlr parse tree after the parse process has completed,
    /// and builts a <see cref="BuiltFile"/> from that tree.
    /// </summary>
    internal class FileVisitor : ArgumentHandlingVisitor<BuiltFile>
    {
        private readonly StepReferenceVisitor stepVisitor;
        private readonly TableVisitor tableVisitor;

        private IAnnotatableElement? currentAnnotatable;
        private ScenarioElement? currentScenario;

        private List<StepReferenceElement>? currentStepSet = null;
        private StepDefinitionElement? currentStepDefinition;
        private StepReferenceElement? lastStep = null;
        private StepReferenceElement? currentStepSetLastConcrete = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The source name; added to compiler messages.</param>
        /// <param name="tokens">The token stream used by the parse tree.</param>
        public FileVisitor(string? sourceName, ITokenStream tokens)
            : base(sourceName, tokens)
        {
            stepVisitor = new StepReferenceVisitor(sourceName, tokenStream, rewriter, ValidateVariableInsertionName);
            tableVisitor = new TableVisitor(sourceName, tokenStream, rewriter, ValidateVariableInsertionName);
        }

        /// <summary>
        /// Visits the top level Result node. Creates the Result object.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitFile(AutoStepParser.FileContext context)
        {
            Result = new BuiltFile();

            VisitChildren(context);

            return Result;
        }

        /// <summary>
        /// Visits the Feature block (Feature:) and generates a <see cref="FeatureElement"/> that is added to the Result.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitFeatureBlock([NotNull] AutoStepParser.FeatureBlockContext context)
        {
            Debug.Assert(Result is object);

            if (Result.Feature != null)
            {
                // We already have a feature, don't go any deeper, add an error.
                AddMessage(context.featureDefinition().featureTitle(), CompilerMessageLevel.Error, CompilerMessageCode.OnlyOneFeatureAllowed);
                return Result;
            }

            Result.Feature = new FeatureElement();

            currentAnnotatable = Result.Feature;

            VisitChildren(context);

            if (Result.Feature.Scenarios.Count == 0)
            {
                // Warning should be associated to the title.
                AddMessage(context.featureDefinition().featureTitle(), CompilerMessageLevel.Warning, CompilerMessageCode.NoScenarios, Result.Feature.Name);
            }

            return Result;
        }

        /// <summary>
        /// Visits a feature or scenario tag '@tag' and adds it to the current <see cref="IAnnotatableElement"/> feature or scenario object.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitTagAnnotation([NotNull] AutoStepParser.TagAnnotationContext context)
        {
            Debug.Assert(Result is object);

            if (currentAnnotatable == null)
            {
                AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.UnexpectedAnnotation);
                return Result;
            }

            var tag = context.TAG();

            var tagBody = context.TAG().GetText().Substring(1).TrimEnd();

            currentAnnotatable.Annotations.Add(LineInfo(new TagElement { Tag = tagBody }, tag));

            return Result;
        }

        /// <summary>
        /// Visits a feature or scenario option '$tag' and adds it to the current <see cref="IAnnotatableElement"/> feature or scenario object.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitOptionAnnotation([NotNull] AutoStepParser.OptionAnnotationContext context)
        {
            Debug.Assert(Result is object);

            if (currentAnnotatable == null)
            {
                AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.UnexpectedAnnotation);
                return Result;
            }

            var option = context.OPTION();

            var optBody = option.GetText().Substring(1);

            // Trim the body to get rid of trailing whitespace.
            optBody = optBody.TrimEnd();

            // Now split on the first ':'.
            var positionOfColon = optBody.IndexOf(':', StringComparison.CurrentCulture);

            string? setting = null;
            string name = optBody;

            if (positionOfColon != -1)
            {
                var nextChar = positionOfColon + 1;
                name = name.Substring(0, positionOfColon);

                if (nextChar < optBody.Length)
                {
                    // Error, colon has been placed with no other content.
                    setting = optBody.Substring(nextChar).Trim();
                }

                if (string.IsNullOrEmpty(setting))
                {
                    AddMessage(option, CompilerMessageLevel.Error, CompilerMessageCode.OptionWithNoSetting, name);
                    return Result;
                }
            }

            currentAnnotatable.Annotations.Add(LineInfo(
                new OptionElement
                {
                    Name = name,
                    Setting = setting,
                }, option));

            return Result;
        }

        /// <summary>
        /// Visits the feature definition (which defines the name and description).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitFeatureDefinition([NotNull] AutoStepParser.FeatureDefinitionContext context)
        {
            Debug.Assert(Result is object);

            var titleTree = context.featureTitle();

            var featureToken = titleTree.FEATURE();
            var featureKeyWordText = featureToken.GetText();

            // We want the parser to allow case-insensitive keywords through, so we can assert on them
            // here and give more useful errors.
            if (featureKeyWordText != "Feature:")
            {
                AddMessage(featureToken, CompilerMessageLevel.Error, CompilerMessageCode.InvalidFeatureKeyword, featureKeyWordText);
            }

            var title = titleTree.text().GetText();
            var description = ExtractDescription(context.description());

            // Past this point, annotations aren't valid.
            currentAnnotatable = null;

            LineInfo(Result.Feature, titleTree);

            Result.Feature.Name = title;
            Result.Feature.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            return Result;
        }

        /// <summary>
        /// Visits the Background block (which can contain steps).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitBackgroundBlock([NotNull] AutoStepParser.BackgroundBlockContext context)
        {
            Debug.Assert(Result is object);

            var background = new BackgroundElement();

            LineInfo(background, context.BACKGROUND());

            Result.Feature.Background = background;

            currentStepSet = background.Steps;
            currentStepSetLastConcrete = null;

            return base.VisitBackgroundBlock(context);
        }

        public override BuiltFile VisitStepDefinitionBlock([NotNull] AutoStepParser.StepDefinitionBlockContext context)
        {
            Debug.Assert(Result is object);

            var stepDefinition = new StepDefinitionElement();

            var definition = context.stepDefinition();

            var declaration = definition.stepDeclaration();

            LineInfo(stepDefinition, definition.STEP_DEFINE());

            currentAnnotatable = stepDefinition;

            var annotations = context.annotations();
            if (annotations is object)
            {
                Visit(annotations);
            }

            currentAnnotatable = null;

            var description = ExtractDescription(definition.description());
            stepDefinition.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            currentStepDefinition = stepDefinition;

            // Visit the declaration to built the 'signature' of the method.
            Visit(declaration);

            if (stepDefinition.Arguments is object)
            {
                // At this point, we'll validate the provided 'arguments' to the step. All the arguments should just be variable names.
                foreach (var declaredArgument in stepDefinition.Arguments)
                {
                    if (declaredArgument.Type == ArgumentType.Empty)
                    {
                        AddMessage(declaredArgument, CompilerMessageLevel.Error, CompilerMessageCode.StepVariableNameRequired);
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

                        AddMessage(declaredArgument, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition, argumentName);
                    }
                }
            }

            currentStepSet = stepDefinition.Steps;

            Visit(context.stepDefinitionBody());

            Result.AddStepDefinition(stepDefinition);

            currentStepSet = null;
            currentStepDefinition = null;

            return Result;
        }

        /// <summary>
        /// Visits the Scenario block (which is annotable and can contain steps). Adds the scenario to the feature.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitScenarioBlock([NotNull] AutoStepParser.ScenarioBlockContext context)
        {
            Debug.Assert(Result is object);

            ScenarioElement scenario;

            var definition = context.scenarioDefinition();
            var title = definition.scenarioTitle();

            string titleText;

            if (title is AutoStepParser.NormalScenarioTitleContext scenarioTitle)
            {
                var scenarioToken = scenarioTitle.SCENARIO();
                var scenarioKeyWordText = scenarioToken.GetText();

                // We want the parser to allow case-insensitive keywords through, so we can assert on them
                // here and give more useful errors.
                if (scenarioKeyWordText != "Scenario:")
                {
                    AddMessage(scenarioToken, CompilerMessageLevel.Error, CompilerMessageCode.InvalidScenarioKeyword, scenarioKeyWordText);
                    return Result;
                }

                scenario = new ScenarioElement();
                titleText = scenarioTitle.text().GetText();
            }
            else if (title is AutoStepParser.ScenarioOutlineTitleContext scenariOutlineTitle)
            {
                var scenarioOutlineToken = scenariOutlineTitle.SCENARIO_OUTLINE();
                var scenarioOutlineKeyWordText = scenarioOutlineToken.GetText();

                // We want the parser to allow case-insensitive keywords through, so we can assert on them
                // here and give more useful errors.
                if (scenarioOutlineKeyWordText != "Scenario Outline:")
                {
                    AddMessage(scenarioOutlineToken, CompilerMessageLevel.Error, CompilerMessageCode.InvalidScenarioOutlineKeyword, scenarioOutlineKeyWordText);
                    return Result;
                }

                scenario = new ScenarioOutlineElement();
                titleText = scenariOutlineTitle.text().GetText();
            }
            else
            {
                const string assertFailure = "Cannot reach here if the parser rules are valid; parser will not enter the " +
                                             "scenario block if neither SCENARIO or SCENARIO_OUTLINE tokens are present.";
                Debug.Assert(false, assertFailure);
                return Result;
            }

            currentAnnotatable = scenario;

            var annotations = context.annotations();
            if (annotations is object)
            {
                Visit(annotations);
            }

            var description = ExtractDescription(definition.description());

            LineInfo(scenario, title);

            scenario.Name = titleText;
            scenario.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            currentAnnotatable = null;
            currentScenario = scenario;

            // Visit the examples first, it will let us validate any insertion variables
            // when we process the steps.
            Visit(context.examples());

            currentStepSet = scenario.Steps;
            currentStepSetLastConcrete = null;

            Visit(context.scenarioBody());

            currentStepSet = null;
            currentScenario = null;

            Result.Feature.Scenarios.Add(scenario);

            return Result;
        }

        /// <summary>
        /// Visits a Given step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitGiven([NotNull] AutoStepParser.GivenContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.Given, context, context.statementBody());

            return Result;
        }

        /// <summary>
        /// Visits a Then step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitThen([NotNull] AutoStepParser.ThenContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.Then, context, context.statementBody());

            return Result;
        }

        /// <summary>
        /// Visits a When step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitWhen([NotNull] AutoStepParser.WhenContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.When, context, context.statementBody());

            return Result;
        }

        /// <summary>
        /// Visits an And step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitAnd([NotNull] AutoStepParser.AndContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.And, context, context.statementBody());

            return Result;
        }

        /// <summary>
        /// Vists a statement that contains a table.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitStatementWithTable([NotNull] AutoStepParser.StatementWithTableContext context)
        {
            Debug.Assert(Result is object);

            Visit(context.statement());

            var tableBlock = context.tableBlock();

            var table = tableVisitor.BuildTable(tableBlock);

            if (lastStep is object)
            {
                lastStep.Table = table;
            }

            MergeVisitorAndReset(tableVisitor);

            return Result;
        }

        /// <summary>
        /// Visit an examples block.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override BuiltFile VisitExampleBlock([NotNull] AutoStepParser.ExampleBlockContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentScenario is object);

            var outline = currentScenario as ScenarioOutlineElement;
            var exampleTokenText = context.EXAMPLES().GetText();

            if (exampleTokenText != "Examples:")
            {
                AddMessage(context.EXAMPLES(), CompilerMessageLevel.Error, CompilerMessageCode.InvalidExamplesKeyword, exampleTokenText);
                return Result;
            }

            if (outline == null)
            {
                AddMessage(context.EXAMPLES(), CompilerMessageLevel.Error, CompilerMessageCode.NotExpectingExample, currentScenario.Name);
                return Result;
            }

            var example = new ExampleElement();

            currentAnnotatable = example;

            LineInfo(example, context.EXAMPLES());

            Visit(context.annotations());

            currentAnnotatable = null;

            var table = tableVisitor.BuildTable(context.tableBlock());

            example.Table = table;

            MergeVisitorAndReset(tableVisitor);

            outline.AddExample(example);

            return Result;
        }

        private CompilerMessage? ValidateVariableInsertionName(ParserRuleContext context, string insertionName)
        {
            if (currentStepDefinition is object && currentStepSet is object)
            {
                // We are inside a step definition body, so insertions will be references to step parameters defined on the step definition.
                if (!currentStepDefinition.ContainsArgument(insertionName))
                {
                    return CreateMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.StepVariableNotDeclared, insertionName);
                }
            }
            else if (currentScenario is ScenarioOutlineElement outline)
            {
                if (!outline.ContainsVariable(insertionName))
                {
                    // Referencing an undeclared examples variable
                    return CreateMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableNotDeclared, insertionName);
                }
            }
            else if (currentScenario is object)
            {
                // Example variable in a regular scenario
                return CreateMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableInScenario, insertionName);
            }

            return null;
        }

        private void AddStep(StepType type, ParserRuleContext context, AutoStepParser.StatementBodyContext bodyContext)
        {
            Debug.Assert(Result is object);

            if (currentStepSet is null && currentStepDefinition is null)
            {
                AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.StepNotExpected);
                return;
            }

            // All step references are currently added as 'unknown', until they are linked.
            StepType? bindingType = null;

            var step = stepVisitor.BuildStep(type, context, bodyContext);

            MergeVisitorAndReset(stepVisitor);

            if (step is object)
            {
                if (type == StepType.And)
                {
                    if (currentStepDefinition is object)
                    {
                        // We are in the step declaration, which does not permit 'And'.
                        AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.CannotDefineAStepWithAnd);
                    }
                    else if (currentStepSetLastConcrete is null)
                    {
                        AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.AndMustFollowNormalStep);
                    }
                    else
                    {
                        bindingType = currentStepSetLastConcrete.BindingType;
                    }
                }
                else
                {
                    bindingType = type;

                    if (currentStepDefinition is null)
                    {
                        currentStepSetLastConcrete = step;
                    }
                }

                step.BindingType = bindingType;

                if (currentStepSet is object)
                {
                    currentStepSet.Add(step);

                    // Update the global step list.
                    Result.AllStepReferences.AddLast(step);
                }
                else if (currentStepDefinition is object)
                {
                    currentStepDefinition.UpdateFromStepReference(step);
                }

                lastStep = step;
            }
        }

        /// <summary>
        /// Generates the description text from a parsed description context.
        /// Handles indentation of the overall description, and indentation inside it.
        /// </summary>
        /// <param name="descriptionContext">The context.</param>
        /// <returns>The complete description string.</returns>
        private string? ExtractDescription(AutoStepParser.DescriptionContext descriptionContext)
        {
            if (descriptionContext is null)
            {
                return null;
            }

            var lines = descriptionContext.line();

            if (lines.Length == 0)
            {
                return null;
            }

            var descriptionBuilder = new StringBuilder();

            int? whitespaceRemovalCount = null;
            int? firstTextIndex = null;
            int lastTextIndex = 0;

            // First pass to get our whitespace size and last text position.
            for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                var line = lines[lineIdx];
                var text = line.text();

                if (text is object)
                {
                    if (firstTextIndex == null)
                    {
                        firstTextIndex = lineIdx;
                    }

                    lastTextIndex = lineIdx;
                    var whiteSpaceSymbol = line.WS()?.Symbol;
                    var whiteSpaceSize = 0;

                    if (whiteSpaceSymbol is object)
                    {
                        // This is the size of the whitespace.
                        whiteSpaceSize = 1 + whiteSpaceSymbol.StopIndex - whiteSpaceSymbol.StartIndex;
                    }

                    if (whitespaceRemovalCount is null)
                    {
                        // This is the first item of non-whitespace text we have reached.
                        // Base our initial minimum whitespace on this.
                        whitespaceRemovalCount = whiteSpaceSize;
                    }
                    else if (whiteSpaceSize < whitespaceRemovalCount)
                    {
                        // Bring the whitespace in if the amount of whitespace has changed.
                        // We'll ignore whitespace lengths for lines with no text.
                        whitespaceRemovalCount = whiteSpaceSize;
                    }
                }
            }

            // No point rendering anything if there were no text lines.
            if (firstTextIndex is object)
            {
                // Second pass to render our description, only go up to the last text position.
                for (var lineIdx = firstTextIndex.Value; lineIdx <= lastTextIndex; lineIdx++)
                {
                    var line = lines[lineIdx];
                    var text = line.text();

                    if (text is null)
                    {
                        descriptionBuilder.AppendLine();
                    }
                    else
                    {
                        var wsText = line.WS()?.GetText();
                        if (whitespaceRemovalCount is object && wsText is object)
                        {
                            wsText = wsText.Substring(whitespaceRemovalCount.Value);
                        }

                        // Append all whitespace after the removal amount, plus the text.
                        descriptionBuilder.Append(wsText);
                        descriptionBuilder.Append(text.GetText());

                        if (lineIdx < lastTextIndex)
                        {
                            // Only add the line if we're not at the end.
                            descriptionBuilder.AppendLine();
                        }
                    }
                }
            }
            else
            {
                return null;
            }

            return descriptionBuilder.ToString();
        }
    }
}
