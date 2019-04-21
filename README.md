# _Chain of Responsibility_ and _Composite_ pattern combined

## Usage

### Step 1: Declare

Handlers should implement `IHandler<TIn, TOut>` interface

```cs
public interface IHandler<TIn, TOut>
{
    TOut Handle(TIn input, Func<TIn, TOut> next);
}
```

Example

```cs
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

A composite handler then extends `Handler<TIn, TOut>` and add child handlers via its constructor

```cs
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

```cs
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

```cs
// work log in minutes
int workLog = parser.Handle("1w 2d 4h 30m", next: null);

Assert.Equal(3630, workLog);
```

## Notes

If the last handler in the chain cannot handle the input (and it passes the input to the next handler), the composite handler will throw an exception of type `NotSupportedException` by default. This behavior can be explicitly configured via chain's constructor

```cs
public class WorkLogParser : IHandler<string, int>
{
    public WorkLogParser(WorkLogValidator validator, IndividualUnitParser individualUnitParser)
    {
        AddHandler(validator);
        AddHandler(individualUnitParser);
        // explicitly tell the chain to use ThrowNotSupportedHandler as the last resort
        AddHandler(ThrowNotSupportedHandler<string, int>.Instance);
    }
}
```

or via method invocation

```cs
var workLog = parser.Handle("1w 2d 4h 30m", ThrowNotSupportedHandler<string, int>.Instance);
```

There are also other built-in last resort handlers
* `ReturnDefaultValueHandler`
* `ReturnCompletedTaskHandler`
* `ReturnCompletedTaskFromDefaultValueHandler`