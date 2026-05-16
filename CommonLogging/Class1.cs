using Microsoft.Extensions.Logging;

namespace CommonLogging;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddPortalLogging(this ILoggingBuilder builder)
    {
        builder.AddProvider(new PortalLoggerProvider());
        return builder;
    }
}

internal sealed class PortalLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new PortalLogger(categoryName);

    public void Dispose()
    {
    }
}

internal sealed class PortalLogger : ILogger
{
    private readonly string _categoryName;

    public PortalLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var output = $"[{DateTime.UtcNow:O}] {logLevel} [{_categoryName}] {message}";

        if (exception is not null)
            output += $" -- {exception}";

        Console.WriteLine(output);
    }
}
