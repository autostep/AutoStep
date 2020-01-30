using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using AutoStep.Elements.StepTokens;
using static AutoStep.Compiler.Parser.AutoStepParser;

namespace AutoStep
{
    /// <summary>
    /// Provides a visitor for handling single-line tokenisation.
    /// </summary>
    internal class AutoStepLineVisitor : AutoStepParserBaseVisitor<LineTokeniserState>
    {
        private readonly IAutoStepLinker linker;
        private readonly OnlyLineContext lineContext;
        private readonly CommonTokenStream tokenStream;
        private readonly LineTokeniserState lastState;
        private List<LineToken>? lineTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepLineVisitor"/> class.
        /// </summary>
        /// <param name="linker">The linker to use for binding step lines (for better tokenisation).</param>
        /// <param name="lineContext">The parsed line context.</param>
        /// <param name="tokenStream">The parsed token stream.</param>
        /// <param name="lastState">The last state of the tokeniser (i.e. the state of the previous line).</param>
        public AutoStepLineVisitor(IAutoStepLinker linker, OnlyLineContext lineContext, CommonTokenStream tokenStream, LineTokeniserState lastState)
        {
            this.linker = linker;
            this.lineContext = lineContext;
            this.tokenStream = tokenStream;
            this.lastState = lastState;
        }

