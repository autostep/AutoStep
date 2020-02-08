using System;
using System.Collections.Generic;
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Elements;
using AutoStep.Elements.Test;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test.Visitors
{
    /// <summary>
    /// The FileVisitor is an implementation of an Antlr Visitor that traverses the Antlr parse tree after the parse process has completed,
    /// and builts a <see cref="FileElement"/> from that tree.
    /// </summary>
    internal class FileVisitor : BaseAutoStepTestVisitor<FileElement>
    {
        private readonly StepReferenceVisitor stepVisitor;
        private readonly TableVisitor tableVisitor;
        private readonly StepDefinitionVisitor stepDefinitionVisitor;

        private IAnnotatableElement? currentAnnotatable;
        private ScenarioElement? currentScenario;

        private List<StepReferenceElement>? currentStepSet = null;
        private StepReferenceElement? lastStep = null;
        private StepReferenceElement? currentStepSetLastConcrete = null;
        private StepDefinitionElement? currentStepDefinition = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The source name; added to compiler messages.</param>
        /// <param name="tokens">The token stream used by the parse tree.</param>
        public FileVisitor(string? sourceName, ITokenStream tokens)
            : base(sourceName, tokens)
        {
            stepVisitor = new StepReferenceVisitor(sourceName, TokenStream, Rewriter, ValidateVariableInsertionName);
            tableVisitor = new TableVisitor(sourceName, TokenStream, Rewriter, ValidateVariableInsertionName);
            stepDefinitionVisitor = new StepDefinitionVisitor(sourceName, TokenStream, Rewriter);
        }

        /// <summary>
        /// Visits the top level Result node. Creates the Result object.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitFile(AutoStepParser.FileContext context)
        {
            Result = new FileElement();

            VisitChildren(context);

            return Result;
        }

        /// <summary>
        /// Visits the Feature block (Feature:) and generates a <see cref="FeatureElement"/> that is added to the Result.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitFeatureBlock([NotNull] AutoStepParser.FeatureBlockContext context)
        {
            Debug.Assert(Result is object);

            if (Result.Feature != null)
            {
                // We already have a feature, don't go any deeper, add an error.
                MessageSet.Add(context.featureDefinition().featureTitle(), CompilerMessageLevel.Error, CompilerMessageCode.OnlyOneFeatureAllowed);
                return Result;
            }

            Result.Feature = new FeatureElement();

            currentAnnotatable = Result.Feature;

            VisitChildren(context);

            if (Result.Feature.Scenarios.Count == 0)
            {
                // Warning should be associated to the title.
                MessageSet.Add(context.featureDefinition().featureTitle(), CompilerMessageLevel.Warning, CompilerMessageCode.NoScenarios, Result.Feature.Name ?? "unknown");
            }

            return Result;
        }

        /// <summary>
        /// Visits a feature or scenario tag '@tag' and adds it to the current <see cref="IAnnotatableElement"/> feature or scenario object.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitTagAnnotation([NotNull] AutoStepParser.TagAnnotationContext context)
        {
            Debug.Assert(Result is object);

            if (currentAnnotatable == null)
            {
                MessageSet.Add(context, CompilerMessageLevel.Error, CompilerMessageCode.UnexpectedAnnotation);
                return Result;
            }

            var tag = context.TAG();

            var tagBody = context.TAG().GetText().Substring(1).TrimEnd();

            currentAnnotatable.Annotations.Add(new TagElement(tagBody).AddLineInfo(tag));

            return Result;
        }

        /// <summary>
        /// Visits a feature or scenario option '$tag' and adds it to the current <see cref="IAnnotatableElement"/> feature or scenario object.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitOptionAnnotation([NotNull] AutoStepParser.OptionAnnotationContext context)
        {
            Debug.Assert(Result is object);

            if (currentAnnotatable == null)
            {
                MessageSet.Add(context, CompilerMessageLevel.Error, CompilerMessageCode.UnexpectedAnnotation);
                return Result;
            }

            var option = context.OPTION();

            var optBody = option.GetText().Substring(1);

            // Trim the body to get rid of trailing whitespace.
            optBody = optBody.TrimEnd();

            // Now split on the first ':'.
            var positionOfColon = optBody.IndexOf(':', StringComparison.CurrentCulture);

            string? setting = null;
            var name = optBody;

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
                    MessageSet.Add(option, CompilerMessageLevel.Error, CompilerMessageCode.OptionWithNoSetting, name);
                    return Result;
                }
            }

            currentAnnotatable.Annotations.Add(
                new OptionElement(name)
                {
                    Setting = setting,
                }.AddLineInfo(option));

            return Result;
        }

        /// <summary>
        /// Visits the feature definition (which defines the name and description).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitFeatureDefinition([NotNull] AutoStepParser.FeatureDefinitionContext context)
        {
            Debug.Assert(Result is object);

            var titleTree = context.featureTitle();

            var featureToken = titleTree.FEATURE();
            var featureKeyWordText = featureToken.GetText();

            // We want the parser to allow case-insensitive keywords through, so we can assert on them
            // here and give more useful errors.
            if (featureKeyWordText != "Feature:")
            {
                MessageSet.Add(featureToken, CompilerMessageLevel.Error, CompilerMessageCode.InvalidFeatureKeyword, featureKeyWordText);
            }

            var title = titleTree.text().GetText();
            var description = ExtractDescription(context.description());

            // Past this point, annotations aren't valid.
            currentAnnotatable = null;

            Result.Feature!.AddLineInfo(titleTree);

            Result.Feature!.Name = title;
            Result.Feature.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            return Result;
        }

        /// <summary>
        /// Visits the Background block (which can contain steps).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitBackgroundBlock([NotNull] AutoStepParser.BackgroundBlockContext context)
        {
            Debug.Assert(Result is object);

            var background = new BackgroundElement();

            background.AddLineInfo(context.BACKGROUND());

            Result.Feature!.Background = background;

            currentStepSet = background.Steps;
            currentStepSetLastConcrete = null;

            return base.VisitBackgroundBlock(context);
        }

        /// <summary>
        /// Visit a step definition parser block.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override FileElement VisitStepDefinitionBlock([NotNull] AutoStepParser.StepDefinitionBlockContext context)
        {
            Debug.Assert(Result is object);

            var definition = context.stepDefinition();

            var declaration = definition.stepDeclaration();

            var type = declaration switch
            {
                AutoStepParser.DeclareGivenContext _ => StepType.Given,
                AutoStepParser.DeclareWhenContext _ => StepType.When,
                AutoStepParser.DeclareThenContext _ => StepType.Then,
                _ => throw new LanguageEngineAssertException()
            };

            var stepDefinition = stepDefinitionVisitor.BuildStepDefinition(type, declaration, declaration.GetRuleContext<AutoStepParser.StepDeclarationBodyContext>(0));

            MergeVisitorAndReset(stepDefinitionVisitor);

            currentAnnotatable = stepDefinition;

            var annotations = context.annotations();
            if (annotations is object)
            {
                Visit(annotations);
            }

            currentAnnotatable = null;

            var description = ExtractDescription(definition.description());
            stepDefinition.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            if (stepDefinition.Arguments is object)
            {
                // At this point, we'll validate the provided 'arguments' to the step. All the arguments should just be variable names.
                foreach (var declaredArgument in stepDefinition.Arguments)
                {
                    // Validate type hints
                    // TODO
                }
            }

            currentStepSet = stepDefinition.Steps;
            currentStepDefinition = stepDefinition;

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
        public override FileElement VisitScenarioBlock([NotNull] AutoStepParser.ScenarioBlockContext context)
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
                    MessageSet.Add(scenarioToken, CompilerMessageLevel.Error, CompilerMessageCode.InvalidScenarioKeyword, scenarioKeyWordText);
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
                    MessageSet.Add(scenarioOutlineToken, CompilerMessageLevel.Error, CompilerMessageCode.InvalidScenarioOutlineKeyword, scenarioOutlineKeyWordText);
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

            scenario.AddLineInfo(title);

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

            Result.Feature!.Scenarios.Add(scenario);

            return Result;
        }

        /// <summary>
        /// Visits a Given step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitGiven([NotNull] AutoStepParser.GivenContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.Given, context);

            return Result;
        }

        /// <summary>
        /// Visits a Then step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitThen([NotNull] AutoStepParser.ThenContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.Then, context);

            return Result;
        }

        /// <summary>
        /// Visits a When step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitWhen([NotNull] AutoStepParser.WhenContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.When, context);

            return Result;
        }

        /// <summary>
        /// Visits an And step (and adds it to the current step collection).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitAnd([NotNull] AutoStepParser.AndContext context)
        {
            Debug.Assert(Result is object);

            AddStep(StepType.And, context);

            return Result;
        }

        /// <summary>
        /// Vists a statement that contains a table.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The Result.</returns>
        public override FileElement VisitStatementWithTable([NotNull] AutoStepParser.StatementWithTableContext context)
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
        public override FileElement VisitExampleBlock([NotNull] AutoStepParser.ExampleBlockContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentScenario is object);

            var outline = currentScenario as ScenarioOutlineElement;
            var exampleTokenText = context.EXAMPLES().GetText();

            if (exampleTokenText != "Examples:")
            {
                MessageSet.Add(context.EXAMPLES(), CompilerMessageLevel.Error, CompilerMessageCode.InvalidExamplesKeyword, exampleTokenText);
                return Result;
            }

            if (outline == null)
            {
                MessageSet.Add(context.EXAMPLES(), CompilerMessageLevel.Error, CompilerMessageCode.NotExpectingExample, currentScenario.Name!);
                return Result;
            }

            var example = new ExampleElement();

            currentAnnotatable = example;

            example.AddLineInfo(context.EXAMPLES());

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
                    return MessageSet.CreateMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.StepVariableNotDeclared, insertionName);
                }
            }
            else if (currentScenario is ScenarioOutlineElement outline)
            {
                if (!outline.ContainsVariable(insertionName))
                {
                    // Referencing an undeclared examples variable
                    return MessageSet.CreateMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableNotDeclared, insertionName);
                }
            }
            else if (currentScenario is object)
            {
                // Example variable in a regular scenario
                return MessageSet.CreateMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableInScenario, insertionName);
            }

            return null;
        }

        private void AddStep(StepType type, AutoStepParser.StatementContext context)
        {
            Debug.Assert(Result is object);

            if (currentStepSet is null && currentStepDefinition is null)
            {
                MessageSet.Add(context, CompilerMessageLevel.Error, CompilerMessageCode.StepNotExpected);
                return;
            }

            // All step references are currently added as 'unknown', until they are linked.
            StepType? bindingType = null;

            var step = stepVisitor.BuildStep(type, context);

            MergeVisitorAndReset(stepVisitor);

            if (step is object)
            {
                if (type == StepType.And)
                {
                    if (currentStepDefinition is object && currentStepSet is null)
                    {
                        // We are in the step declaration, which does not permit 'And'.
                        MessageSet.Add(context, CompilerMessageLevel.Error, CompilerMessageCode.InvalidStepDefineKeyword, type);
                    }
                    else if (currentStepSetLastConcrete is null)
                    {
                        MessageSet.Add(context, CompilerMessageLevel.Error, CompilerMessageCode.AndMustFollowNormalStep);
                    }
                    else
                    {
                        bindingType = currentStepSetLastConcrete.BindingType;
                    }
                }
                else
                {
                    bindingType = type;

                    if (currentStepSet is object)
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

                lastStep = step;
            }
        }
    }
}
