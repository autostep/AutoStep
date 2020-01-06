using System;
using System.Diagnostics;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Generates table elements from table parse contexts.
    /// </summary>
    internal class TableVisitor : ArgumentHandlingVisitor<TableElement>
    {
        private readonly Func<ParserRuleContext, string, CompilerMessage?> insertionNameValidator;
        private readonly (int TokenType, string Replace)[] tableCellReplacements = new[]
        {
            (AutoStepParser.ESCAPED_TABLE_DELIMITER, "|"),
            (AutoStepParser.ARG_EXAMPLE_START_ESCAPE, "<"),
            (AutoStepParser.ARG_EXAMPLE_END_ESCAPE, ">"),
        };

        private TableRowElement? currentRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableVisitor"/> class.
        /// </summary>
        /// <param name="sourceName">The name of the source.</param>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="rewriter">A shared escape rewriter.</param>
        /// <param name="insertionNameValidator">The insertion name validator callback, used to check for valid insertion names.</param>
        public TableVisitor(string? sourceName, ITokenStream tokenStream, TokenStreamRewriter rewriter, Func<ParserRuleContext, string, CompilerMessage?> insertionNameValidator)
            : base(sourceName, tokenStream, rewriter)
        {
            this.insertionNameValidator = insertionNameValidator;
        }

        /// <summary>
        /// Build a table from the Antlr table block context.
        /// </summary>
        /// <param name="bodyContext">The body context.</param>
        /// <returns>The built table.</returns>
        public TableElement BuildTable(AutoStepParser.TableBlockContext bodyContext)
        {
            Result = new TableElement();

            LineInfo(Result, bodyContext.tableHeader());

            Visit(bodyContext);

            return Result;
        }

        /// <summary>
        /// Vists a table header.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitTableHeader([NotNull] AutoStepParser.TableHeaderContext context)
        {
            Debug.Assert(Result is object);

            LineInfo(Result.Header, context);

            base.VisitTableHeader(context);

            return Result;
        }

        /// <summary>
        /// Vists a table header cell.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitTableHeaderCell([NotNull] AutoStepParser.TableHeaderCellContext context)
        {
            Debug.Assert(Result is object);

            var headerTextBlock = context.headerCell();

            var header = new TableHeaderCellElement
            {
                HeaderName = headerTextBlock?.GetText(),
            };

            if (headerTextBlock is null)
            {
                var cellWs = context.CELL_WS(0);

                if (cellWs is object)
                {
                    PositionalLineInfo(header, cellWs);
                }
                else
                {
                    PositionalLineInfo(header, context);
                }
            }
            else
            {
                PositionalLineInfo(header, headerTextBlock);
            }

            Result.Header.AddHeader(header);

            return Result;
        }

        /// <summary>
        /// Vists a table data row.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitTableRow([NotNull] AutoStepParser.TableRowContext context)
        {
            Debug.Assert(Result is object);

            currentRow = LineInfo(new TableRowElement(), context);

            base.VisitTableRow(context);

            // Check if the number of cells in the row doesn't match the headings.
            if (currentRow.Cells.Count != Result.ColumnCount)
            {
                AddMessageStoppingAtPrecedingToken(context, CompilerMessageLevel.Error, CompilerMessageCode.TableColumnsMismatch, currentRow.Cells.Count, Result.ColumnCount);
            }

            Result.AddRow(currentRow);

            return Result;
        }

        /// <summary>
        /// Vists the cell of a table data row.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitTableRowCell([NotNull] AutoStepParser.TableRowCellContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentRow is object);

            var cellContent = context.tableRowCellContent();

            if (cellContent == null)
            {
                // Empty cell, add a cell with an empty argument.
                var cell = new TableCellElement();

                var cellWs = context.CELL_WS(0);

                var arg = new StepArgumentElement
                {
                    RawArgument = null,
                    Type = ArgumentType.Empty,
                    EscapedArgument = null,
                    Value = null,
                };

                if (cellWs == null)
                {
                    // If there's no whitespace, we'll just have to use the start of the table delimiter.
                    PositionalLineInfo(cell, context);
                    PositionalLineInfo(arg, context);
                }
                else
                {
                    PositionalLineInfo(cell, cellWs);
                    PositionalLineInfo(arg, cellWs);
                }

                cell.Value = arg;

                currentRow.AddCell(cell);
            }
            else
            {
                Visit(cellContent);
            }

            return Result;
        }

        /// <summary>
        /// Vists a float value in a table data cell.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitCellFloat([NotNull] AutoStepParser.CellFloatContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentRow is object);

            var cell = new TableCellElement();

            PositionalLineInfo(cell, context);

            var valueText = context.CELL_FLOAT().GetText();
            var symbolText = context.CELL_CURR_SYMBOL()?.GetText();
            var content = symbolText + valueText;

            var arg = PositionalLineInfo(
                new StepArgumentElement
                {
                    RawArgument = content,
                    Type = ArgumentType.NumericDecimal,
                    EscapedArgument = content,
                    Value = decimal.Parse(valueText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture),
                    Symbol = symbolText,
                }, context);

            cell.Value = arg;

            currentRow.AddCell(cell);

            return Result;
        }

        /// <summary>
        /// Vists an int value in a table data cell.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitCellInt([NotNull] AutoStepParser.CellIntContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentRow is object);

            var cell = new TableCellElement();

            PositionalLineInfo(cell, context);

            var valueText = context.CELL_INT().GetText();
            var symbolText = context.CELL_CURR_SYMBOL()?.GetText();
            var content = symbolText + valueText;

            var arg = PositionalLineInfo(
                new StepArgumentElement
                {
                    RawArgument = content,
                    Type = ArgumentType.NumericInteger,
                    EscapedArgument = content,
                    Value = int.Parse(valueText, NumberStyles.AllowThousands, CultureInfo.CurrentCulture),
                    Symbol = symbolText,
                }, context);

            cell.Value = arg;

            currentRow.AddCell(cell);

            return Result;
        }

        /// <summary>
        /// Vists an interpolated value in a table data cell.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitCellInterpolate([NotNull] AutoStepParser.CellInterpolateContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentRow is object);

            var cell = new TableCellElement();

            PositionalLineInfo(cell, context);

            var contentBlock = context.cellArgument();

            Visit(contentBlock);

            PersistWorkingTextSection(tableCellReplacements);

            var escaped = Rewriter.GetText(contentBlock.SourceInterval);

            var arg = new StepArgumentElement
            {
                RawArgument = contentBlock.GetText(),
                Type = ArgumentType.Interpolated,

                // The rewriter will contain any modifications that replace the escaped characters.
                EscapedArgument = escaped,
            };

            arg.ReplaceSections(CurrentArgumentSections);

            PositionalLineInfo(arg, context);

            CurrentArgumentSections.Clear();
            CanArgumentValueBeDetermined = true;

            cell.Value = arg;

            currentRow.AddCell(cell);

            return Result;
        }

        /// <summary>
        /// Vists a text value in a table data cell.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitCellText([NotNull] AutoStepParser.CellTextContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentRow is object);

            var cell = new TableCellElement();

            PositionalLineInfo(cell, context);

            var contentBlock = context.cellArgument();

            Visit(contentBlock);

            PersistWorkingTextSection(tableCellReplacements);

            var escaped = Rewriter.GetText(contentBlock.SourceInterval);

            var arg = new StepArgumentElement
            {
                RawArgument = contentBlock.GetText(),
                Type = ArgumentType.Text,

                // The rewriter will contain any modifications that replace the escaped characters.
                EscapedArgument = escaped,
            };

            arg.ReplaceSections(CurrentArgumentSections);

            PositionalLineInfo(arg, context);

            if (CanArgumentValueBeDetermined)
            {
                arg.Value = escaped;
            }

            CurrentArgumentSections.Clear();
            CanArgumentValueBeDetermined = true;

            cell.Value = arg;

            currentRow.AddCell(cell);

            return Result;
        }

        /// <summary>
        /// Visit the example cell block (which is the part of the cell that contains an example reference).
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>The file.</returns>
        public override TableElement VisitExampleCellBlock([NotNull] AutoStepParser.ExampleCellBlockContext context)
        {
            Debug.Assert(Result is object);

            PersistWorkingTextSection(tableCellReplacements);

            var content = context.GetText();

            var escaped = EscapeText(
                context,
                tableCellReplacements);

            var allBodyInterval = context.cellExampleNameBody().SourceInterval;

            var insertionName = Rewriter.GetText(allBodyInterval);

            var arg = new ArgumentSectionElement
            {
                RawText = content,
                EscapedText = escaped,

                // The insertion name is the escaped name inside the angle brackets
                ExampleInsertionName = Rewriter.GetText(allBodyInterval),
            };

            // If we've got an insertion, then the value of an argument cannot be determined at compile time.
            CanArgumentValueBeDetermined = false;

            var additionalError = insertionNameValidator(context, insertionName);

            if (additionalError is object)
            {
                AddMessage(additionalError);
            }

            CurrentArgumentSections.Add(PositionalLineInfo(arg, context));

            return Result;
        }
    }
}
