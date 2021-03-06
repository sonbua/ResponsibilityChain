﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnsureThat;
using FluentAssertions;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class HandlerTest
    {
        public class given_a_composite_handler_with_no_child_handler : HandlerTest
        {
            private readonly CompositeHandlerWithNoChild _handler;

            public given_a_composite_handler_with_no_child_handler()
            {
                _handler = new CompositeHandlerWithNoChild();
            }

            public class when_invoking_with_a_next_delegate : given_a_composite_handler_with_no_child_handler
            {
                private readonly int _result;

                public when_invoking_with_a_next_delegate()
                {
                    _result = _handler.Handle(string.Empty, _ => 0);
                }

                [Fact]
                public void then_passes_through_this_empty_composite_handler()
                {
                    // arrange

                    // act

                    // assert
                    _result.Should().Be(0);
                }
            }

            public class when_invoking_without_next_delegate_argument : given_a_composite_handler_with_no_child_handler
            {
                private readonly Action _action;

                public when_invoking_without_next_delegate_argument()
                {
                    _action = () => _handler.Handle(string.Empty);
                }

                [Fact]
                public void then_throws_NotSupportedException()
                {
                    // arrange

                    // act

                    // assert
                    _action.Should().Throw<NotSupportedException>();
                }
            }

            private class CompositeHandlerWithNoChild : Handler<string, int>
            {
            }
        }

        public class given_a_composite_work_log_parser_with_children : HandlerTest
        {
            private readonly WorkLogParser _parser;

            public given_a_composite_work_log_parser_with_children()
            {
                _parser = new WorkLogParser(
                    new WorkLogValidator(
                        new WorkLogMustNotBeNullOrEmptyRule(),
                        new ThereShouldBeNoUnitDuplicationRule(),
                        new UnitsMustBeInDescendingOrderRule()
                    ),
                    new IndividualUnitParser(
                        new HourParser(),
                        new MinuteParser()
                    )
                );
            }

            [Theory]
            [InlineData("20m", 20)]
            [InlineData("1h", 60)]
            [InlineData("2h 10m", 130)]
            public void GivenTextWithUnit_ReturnsCorrectNumberOfMinutes(string workLog, int expected)
            {
                // arrange

                // act
                var actual = _parser.Handle(workLog, null);

                // assert
                Assert.Equal(expected, actual);
            }

            [Theory]
            [InlineData("30m 10m")]
            [InlineData("1h 2h")]
            public void GivenUnitDuplication_ThrowsException(string workLog)
            {
                // arrange

                // act
                Action testDelegate = () => _parser.Handle(workLog, null);

                // assert
                Assert.Throws<ArgumentException>(testDelegate);
            }

            [Theory]
            [InlineData("30m 1h")]
            public void GivenUnitsInAscendingOrder_ThrowsException(string workLog)
            {
                // arrange

                // act
                Action testDelegate = () => _parser.Handle(workLog, null);

                // assert
                Assert.Throws<ArgumentException>(testDelegate);
            }

            [Theory]
            [InlineData("1d")]
            public void GivenUnknownUnit_ThrowsNotSupportedException(string workLog)
            {
                // arrange

                // act
                Action testDelegate = () => _parser.Handle(workLog, null);

                // assert
                Assert.Throws<NotSupportedException>(testDelegate);
            }

            private class WorkLogParser : Handler<string, int>, IWorkLogParser
            {
                public WorkLogParser(WorkLogValidator validator, IndividualUnitParser parser)
                {
                    AddHandler(validator);
                    AddHandler(parser);
                }
            }

            private class WorkLogValidator : Handler<string, int>, IWorkLogParser
            {
                public WorkLogValidator(
                    WorkLogMustNotBeNullOrEmptyRule workLogMustNotBeNullOrEmptyRule,
                    ThereShouldBeNoUnitDuplicationRule thereShouldBeNoUnitDuplicationRule,
                    UnitsMustBeInDescendingOrderRule unitsMustBeInDescendingOrderRule)
                {
                    AddHandler(workLogMustNotBeNullOrEmptyRule);
                    AddHandler(thereShouldBeNoUnitDuplicationRule);
                    AddHandler(unitsMustBeInDescendingOrderRule);
                }
            }

            private class WorkLogMustNotBeNullOrEmptyRule : IWorkLogParser
            {
                public int Handle(string input, Func<string, int> next)
                {
                    EnsureArg.IsNotNullOrEmpty(input, nameof(input));

                    return next.Invoke(input);
                }
            }

            private class ThereShouldBeNoUnitDuplicationRule : IWorkLogParser
            {
                public int Handle(string input, Func<string, int> next)
                {
                    var duplicatedUnitGrouping =
                        input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Last())
                            .GroupBy(x => x)
                            .FirstOrDefault(g => g.Count() > 1);

                    if (duplicatedUnitGrouping != null)
                    {
                        throw new ArgumentException("Duplicate unit: " + duplicatedUnitGrouping.Key);
                    }

                    return next.Invoke(input);
                }
            }

            private class UnitsMustBeInDescendingOrderRule : IWorkLogParser
            {
                private static readonly Dictionary<char, int> UnitOrderMap;

                static UnitsMustBeInDescendingOrderRule()
                {
                    UnitOrderMap = new Dictionary<char, int> {{'d', 3}, {'h', 2}, {'m', 1}};
                }

                public int Handle(string input, Func<string, int> next)
                {
                    var units = input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Last());
                    var unitOrders = units.Select(unit => UnitOrderMap[unit]).ToList();

                    for (var i = 0; i < unitOrders.Count - 1; i++)
                    {
                        if (unitOrders[i] < unitOrders[i + 1])
                        {
                            throw new ArgumentException("Unit must be in descending order: " + unitOrders[i + 1]);
                        }
                    }

                    return next.Invoke(input);
                }
            }

            private class IndividualUnitParser : Handler<string, int>, IWorkLogParser
            {
                public IndividualUnitParser(HourParser hourParser, MinuteParser minuteParser)
                {
                    AddHandler(hourParser);
                    AddHandler(minuteParser);
                }

                public override int Handle(string input, Func<string, int> next)
                {
                    return input.Split(' ').Select(piece => base.Handle(piece, next)).Sum();
                }
            }

            private class HourParser : IWorkLogParser
            {
                private readonly Regex _pattern = new Regex("^(\\d+)h$");

                public int Handle(string input, Func<string, int> next)
                {
                    if (!_pattern.IsMatch(input))
                    {
                        return next.Invoke(input);
                    }

                    var match = _pattern.Match(input);
                    var hourAsText = match.Groups[1].Value;

                    return (int) Math.Round(double.Parse(hourAsText) * 60);
                }
            }

            private class MinuteParser : IWorkLogParser
            {
                private readonly Regex _pattern = new Regex("^(\\d+)m$");

                public int Handle(string input, Func<string, int> next)
                {
                    if (!_pattern.IsMatch(input))
                    {
                        return next.Invoke(input);
                    }

                    var match = _pattern.Match(input);
                    var minuteAsText = match.Groups[1].Value;

                    return int.Parse(minuteAsText);
                }
            }

            private interface IWorkLogParser : IHandler<string, int>
            {
            }
        }
    }
}