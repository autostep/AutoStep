using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements;
using AutoStep.Language;
using AutoStep.Language.Position;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Language.Position
{
    public class PositionIndexTests
    {
        [Fact]
        public void ConstructorThrowsOnNegativeLines()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PositionIndex(-1));
        }

        [Fact]
        public void LookupReturnsPositionNoScopes()
        {
            var index = new PositionIndex(1);
            index.AddLineToken(1, 2, 4, new DummyElement("1st"), LineTokenCategory.EntryMarker);
            index.AddLineToken(1, 5, 8, new DummyElement("2nd"), LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
            index.Seal();

            var pos = index.Lookup(1, 6);
            pos.ClosestPrecedingTokenIndex.Should().Be(0);
            pos.CursorTokenIndex.Should().Be(1);
            pos.Token!.Category.Should().Be(LineTokenCategory.EntityName);
            pos.Token.SubCategory.Should().Be(LineTokenSubCategory.Feature);
            pos.Token.AttachedElement.Should().BeOfType<DummyElement>().Subject.Value.Should().Be("2nd");
            pos.CurrentScope.Should().BeNull();
            pos.Scopes.Should().HaveCount(0);
            pos.LineTokens.Should().HaveCount(2);
            pos.Line.Should().Be(1);
            pos.Column.Should().Be(6);
        }

        [Fact]
        public void LookupReturnsTokenWithScopes()
        {
            var index = new PositionIndex(5);
            index.PushScope(new DummyElement("file"), 1);
            index.AddLineToken(1, 2, 4, LineTokenCategory.EntryMarker);
            index.PushScope(new DummyElement("inner"), 3);
            index.AddLineToken(3, 5, 8, LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
            index.PopScope(4);
            index.PopScope(5);
            index.Seal();

            var pos = index.Lookup(3, 6);
            pos.CurrentScope.Should().BeOfType<DummyElement>().Subject.Value.Should().Be("inner");
            ((DummyElement)pos.Scopes[0]).Value.Should().Be("inner");
            ((DummyElement)pos.Scopes[1]).Value.Should().Be("file");
        }

        [Fact]
        public void LookupReturnsClosestPreceding()
        {
            var index = new PositionIndex(1);
            index.AddLineToken(1, 2, 4, new DummyElement("1st"), LineTokenCategory.EntryMarker);
            index.AddLineToken(1, 7, 10, new DummyElement("2nd"), LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
            index.AddLineToken(1, 12, 15, new DummyElement("3rd"), LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
            index.Seal();

            var pos = index.Lookup(1, 11);

            pos.CursorTokenIndex.Should().BeNull();
            pos.Token.Should().BeNull();
            pos.ClosestPrecedingTokenIndex.Should().Be(1);
        }

        [Fact]
        public void LookupBeyondLineEndClosestPreceding()
        {
            var index = new PositionIndex(1);
            index.AddLineToken(1, 2, 4, new DummyElement("1st"), LineTokenCategory.EntryMarker);
            index.AddLineToken(1, 7, 10, new DummyElement("2nd"), LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
            index.AddLineToken(1, 12, 15, new DummyElement("3rd"), LineTokenCategory.EntityName, LineTokenSubCategory.Feature);
            index.Seal();

            var pos = index.Lookup(1, 20);

            pos.CursorTokenIndex.Should().BeNull();
            pos.Token.Should().BeNull();
            pos.ClosestPrecedingTokenIndex.Should().Be(2);
        }

        [Fact]
        public void UnsealedIndexLookupThrows()
        {
            var index = new PositionIndex(1);

            index.Invoking(i => i.Lookup(1, 1)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void BadLookupLineNumberThrows()
        {
            var index = new PositionIndex(1);
            index.Seal();

            index.Invoking(i => i.Lookup(0, 1)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void BadLookupColumnNumberThrows()
        {
            var index = new PositionIndex(1); 
            index.Seal();

            index.Invoking(i => i.Lookup(1, 0)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void LookupBeyondFileEndReturnsEmptyInfo()
        {
            var index = new PositionIndex(1);
            index.Seal();

            var pos = index.Lookup(2, 1);
            pos.LineTokens.Should().BeEmpty();
            pos.Scopes.Should().BeEmpty();
            pos.ClosestPrecedingTokenIndex.Should().BeNull();
        }

        [Fact]
        public void AddLineTokenToSealedThrows()
        {
            var index = new PositionIndex(1);
            index.Seal();

            index.Invoking(i => i.AddLineToken(1, 1, 1, null, LineTokenCategory.EntityName)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void AddLineTokenZeroLine()
        {
            var index = new PositionIndex(1);

            index.Invoking(i => i.AddLineToken(0, 1, 1, null, LineTokenCategory.EntityName)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddLineTokenLineBeyondFile()
        {
            var index = new PositionIndex(1);

            index.Invoking(i => i.AddLineToken(2, 1, 1, null, LineTokenCategory.EntityName)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddLineTokenLineOutOfOrder()
        {
            var index = new PositionIndex(1);
            index.AddLineToken(1, 5, 5, LineTokenCategory.EntityName);

            index.Invoking(i => i.AddLineToken(1, 2, 2, null, LineTokenCategory.EntityName)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void PushScopeSealedIndexThrows()
        {
            var index = new PositionIndex(1);
            index.Seal();

            index.Invoking(i => i.PushScope(new DummyElement("123"), 1)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void PushScopeBadLineNumberThrows()
        {
            var index = new PositionIndex(1);

            index.Invoking(i => i.PushScope(new DummyElement("123"), 0)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void PushScopeLineNumberBeyondEndThrows()
        {
            var index = new PositionIndex(1);

            index.Invoking(i => i.PushScope(new DummyElement("123"), 2)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void PushScopePopulatesBlankLinesSincePrecedingScope()
        {
            var index = new PositionIndex(10);

            index.PushScope(new DummyElement("outer"), 2);

            index.PushScope(new DummyElement("inner"), 5);

            index.PopScope(8);

            index.PopScope(10);

            index.Seal();

            var pos = index.Lookup(9, 1);

            pos.CurrentScope.Should().BeOfType<DummyElement>().Subject.Value.Should().Be("outer");
        }

        private class DummyElement : BuiltElement
        {
            public DummyElement(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
}
