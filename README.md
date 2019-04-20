# _Chain of Responsibility_ and _Composite_ pattern combined

## Usage

### Step 1: Declare

Handlers should implement `IHandler<TIn, TOut>` interface

```
public interface IHandler<TIn, TOut>
{
    TOut Handle(TIn input, Func<TIn, TOut> next);
}
```

Example

```
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

Composite handler then extends Handler<TIn, TOut> and add child handlers via its constructor.

```
public class WorkLogParser : IHandler<string, int>
{
    public WorkLogParser(WorkLogValidator validator, IndividualUnitParser individualUnitParser)
    {
        AddHandler(validator);
        AddHandler(individualUnitParser);
    }
}
```

Composite handler can have deeply nested handlers as much as needed.

```
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

```
// work log in minutes
int workLog = parser.Handle("1w 2d 4h 30m", next: null);

Assert.Equal(3630, workLog);
```

## Notes

If the last handler in the chain cannot handle the input, the composite handler will throw an exception of type `NotSupportedException`. The chain can be explicitly configured via its constructor

```
public class WorkLogParser : IHandler<string, int>
{
    public WorkLogParser(WorkLogValidator validator, IndividualUnitParser individualUnitParser)
    {
        AddHandler(validator);
        AddHandler(individualUnitParser);
        AddHandler(ThrowNotSupportedHandler<string, int>.Instance);
    }
}
```

or via method invocation

```
var workLog = parser.Handle("1w 2d 4h 30m", ThrowNotSupportedHandler<string, int>.Instance);
```

There also are other built-in utility handlers:
* `ReturnDefaultValueHandler`
* `ReturnCompletedTaskHandler`
* `ReturnCompletedTaskFromDefaultValueHandler`