using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;
using AutoStep.Core;

namespace AutoStep.Compiler
{
    internal class CompilerTreeWalker : AutoStepParserBaseVisitor<BuiltFile?>
    {
        private BuiltFile? file;
        private string? sourceName;
        private IAnnotatable? currentAnnotatable;
        private List<CompilerMessage> messages = new List<CompilerMessage>();
        private BuiltScenario? currentScenario;
        private List<StepReference> currentStepSet = null;
        private bool currentStepSetHasInitial = false;

        public IReadOnlyList<CompilerMessage> Messages => messages;

        public CompilerTreeWalker(string? sourceName)
        {
            this.sourceName = sourceName;
        }

        public BuiltFile? Result => file;

        public bool Success { get; private set; }

        public override BuiltFile? VisitFile(AutoStepParser.FileContext context)
        {
            Success = true;
            file = new BuiltFile();

            VisitChildren(context);

            return file;
        }

        public override BuiltFile? VisitFeatureBlock([NotNull] AutoStepParser.FeatureBlockContext context)
        {
            Debug.Assert(file is object);

            if (file.Feature != null)
            {
                // We already have a feature, don't go any deeper, add an error.
                AddMessage(context.featureDefinition().featureTitle(), CompilerMessageLevel.Error, CompilerMessageCode.OnlyOneFeatureAllowed);
                return file;
            }

            file.Feature = new BuiltFeature();

            currentAnnotatable = file.Feature;

            VisitChildren(context);

            if (file.Feature.Scenarios.Count == 0)
            {
                // Warning should be associated to the title.
                AddMessage(context.featureDefinition().featureTitle(), CompilerMessageLevel.Warning, CompilerMessageCode.NoScenarios, file.Feature.Name);
            }

            return file;
        }

        public override BuiltFile? VisitTagAnnotation(AutoStepParser.TagAnnotationContext context)
        {
            Debug.Assert(file is object);

            if (currentAnnotatable == null)
            {
                AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.UnexpectedAnnotation);
                return file;
            }

            var tagBody = context.TAG().GetText().Substring(1);

            currentAnnotatable.Annotations.Add(LineInfo(new TagElement { Tag = tagBody }, context));

            return file;
        }

        public override BuiltFile? VisitOptionAnnotation(AutoStepParser.OptionAnnotationContext context)
        {
            Debug.Assert(file is object);

            if (currentAnnotatable == null)
            {
                AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.UnexpectedAnnotation);
                return file;
            }

            var tagBody = context.OPTION().GetText().Substring(1);

            currentAnnotatable.Annotations.Add(LineInfo(new TagElement { Tag = tagBody }, context));

            return file;
        }

        public override BuiltFile? VisitFeatureDefinition([NotNull] AutoStepParser.FeatureDefinitionContext context)
        {
            Debug.Assert(file is object);

            var titleTree = context.featureTitle();

            var title = titleTree.text().GetText();
            var description = ExtractDescription(context.description());

            currentAnnotatable = null;

            LineInfo(file.Feature, titleTree);

            file.Feature.Name = title;
            file.Feature.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            return file;
        }

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

                        if(lineIdx < lastTextIndex)
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

        public override BuiltFile? VisitBackgroundBody([NotNull] AutoStepParser.BackgroundBodyContext context)
        {
            Debug.Assert(file is object);

            currentStepSet = file.Feature.Background;
            currentStepSetHasInitial = false;

            return base.VisitBackgroundBody(context);
        }

        public override BuiltFile? VisitScenarioBlock([NotNull] AutoStepParser.ScenarioBlockContext context)
        {
            Debug.Assert(file is object);

            var scenario = new BuiltScenario();

            currentScenario = scenario;
            currentAnnotatable = scenario;

            var annotations = context.annotations();
            if (annotations is object)
            {
                Visit(annotations);
            }

            var definition = context.scenarioDefinition();

            var description = ExtractDescription(definition.description());
            var title = definition.scenarioTitle();

            LineInfo(scenario, title);

            scenario.Name = title.text().GetText();
            scenario.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            currentStepSet = scenario.Steps;
            currentStepSetHasInitial = false;

            currentAnnotatable = null;

            Visit(context.scenarioBody());

            file.Feature.Scenarios.Add(scenario);

            currentScenario = null;

            return file;
        }

        public override BuiltFile? VisitGiven([NotNull] AutoStepParser.GivenContext context)
        {
            AddStep(StepType.Given, context, context.statementBody());

            return file;
        }

        public override BuiltFile? VisitThen([NotNull] AutoStepParser.ThenContext context)
        {
            AddStep(StepType.Then, context, context.statementBody());

            return file;
        }

        public override BuiltFile? VisitWhen([NotNull] AutoStepParser.WhenContext context)
        {
            AddStep(StepType.When, context, context.statementBody());

            return file;
        }

        public override BuiltFile? VisitAnd([NotNull] AutoStepParser.AndContext context)
        {
            AddStep(StepType.And, context, context.statementBody());

            return file;
        }

        private void AddStep(StepType type, ParserRuleContext context, AutoStepParser.StatementBodyContext bodyContext)
        {
            if (currentStepSet == null)
            {
                AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.StepNotExpected);
                return;
            }

            if (type == StepType.And)
            {
                if (!currentStepSetHasInitial)
                {
                    AddMessage(context, CompilerMessageLevel.Error, CompilerMessageCode.AndMustFollowNormalStep);
                }
            }
            else
            {
                currentStepSetHasInitial = true;
            }

            currentStepSet.Add(LineInfo(
                new UnknownStepReference
                {
                    Type = type,
                    Text = bodyContext.GetText(),
                }, context));
        }

        private void AddMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            if(level == CompilerMessageLevel.Error)
            {
                Success = false;
            }

            var message = new CompilerMessage(
                sourceName,
                level,
                code,
                string.Format(CultureInfo.CurrentCulture, CompilerMessages.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture), args),
                context.Start.Line,
                context.Start.Column + 1,
                context.Stop.Line,
                context.Stop.Column + 1 + (context.Stop.StopIndex - context.Stop.StartIndex));

            messages.Add(message);
        }

        private TElement LineInfo<TElement>(TElement element, ParserRuleContext ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Start.Line;
            element.SourceColumn = ctxt.Start.Column + 1;

            return element;
        }
    }
}
