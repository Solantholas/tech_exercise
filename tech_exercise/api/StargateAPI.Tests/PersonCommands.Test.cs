using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

public class PersonCommandsTests
{
    private static DbContextOptions<StargateContext> GetSqliteInMemoryOptions(out SqliteConnection connection)
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(connection)
            .Options;
    }

    [Fact]
    public async Task Handle_CreatesPerson_WhenValidRequest()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<CreatePersonHandler>>();
        var handler = new CreatePersonHandler(context, logger);

        var request = new CreatePerson { Name = "New Person" };
        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Person created successfully.", result.Message);
        Assert.NotEqual(0, result.Id);

        connection.Close();
    }

    [Fact]
    public async Task Process_ThrowsBadRequest_WhenDuplicateName()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean and seed database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        context.People.Add(new Person { Name = "Duplicate Person" });
        context.SaveChanges();

        var logger = Mock.Of<ILogger<CreatePersonPreProcessor>>();
        var preProcessor = new CreatePersonPreProcessor(context, logger);

        var request = new CreatePerson { Name = "Duplicate Person" };

        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(request, CancellationToken.None));

        connection.Close();
    }

    [Fact]
    public async Task Process_ThrowsBadRequest_WhenNameIsNull()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<CreatePersonPreProcessor>>();
        var preProcessor = new CreatePersonPreProcessor(context, logger);

        var request = new CreatePerson { Name = null };

        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(request, CancellationToken.None));

        connection.Close();
    }
}