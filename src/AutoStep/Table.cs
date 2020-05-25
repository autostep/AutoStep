using System;
using System.Collections.Generic;
using System.Linq;
using AutoStep.Elements.Metadata;
using AutoStep.Execution;
using AutoStep.Execution.Binding;

namespace AutoStep
{
    /// <summary>
    /// Defines a bound table, provided to step definitions at runtime, containing the table data from a step.
    /// </summary>
    public class Table
    {
        private readonly IServiceProvider serviceProvider;
        private readonly VariableSet variables;

        private IReadOnlyList<TableRow>? cachedTableRows;
        private IReadOnlyList<string?>? cachedHeaderNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="tableInfo">The source table information from a compiled test file.</param>
        /// <param name="serviceProvider">A service provider.</param>
        /// <param name="variables">The set of in-scope variables.</param>
        public Table(ITableInfo tableInfo, IServiceProvider serviceProvider, VariableSet variables)
        {
            this.serviceProvider = serviceProvider;
            this.variables = variables;
            this.Source = tableInfo;
        }

        /// <summary>
        /// Gets the raw compiled source table.
        /// </summary>
        public ITableInfo Source { get; }

        /// <summary>
        /// Gets the set of headers in the table, in position order.
        /// </summary>
        public IReadOnlyList<string?> Headers => cachedHeaderNames ??= Source.Header.Headers.Select(x => x.HeaderName).ToList();

        /// <summary>
        /// Gets the set of rows in the table.
        /// </summary>
        public IReadOnlyList<TableRow> Rows => cachedTableRows ??= BindTableRows();

        private IReadOnlyList<TableRow> BindTableRows()
        {
            // First, bind the set of headers to a dictionary of column -> index.
            var headers = ExtractHeaderIndex();

            var results = new List<TableRow>();

            // Go through each row and bind the cells.
            foreach (var srcRow in Source.Rows)
            {
                var row = new TableRow(srcRow, serviceProvider, headers, variables);
                results.Add(row);
            }

            return results;
        }

        private Dictionary<string, int> ExtractHeaderIndex()
        {
            var headers = new Dictionary<string, int>();
            var rawHeaders = Source.Header.Headers;

            for (var headerIdx = 0; headerIdx < rawHeaders.Count; headerIdx++)
            {
                var rawHeader = rawHeaders[headerIdx];

                if (!string.IsNullOrEmpty(rawHeader.HeaderName))
                {
                    headers[rawHeader.HeaderName] = headerIdx;
                }
            }

            return headers;
        }
    }
}
