using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Data;
using System;
using Microsoft.EntityFrameworkCore;

public class LoggerTests
{
    private static DbContextOptions<StargateContext> GetSqliteInMemoryOptions(out Microsoft.Data.Sqlite.SqliteConnection connection)
    {
        connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        return new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(connection)
            .Options;
    }

    [Fact]
    public void Can_Log_Information_To_Database()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();

        // Arrange
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = "Information",
            Message = "Test log entry",
            Exception = null
        };

        // Act
        context.LogEntries.Add(logEntry);
        context.SaveChanges();

        // Assert
        var saved = context.LogEntries.FirstOrDefaultAsync(l => l.Message == "Test log entry").Result;
        Assert.NotNull(saved);
        Assert.Equal("Information", saved.Level);

        connection.Close();
    }

    [Fact]
    public void Can_Log_Error_With_Exception_To_Database()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();

        // Arrange
        var logEntry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = "Error",
            Message = "Error occurred",
            Exception = "System.Exception: Test exception"
        };

        // Act
        context.LogEntries.Add(logEntry);
        context.SaveChanges();

        // Assert
        var saved = context.LogEntries.FirstOrDefaultAsync(l => l.Level == "Error").Result;
        Assert.NotNull(saved);
        Assert.Contains("Test exception", saved.Exception);

        connection.Close();
    }
}