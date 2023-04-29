using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Stef.Validation;

namespace OpenAI_API.Logging;

internal class DebugLogger : ILogger
{
    private readonly string _categoryName;

    public DebugLogger(string categoryName)
    {
        _categoryName = Guard.NotNullOrWhiteSpace(categoryName);
    }

    public IDisposable? BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        Debug.WriteLine($"{logLevel}: {_categoryName}[{eventId}]: {message}");

        if (exception != null)
        {
            Debug.WriteLine(exception.ToString());
        }
    }
}