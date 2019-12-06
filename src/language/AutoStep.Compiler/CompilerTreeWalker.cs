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

            file.Feature = LineInfo(new BuiltFeature(), context);

            currentAnnotatable = file.Feature;

            VisitChildren(context);

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

            var title = context.featureTitle().text().GetText();
            var description = context.description()?.GetText();

            currentAnnotatable = null;

            file.Feature.Name = title;
            file.Feature.Description = string.IsNullOrWhiteSpace(description) ? null : description;

            return file;
        }

        public override BuiltFile? VisitFeatureBody([NotNull] AutoStepParser.FeatureBodyContext context)
        {
            Debug.Assert(file is object);
            bool foundScenario = false;

            foreach (var scenario in context.scenarioBlock())
            {
                foundScenario = true;
                Visit(scenario);
            }

            if (!foundScenario)
            {
                AddMessage(context, CompilerMessageLevel.Warning, CompilerMessageCode.NoScenarios);
            }

            return file;
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

            var scenario = LineInfo(new BuiltScenario(), context);

            currentScenario = scenario;
            currentAnnotatable = scenario;

            var annotations = context.annotations();
            if (annotations is object)
            {
                Visit(annotations);
            }

            var definition = context.scenarioDefinition();
            var description = definition.description()?.GetText();

            scenario.Name = definition.text().GetText();
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

        private void AddMessage(ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code)
        {
            if(level == CompilerMessageLevel.Error)
            {
                Success = false;
            }

            var message = new CompilerMessage(
                sourceName,
                level,
                code,
                CompilerMessages.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture),
                context.Start.Line,
                context.Start.Column + 1,
                context.Stop.Line,
                context.Stop.Column);

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
