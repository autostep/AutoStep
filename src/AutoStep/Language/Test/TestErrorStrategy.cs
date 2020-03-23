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
        private readonly IntervalSet newLineConsumeSet = new IntervalSet(NEWLINE);

        /// <inheritdoc/>
        public override void Recover(Antlr4.Runtime.Parser recognizer, RecognitionException e)
        {
            if (e is NoViableAltException && recognizer.RuleContext.RuleIndex == RULE_fileEntity)
            {
                // Recovering from an error in the file entity context suggests that a Feature/Step could not be determined,
                // probably due to some junk in the file.
                // Let's skip ahead until we see any of the tokens that might be the start of the next bit.
                ConsumeUntil(recognizer, fileEntityRecoverySet);
            }
            else if (recognizer.RuleContext.RuleIndex == RULE_stepDeclaration)
            {
                // Problem parsing the step declaration. Progress to the next NEWLINE.
                ConsumeUntil(recognizer, newLineConsumeSet);
            }

            base.Recover(recognizer, e);
        }
    }
}
