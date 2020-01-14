using System;
using System.Diagnostics;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;
using AutoStep.Elements.Parts;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Generates table elements from table parse contexts.
    /// </summary>
    internal class TableVisitor : BaseAutoStepVisitor<TableElement>
    {
        private readonly Func<ParserRuleContext, string, CompilerMessage?> insertionNameValidator;
        private readonly (int TokenType, string Replace)[] tableCellReplacements = new[]
        {
            (AutoStepParser.CELL_ESCAPED_DELIMITER, "|"),
            (AutoStepParser.CELL_ESCAPED_VARSTART, "<"),
            (AutoStepParser.CELL_ESCAPED_VAREND, ">"),
        };

        private TableRowElement? currentRow;
        private TableCellElement? currentCell;

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

            var variableName = context.cellVariableName();

            var header = new TableHeaderCellElement
            {
                HeaderName = variableName?.GetText(),
            };

            if (variableName is null)
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
                PositionalLineInfo(header, variableName);
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

            var cell = new TableCellElement();

            if (cellContent == null)
            {
                var cellWs = context.CELL_WS(0);

                if (cellWs == null)
                {
                    // If there's no whitespace, we'll just have to use the start of the table delimiter.
                    PositionalLineInfo(cell, context);
                }
                else
                {
                    PositionalLineInfo(cell, cellWs);
                }

                currentRow.AddCell(cell);
            }
            else
            {
                cell.Text = cellContent.GetText();

                currentCell = cell; 

                PositionalLineInfo(currentCell, cellContent);

                try
                {
                    Visit(cellContent);
                }
                finally
                {
                    currentCell = null;
                }

                currentRow.AddCell(cell);
            }

            return Result;
        }

        public override TableElement VisitCellWord([NotNull] AutoStepParser.CellWordContext context)
        {
            AddPart(CreatePart(context, (s, l) => new WordPart(s, l)));

            return Result!;
        }

        public override TableElement VisitCellInt([NotNull] AutoStepParser.CellIntContext context)
        {
            var intPart = CreatePart(context, (s, l) => new IntPart(s, l));

            AddPart(intPart);

            return Result!;
        }

        public override TableElement VisitCellFloat([NotNull] AutoStepParser.CellFloatContext context)
        {
            var floatPart = CreatePart(context, (s, l) => new FloatPart(s, l));

            AddPart(floatPart);

            return Result!;
        }

        public override TableElement VisitCellEscapedChar([NotNull] AutoStepParser.CellEscapedCharContext context)
        {
            var part = CreatePart(context, (s, l) => new WordPart(s, l));
            part.EscapedText = EscapeText(context, tableCellReplacements);

            AddPart(part);

            return Result!;
        }

        public override TableElement VisitCellVariable([NotNull] AutoStepParser.CellVariableContext context)
        {
            Debug.Assert(Result is object);
            Debug.Assert(currentCell is object);

            var variablePart = CreatePart(context, (s, l) => new VariablePart(s, l));

            variablePart.VariableName = context.cellVariableName().GetText();

            if (insertionNameValidator is object)
            {
                var additionalError = insertionNameValidator(context, variablePart.VariableName);

                if (additionalError is object)
                {
                    AddMessage(additionalError);
                }
            }

            AddPart(variablePart);

            return Result;
        }

        public override TableElement VisitCellColon([NotNull] AutoStepParser.CellColonContext context)
        {
            AddPart(CreatePart(context, (s, l) => new WordPart(s, l)));

            return Result!;
        }

        public override TableElement VisitCellInterpolate([NotNull] AutoStepParser.CellInterpolateContext context)
        {
            Debug.Assert(Result is object);

            // Interpolate part itself is just the colon.
            AddPart(CreatePart(context.CELL_COLON(), (s, l) => new InterpolatePart(s, l)));

            // Now add a part for the first word.
            AddPart(CreatePart(context.CELL_WORD(), (s, l) => new WordPart(s, l)));

            return Result;
        }

        private void AddPart(ContentPart part)
        {
            Debug.Assert(currentCell is object);

            currentCell.AddPart(part);
        }

        private TStepPart CreatePart<TStepPart>(ParserRuleContext ctxt, Func<int, int, TStepPart> creator)
            where TStepPart : ContentPart
        {
            Debug.Assert(currentCell is object);

            var offset = currentCell.SourceColumn;
            var start = (ctxt.Start.Column + 1) - offset;
            var startIndex = ctxt.Start.StartIndex;

            var part = creator(start, (ctxt.Stop.StopIndex - startIndex) + 1);

            PositionalLineInfo(part, ctxt);

            return part;
        }

        private TStepPart CreatePart<TStepPart>(ITerminalNode ctxt, Func<int, int, TStepPart> creator)
            where TStepPart : ContentPart
        {
            Debug.Assert(currentCell is object);

            var offset = currentCell.SourceColumn;
            var start = (ctxt.Symbol.Column + 1) - offset;
            var startIndex = ctxt.Symbol.StartIndex;

            var part = creator(start, (ctxt.Symbol.StopIndex - startIndex) + 1);

            PositionalLineInfo(part, ctxt);

            return part;
        }
    }
}
