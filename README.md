# _Chain of Responsibility_ and _Composite_ pattern combined

[![Build status](https://ci.appveyor.com/api/projects/status/dc5qp2litp7rt74k?svg=true)](https://ci.appveyor.com/project/sonbua/responsibilitychain) [![Nuget](https://img.shields.io/nuget/v/ResponsibilityChain.svg)](https://www.nuget.org/packages/ResponsibilityChain) ![netstandard](https://img.shields.io/badge/netstandard-1.1-green)

## Frameworks and platforms support
- .NET Core 1.0
- .NET Framework 4.5
- Mono 4.6
- Xamarin.iOS 10.0
- Xamarin.Mac 3.0
- Xamarin.Android 7.0
- Universal Windows Platform 10.0
- Unity 2018.1

## Usage

### Step 1: Declare

Handlers should implement `IHandler<TIn, TOut>` interface

```c#
public interface IHandler<TIn, TOut>
{
    TOut Handle(TIn input, Func<TIn, TOut> next);
}
```

Example

```c#
/// <summary>
///     Parses work log to minutes.
///     E.g. "30m" => 30 minutes
/// </summary>
public class MinuteParser : IHandler<string, int>
{
    private readonly Regex _pattern = new Regex("^(\\d+)m$");

    // implement IHandler<TIn, TOut>.Handle method
    public int Handle(string input, Func<string, int> next)
    {
        if (!_pattern.IsMatch(input))
        {
            // current handler cannot handle the input, so pass it to the next handler
            return next.Invoke(input);
        }

        // parse and return number of minutes
        // ...
        return minutes;
    }
}
```

### Step 2: Compose

A composite handler then extends `Handler<TIn, TOut>` abstract class and add child handlers via its constructor

```c#
public class WorkLogParser : Handler<string, int>
{
    public WorkLogParser(WorkLogValidator validator, IndividualUnitParser individualUnitParser)
    {
        AddHandler(validator);
        AddHandler(individualUnitParser);
    }
}
```

A composite handler can have as many nested handlers as needed. Support for deeply nested handlers is a natural progression.

```c#
var parser = new WorkLogParser(
    new WorkLogValidator(
        new WorkLogMustNotBeNullOrEmptyRule(),
        new ThereShouldBeNoUnitDuplicationRule(),
        new UnitsMustBeInDescendingOrderRule()
    ),
    new IndividualUnitParser(
        new WeekParser(),
        new DayParser(),
        new HourParser(),
        new MinuteParser()
    )
);
```

### Step 3: Execute

```c#
// work log in minutes
int workLog = parser.Handle("1w 2d 4h 30m");

Assert.Equal(3630, workLog);
```

## Notes

If the last handler in the chain cannot handle the input (and it passes the input to the next handler), the composite handler will throw an exception of type `NotSupportedException` by default. This can be made explicitly via chain's constructor

```c#
public class WorkLogParser : IHandler<string, int>
{
    public WorkLogParser(
        WorkLogValidator validator,
        IndividualUnitParser individualUnitParser,
        ThrowNotSupported<string, int> throwNotSupported)
    {
        AddHandler(validator);
        AddHandler(individualUnitParser);

        // explicitly tell the chain to use ThrowNotSupported as the last resort
        AddHandler(throwNotSupported);
    }
}
```

or via method invocation

```c#
var workLog = parser.Handle("1w 2d 4h 30m", new ThrowNotSupported<string, int>().Handle);
```

There are also other built-in last resort handlers
* `ReturnDefaultValue`
* `ReturnCompletedTask`
* `ReturnCompletedTaskWithDefaultValue`

## Asynchronous operation

For asynchronous operations, handlers should implement `IAsyncHandler<TIn, TOut>`

```c#
public interface IAsyncHandler<TIn, TOut> : IHandler
{
    Task<TOut> HandleAsync(TIn input, Func<TIn, CancellationToken, Task<TOut>> next, CancellationToken cancellationToken);
}
```