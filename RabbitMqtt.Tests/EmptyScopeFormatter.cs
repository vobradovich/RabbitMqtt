using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;

namespace RabbitMqtt.Tests;

public class EmptyScopeFormatter : ILogFormatter
{
    public string Format(int scopeLevel, string categoryName, LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        return string.Empty;
    }
}