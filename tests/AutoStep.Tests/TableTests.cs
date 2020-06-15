using System;
using Autofac;
using AutoStep.Execution;
using AutoStep.Execution.Binding;
using AutoStep.Tests.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStep.Tests
{
    public class TableTests
    {
        [Fact]
        public void CanAccessRowList()
        {
            var srcTable = new TableBuilder(1, 1)
                            .Headers(1, 1, ("header1", 1, 1))
                            .Row(2, 1, ("value1", 2, 2, null))
                            .Row(3, 1, ("value2", 3, 3, null))
                            .Built;

            var table = new Table(srcTable, null!, VariableSet.Blank);

            table.Rows.Should().HaveCount(2);
        }

        [Fact]
        public void CanAccessCellValueByIdx()
        {
            var srcTable = new TableBuilder(1, 1)
                            .Headers(1, 1, ("header1", 1, 1), ("header2", 1, 1))
                            .Row(2, 1, ("h1.v1", 2, 2, null), ("h2.v1", 2, 2, null))
                            .Row(3, 1, ("h1.v2", 3, 3, null), ("h2.v2", 2, 2, null))
                            .Built;

            var mockScope = new Mock<ILifetimeScope>();

            var table = new Table(srcTable, mockScope.Object, VariableSet.Blank);

            table.Rows[0][0].Should().Be("h1.v1");
            table.Rows[1][0].Should().Be("h1.v2");
            table.Rows[0][1].Should().Be("h2.v1");
            table.Rows[1][1].Should().Be("h2.v2");
        }

        [Fact]
        public void CanAccessCellValueByHeader()
        {
            var srcTable = new TableBuilder(1, 1)
                            .Headers(1, 1, ("header1", 1, 1), ("header2", 1, 1))
                            .Row(2, 1, ("h1.v1", 2, 2, null), ("h2.v1", 2, 2, null))
                            .Row(3, 1, ("h1.v2", 3, 3, null), ("h2.v2", 2, 2, null))
                            .Built;

            var mockScope = new Mock<ILifetimeScope>();

            var table = new Table(srcTable, mockScope.Object, VariableSet.Blank);

            table.Rows[0]["header1"].Should().Be("h1.v1");
            table.Rows[1]["header1"].Should().Be("h1.v2");
            table.Rows[0]["header2"].Should().Be("h2.v1");
            table.Rows[1]["header2"].Should().Be("h2.v2");
        }

        [Fact]
        public void CellValueCanHaveVariable()
        {
            var srcTable = new TableBuilder(1, 1)
                            .Headers(1, 1, ("header1", 1, 1))
                            .Row(2, 1, ("<var1> text <var2>", 2, 2, c => c.Variable("var1").Text("text").Variable("var2")))
                            .Built;

            var mockScope = new Mock<ILifetimeScope>();

            var variableSet = new VariableSet();
            variableSet.Set("var1", "123");
            variableSet.Set("var2", "456");

            var table = new Table(srcTable, mockScope.Object, variableSet);

            table.Rows[0][0].Should().Be("123 text 456");
        }

        [Fact]
        public void NonExistentHeaderRaisesError()
        {
            var srcTable = new TableBuilder(1, 1)
                            .Headers(1, 1, ("header1", 1, 1), ("header2", 1, 1))
                            .Row(2, 1, ("h1.v1", 2, 2, null), ("h2.v1", 2, 2, null))
                            .Row(3, 1, ("h1.v2", 3, 3, null), ("h2.v2", 2, 2, null))
                            .Built;

            var mockScope = new Mock<ILifetimeScope>();

            var table = new Table(srcTable, mockScope.Object, VariableSet.Blank);

            table.Invoking(t => t.Rows[0]["notaheader"]).Should().Throw<ArgumentException>();
        }
    }
}
