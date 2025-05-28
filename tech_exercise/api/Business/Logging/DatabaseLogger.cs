using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Data;

public class DatabaseLogger : ILogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _categoryName;

    public DatabaseLogger(IServiceScopeFactory scopeFactory, string categoryName)
    {
        _scopeFactory = scopeFactory;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Only log if the category is from Controllers, Commands, or Queries
        if (!(
            _categoryName.Contains("Controller") ||
            _categoryName.Contains("Commands") ||
            _categoryName.Contains("Queries")
        ))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StargateContext>();

        var entry = new LogEntry
        {
            Level = logLevel.ToString(),
            Message = formatter(state, exception),
            Exception = exception?.ToString(),
            Source = _categoryName,
            Timestamp = DateTime.UtcNow
        };
        
        context.LogEntries.Add(entry);
        context.SaveChanges();
    }
}