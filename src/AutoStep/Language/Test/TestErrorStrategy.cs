using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Test.Parser;

namespace AutoStep.Language.Test
{
    using static AutoStepParser;

    /// <summary>
    /// Custom error recovery behaviour for the test language.
    /// </summary>
    internal class TestErrorStrategy : DefaultErrorStrategy
    {
        private readonly IntervalSet fileEntityRecoverySet = new IntervalSet(TAG, OPTION, FEATURE, STEP_DEFINE);

        /// <inheritdoc/>
        public override void Recover(Antlr4.Runtime.Parser recognizer, RecognitionException e)
        {
            if (recognizer.Context is FileEntityContext && e is NoViableAltException)
            {
                // Recovering from an error in the file entity context suggests that a Feature/Step could not be determined,
                // probably due to some junk in the file.
                // Let's skip ahead until we see any of the tokens that might be the start of the next bit.
                ConsumeUntil(recognizer, fileEntityRecoverySet);
            }

            base.Recover(recognizer, e);
        }
    }
}