        /// <summary>
        /// Build a line result from the information provided in the constructor.
        /// </summary>
        /// <returns>A line tokenisation result.</returns>
        public LineTokeniseResult BuildLineResult()
        {
            lineTokens = null;

            var finalState = Visit(lineContext);

            IToken? commentToken = null;

            if (lineContext.Stop is null)
            {
                // Nothing there, check the line for anything at all.
                var firstToken = tokenStream.Get(0);

                if (firstToken.Type == AutoStepLexer.TEXT_COMMENT ||
                    firstToken.Type == AutoStepLexer.STATEMENT_COMMENT ||
                    firstToken.Type == AutoStepLexer.DEF_COMMENT ||
                    firstToken.Type == AutoStepLexer.ROW_COMMENT)
                {
                    commentToken = firstToken;
                }
            }
            else
            {
                // Check for any comments (they have to come at the end of a line or be on their own).
                commentToken = tokenStream.GetHiddenTokensToRight(lineContext.Stop.TokenIndex)?.FirstOrDefault();
            }

            if (commentToken is object)
            {
                // Token column position
                var commentColumn = commentToken.Column;

                // Skip over whitespace.
                var commentMarkerIndex = commentToken.Text.IndexOf('#', StringComparison.CurrentCulture);

                // Move the column along to ignore whitespace.
                commentColumn += commentMarkerIndex;

                if (lineTokens is null)
                {
                    return new LineTokeniseResult(finalState, new LineToken(commentColumn, LineTokenCategory.Comment));
                }
                else
                {
                    lineTokens.Add(new LineToken(commentColumn, LineTokenCategory.Comment));
                }
            }

            if (lineTokens is null)
            {
                return new LineTokeniseResult(finalState, Enumerable.Empty<LineToken>());
            }

            return new LineTokeniseResult(finalState, lineTokens);
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineTag(LineTagContext context)
        {
            lineTokens = new List<LineToken>(2)
            {
                new LineToken(context.TAG().Symbol.Column, LineTokenCategory.Annotation, LineTokenSubCategory.Tag),
            };

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineOpt(LineOptContext context)
        {
            lineTokens = new List<LineToken>(2)
            {
                new LineToken(context.OPTION().Symbol.Column, LineTokenCategory.Annotation, LineTokenSubCategory.Option),
            };

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineStepDefine(LineStepDefineContext context)
        {
            // We'll assume an initial token capacity of 10 (that's a lot of arguments/text combos).
            lineTokens = new List<LineToken>(10);

            // The step define marker is the first one we can really go for.
            var keyword = context.STEP_DEFINE();

            lineTokens.Add(new LineToken(keyword.Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.StepDefine));

            VisitChildren(context);

            return LineTokeniserState.EntryBlock;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitDeclareGiven(DeclareGivenContext context)
        {
            lineTokens!.Add(new LineToken(context.DEF_GIVEN().Symbol.Column, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Given));

            VisitChildren(context);

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitDeclareWhen(DeclareWhenContext context)
        {
            lineTokens!.Add(new LineToken(context.DEF_WHEN().Symbol.Column, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.When));

            VisitChildren(context);

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitDeclareThen(DeclareThenContext context)
        {
            lineTokens!.Add(new LineToken(context.DEF_THEN().Symbol.Column, LineTokenCategory.StepTypeKeyword, LineTokenSubCategory.Then));

            VisitChildren(context);

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitDeclarationArgument(DeclarationArgumentContext context)
        {
            lineTokens!.Add(new LineToken(context.Start.Column, LineTokenCategory.BoundArgument, LineTokenSubCategory.Declaration));

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitDeclarationSection(DeclarationSectionContext context)
        {
            var bodyType = context.stepDeclarationSectionContent();

            if (!(bodyType is DeclarationWsContext))
            {
                lineTokens!.Add(new LineToken(context.Start.Column, LineTokenCategory.StepText, LineTokenSubCategory.Declaration));
            }

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineFeature(LineFeatureContext context)
        {
            var featureText = context.text();

            // Feature items can have up to 2 tokens, plus the possible comment.
            lineTokens = new List<LineToken>(3)
            {
                new LineToken(context.FEATURE().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.Feature),
            };

            if (featureText is object)
            {
                lineTokens.Add(new LineToken(featureText.Start.Column, LineTokenCategory.EntityName, LineTokenSubCategory.Feature));
            }

            return LineTokeniserState.EntryBlock;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineBackground(LineBackgroundContext context)
        {
            // One marker, plus possible comment.
            lineTokens = new List<LineToken>(2)
            {
                new LineToken(context.BACKGROUND().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.Background),
            };

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineExamples(LineExamplesContext context)
        {
            // One marker, plus possible comment.
            lineTokens = new List<LineToken>(2)
            {
                new LineToken(context.EXAMPLES().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.Examples),
            };

            return LineTokeniserState.EntryBlock;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineScenarioOutline(LineScenarioOutlineContext context)
        {
            lineTokens = new List<LineToken>(3);

            var scenarioText = context.text();

            lineTokens.Add(new LineToken(context.SCENARIO_OUTLINE().Symbol.Column, LineTokenCategory.EntryMarker, LineTokenSubCategory.ScenarioOutline));

            if (scenarioText is object)
            {
                lineTokens.Add(new LineToken(scenarioText.Start.Column, LineTokenCategory.EntityName, LineTokenSubCategory.ScenarioOutline));
            }

            return default;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineTableRow(LineTableRowContext context)
        {
            // Go for number of tokens equal to the interval width
            lineTokens = new List<LineToken>(context.SourceInterval.Length);

            VisitChildren(context);

            // Add the end delimiter.
            var cellDelimit = context.CELL_DELIMITER();

            if (cellDelimit is object)
            {
                lineTokens.Add(new LineToken(cellDelimit.Symbol.Column, LineTokenCategory.TableBorder));
            }

            return LineTokeniserState.TableRow;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitTableRowCell(TableRowCellContext context)
        {
            var cellStart = context.TABLE_START() ?? context.CELL_DELIMITER();

            if (cellStart is object)
            {
                lineTokens!.Add(new LineToken(cellStart.Symbol.Column, LineTokenCategory.TableBorder));
            }

            VisitChildren(context);

            return LineTokeniserState.TableRow;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitTableRowCellContent(TableRowCellContentContext context)
        {
            var contentBlocks = context.cellContentBlock();

            foreach (var block in contentBlocks)
            {
                var tokenType = LineTokenCategory.Text;
                var subCategory = LineTokenSubCategory.Header;

                // If the previous state was a table row, then this isn't a header.
                if (lastState == LineTokeniserState.TableRow)
                {
                    subCategory = LineTokenSubCategory.Cell;

                    if (block is CellVariableContext)
                    {
                        tokenType = LineTokenCategory.Variable;
                    }
                }

                lineTokens!.Add(new LineToken(block.Start.Column, tokenType, subCategory));
            }

            return LineTokeniserState.TableRow;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineGiven(LineGivenContext context)
        {
            AddTokensForStatement(StepType.Given, LineTokenSubCategory.Given, context.GIVEN(), context.statementBody());

            return LineTokeniserState.Given;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineWhen(LineWhenContext context)
        {
            AddTokensForStatement(StepType.When, LineTokenSubCategory.When, context.WHEN(), context.statementBody());

            return LineTokeniserState.When;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineThen(LineThenContext context)
        {
            AddTokensForStatement(StepType.Then, LineTokenSubCategory.Then, context.THEN(), context.statementBody());

            return LineTokeniserState.Then;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineAnd(LineAndContext context)
        {
            AddTokensForStatement(StepType.And, LineTokenSubCategory.And, context.AND(), context.statementBody());

            return lastState;
        }

        /// <inheritdoc/>
        public override LineTokeniserState VisitLineText(LineTextContext context)
        {
            var text = context.text();

            if (text is null)
            {
                // Empty line.
                return lastState;
            }

            // One for the text, one for the potential comment.
            lineTokens = new List<LineToken>(2);

            if (lastState == LineTokeniserState.EntryBlock)
            {
                // Descriptive text.
                lineTokens.Add(new LineToken(text.Start.Column, LineTokenCategory.Text, LineTokenSubCategory.Description));
            }

            return lastState;
        }

        private void AddTokensForStatement(StepType type, LineTokenSubCategory stepCategory, ITerminalNode marker, StatementBodyContext? statementContext)
        {
            if (statementContext is object)
            {
                var stepVisitor = new StepReferenceVisitor(null, tokenStream);

                var stepRef = stepVisitor.BuildStep(type, statementContext);

                // Use the number of built reference tokens for the size of the line token set, plus 1 for the
                // marker and 1 for the potential comment.
                lineTokens = new List<LineToken>(stepRef.TokenSpan.Length + 2)
                {
                    new LineToken(marker.Symbol.Column, LineTokenCategory.StepTypeKeyword, stepCategory),
                };

                if (type == StepType.And)
                {
                    type = lastState switch
                    {
                        LineTokeniserState.Given => StepType.Given,
                        LineTokeniserState.When => StepType.When,
                        LineTokeniserState.Then => StepType.Then,
                        _ => type
                    };
                }

                if (type != StepType.And)
                {
                    stepRef.BindingType = type;

                    // See if we can link the step.
                    linker.BindSingleStep(stepRef);
                }

                var binding = stepRef.Binding;

                var tokens = stepRef.TokenSpan;

                ReadOnlySpan<ArgumentBinding> arguments = default;

                if (binding is object)
                {
                    arguments = binding.Arguments;
                }

                var currentArgumentIdx = 0;
                var currentArgumentTokenIdx = 0;

                for (int tokenIdx = 0; tokenIdx < tokens.Length; tokenIdx++)
                {
                    var token = tokens[tokenIdx];

                    // Shift to a zero-based index.
                    var tokenPosition = token.StartColumn - 1;

                    // Check if the token is inside a bound argument.
                    if (binding is object)
                    {
                        var isArgument = false;
                        if (arguments.Length > currentArgumentIdx)
                        {
                            var currentArgument = binding.Arguments[currentArgumentIdx];

                            // If it's the same token.
                            if (currentArgument.MatchedTokens[currentArgumentTokenIdx] == token)
                            {
                                isArgument = true;

                                currentArgumentTokenIdx++;

                                if (currentArgumentTokenIdx >= currentArgument.MatchedTokens.Length)
                                {
                                    // Move to the next argument as well.
                                    currentArgumentIdx++;
                                    currentArgumentTokenIdx = 0;
                                }
                            }
                        }

                        if (token is VariableToken)
                        {
                            if (isArgument)
                            {
                                lineTokens.Add(new LineToken(tokenPosition, LineTokenCategory.BoundArgument, LineTokenSubCategory.ArgumentVariable));
                            }
                            else
                            {
                                lineTokens.Add(new LineToken(tokenPosition, LineTokenCategory.StepText, LineTokenSubCategory.Bound));
                            }
                        }
                        else if (isArgument)
                        {
                            lineTokens.Add(new LineToken(tokenPosition, LineTokenCategory.BoundArgument));
                        }
                        else
                        {
                            lineTokens.Add(new LineToken(tokenPosition, LineTokenCategory.StepText, LineTokenSubCategory.Bound));
                        }
                    }
                    else if (token is VariableToken)
                    {
                        // Add an unbound variable token.
                        // Shift to a zero-based index.
                        lineTokens.Add(new LineToken(tokenPosition, LineTokenCategory.Variable, LineTokenSubCategory.Unbound));
                    }
                    else
                    {
                        // Not bound, just add the token as text, unless it's a variable. We already know about those.
                        // Shift to a zero-based index.
                        lineTokens.Add(new LineToken(tokenPosition, LineTokenCategory.StepText, LineTokenSubCategory.Unbound));
                    }
                }
            }
            else
            {
                // One item for the marker, one for a potential comment.
                lineTokens = new List<LineToken>(2)
                {
                    new LineToken(marker.Symbol.Column, LineTokenCategory.StepTypeKeyword, stepCategory),
                };
            }
        }
    }
}
