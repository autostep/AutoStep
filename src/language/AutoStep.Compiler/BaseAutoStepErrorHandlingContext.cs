using System;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{

    internal class BaseAutoStepErrorHandlingContext
    {
        public BaseAutoStepErrorHandlingContext(ITokenStream tokenStream, IRecognizer recognizer, IToken offendingSymbol, RecognitionException ex)
        {
            TokenStream = tokenStream;
            Parser = (AutoStepParser) recognizer;
            OffendingSymbol = offendingSymbol;
            Exception = ex;
            StartingSymbol = OffendingSymbol;
            EndingSymbol = OffendingSymbol;
            Code = CompilerMessageCode.SyntaxError;
        }

        protected void SetErrorHandlers(params Func<bool>[] handlers)
        {
            Handlers = handlers;
        }

        public void DoErrorMatching()
        {
            // Go through the handlers, stop on a match.
            foreach (var handler in Handlers)
            {
                if (handler())
                {
                    break;
                }
            }
        }

        public CompilerMessageCode Code { get; private set; }

        public IToken StartingSymbol { get; private set; }

        public IToken EndingSymbol { get; private set; }

        public bool IgnoreFollowingEndOfFileMessages { get; private set; }

        protected ITokenStream TokenStream { get; }

        protected AutoStepParser Parser { get; }

        protected IToken OffendingSymbol { get; }

        protected RecognitionException Exception { get; }

        protected RuleContext? Context => Parser?.Context;

        protected IntervalSet? GetExpectedTokens() => Exception?.GetExpectedTokens();

        protected bool ExceptionIs<TException>()
            where TException : Exception
        {
            return Exception is TException;
        }

        protected bool ContextIs<TContext>()
            where TContext : RuleContext
        {
            return Context is TContext;
        }

        protected void UseOpeningTokenAsStart(params int[] searchTypes)
        {
            // Search back for the preceding symbol that opened the item in question.
            StartingSymbol = TokenStream.GetPrecedingToken(OffendingSymbol, searchTypes);
        }

        protected void UsePrecedingTokenAsEnd()
        {
            // Get the preceding token for the offending new line
            EndingSymbol = TokenStream.GetPrecedingToken(OffendingSymbol);
        }

        protected void UseStartSymbolAsEndSymbol()
        {
            EndingSymbol = StartingSymbol;
        }

        protected bool ExpectingTokens(params int[] tokens)
        {
            return !Parser.GetExpectedTokens().Or(new IntervalSet(tokens)).IsNil;
        }

        protected void ChangeError(CompilerMessageCode code)
        {
            Code = code;
        }

        protected bool OffendingSymbolIs(int symbol)
        {
            return OffendingSymbol.Type == symbol;
        }

        protected bool OffendingSymbolIsNot(int symbol)
        {
            return !OffendingSymbolIs(symbol);
        }

        protected bool OffendingSymbolTextIs(string expectedText)
        {
            return OffendingSymbol.Text == expectedText;
        }

        protected void SwallowEndOfFileErrorsAfterThis()
        {
            IgnoreFollowingEndOfFileMessages = true;
        }

        private Func<bool>[] Handlers { get; set; }

    }
}
