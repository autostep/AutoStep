using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using static AutoStep.Compiler.Parser.AutoStepParser;

namespace AutoStep
{

    internal class AutoStepLineVisitor : AutoStepParserBaseVisitor<LineTokeniserState>
    {
        private List<LineToken>? lineTokens;

        public LineTokeniseResult VisitLine(OnlyLineContext lineContext, CommonTokenStream tokenStream)
        {
            // Assume the number of tokens are going to be roughly the size of the token feed.
            var finalState = Visit(lineContext);

            // Check for any comments (they have to come at the end of a line or be on their own).
            var commentToken = tokenStream.GetHiddenTokensToRight(lineContext.Stop.TokenIndex).FirstOrDefault();

            if (commentToken is object)
            {
                if (lineTokens is null)
                {
                    return new LineTokeniseResult(finalState, new LineToken(commentToken.Column, LineTokenCategory.Comment));
                }
                else
                {
                    lineTokens.Add(new LineToken(commentToken.Column, LineTokenCategory.Comment));
                }
            }
            else if (lineTokens is null)
            {
                return new LineTokeniseResult(finalState, Enumerable.Empty<LineToken>());
            }

            return new LineTokeniseResult(finalState, lineTokens);
        }

        public override LineTokeniserState VisitLineTag(LineTagContext context)
        {
            lineTokens!.Add(new LineToken(context.TAG().Symbol.Column, LineTokenCategory.Annotation, LineTokenSubCategory.Tag));

            return default;
        }

        public override LineTokeniserState VisitLineOpt(LineOptContext context)
        {
            lineTokens!.Add(new LineToken(context.OPTION().Symbol.Column, LineTokenCategory.Annotation, LineTokenSubCategory.Option));

            return default;
        }

        public override LineTokeniserState VisitLineStepDefine(LineStepDefineContext context)
        {
            // We'll assume an initial token capacity of 10 (that's a lot of arguments/text combos).
            lineTokens = new List<LineToken>(10);

            // The step define marker is the first one we can really go for.
            var keyword = context.STEP_DEFINE();

            lineTokens.Add(new LineToken(keyword.Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine));

            VisitChildren(context);

            return default;
        }

        public override LineTokeniserState VisitDeclareGiven(DeclareGivenContext context)
        {
            lineTokens!.Add(new LineToken(context.DEF_GIVEN().Symbol.Column, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given));

            VisitChildren(context);

            return default;
        }

        public override LineTokeniserState VisitDeclareWhen(DeclareWhenContext context)
        {
            lineTokens!.Add(new LineToken(context.DEF_WHEN().Symbol.Column, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.When));

            VisitChildren(context);

            return default;
        }

        public override LineTokeniserState VisitDeclareThen(DeclareThenContext context)
        {
            lineTokens!.Add(new LineToken(context.DEF_THEN().Symbol.Column, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Then));

            VisitChildren(context);

            return default;
        }

        public override LineTokeniserState VisitDeclarationArgument(DeclarationArgumentContext context)
        {
            lineTokens!.Add(new LineToken(context.Start.Column, LineTokenCategory.ArgumentDeclaration));

            return default;
        }

        public override LineTokeniserState VisitDeclarationSection(DeclarationSectionContext context)
        {
            lineTokens!.Add(new LineToken(context.Start.Column, LineTokenCategory.TextDeclaration));

            return default;
        }

        public override LineTokeniserState VisitLineFeature(LineFeatureContext context)
        {
            var featureText = context.text();

            // Feature items can have up to 2 tokens, plus the possible comment.
            lineTokens = new List<LineToken>(3);

            lineTokens.Add(new LineToken(context.FEATURE().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.Feature));

            if (featureText is object)
            {
                lineTokens.Add(new LineToken(featureText.Start.Column, LineTokenCategory.EntityName, LineTokenSubCategory.Feature));
            }

            return default;
        }

        public override LineTokeniserState VisitLineBackground(LineBackgroundContext context)
        {
            // One marker, plus possible comment.
            lineTokens = new List<LineToken>(2);

            lineTokens.Add(new LineToken(context.BACKGROUND().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.Background));

            return default;
        }

        public override LineTokeniserState VisitLineScenario(LineScenarioContext context)
        {
            lineTokens = new List<LineToken>(3);

            var scenarioText = context.text();

            lineTokens.Add(new LineToken(context.SCENARIO().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.Scenario));

            if (scenarioText is object)
            {
                lineTokens.Add(new LineToken(scenarioText.Start.Column, LineTokenCategory.EntityName, LineTokenSubCategory.Scenario));
            }

            return default;
        }

        public override LineTokeniserState VisitLineScenarioOutline(LineScenarioOutlineContext context)
        {
            return base.VisitLineScenarioOutline(context);
        }
    }
}
