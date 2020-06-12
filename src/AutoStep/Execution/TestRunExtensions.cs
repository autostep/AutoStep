using System;
using AutoStep.Execution.Results;

namespace AutoStep.Execution
{
    /// <summary>
    /// Extension methods to assist in setting up a test run.
    /// </summary>
    public static class TestRunExtensions
    {
        /// <summary>
        /// Add the default results collector, which loads all registrations of <see cref="IResultsExporter"/> at the end of
        /// the run and provides it the complete result set.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        public static void AddDefaultResultsCollector(this TestRun testRun)
        {
            if (testRun is null)
            {
                throw new ArgumentNullException(nameof(testRun));
            }

            testRun.Events.Add(new ExportableResultsCollector());
        }
    }
}
