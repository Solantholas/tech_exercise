using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

public class PersonQueriesTests
{
    private static DbContextOptions<StargateContext> GetSqliteInMemoryOptions(out SqliteConnection connection)
    {
        connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        return new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(connection)
            .Options;
    }

    [Fact]
    public async Task Handle_ReturnsAllPeople_WhenCalled()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean and seed database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        context.People.Add(new Person { Name = "John Doe" });
        context.People.Add(new Person { Name = "Jane Doe" });
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetPeopleHandler>>();
        var handler = new GetPeopleHandler(context, logger);

        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.People.Count);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoPeopleExist()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetPeopleHandler>>();
        var handler = new GetPeopleHandler(context, logger);

        var result = await handler.Handle(new GetPeople(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.People);
    }

    [Fact]
    public async Task Handle_ReturnsPerson_WhenPersonExists()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean and seed database before test
        context.People.RemoveRange(context.People);
        context.AstronautDetails.RemoveRange(context.AstronautDetails);
        context.SaveChanges();

        var person = new Person { Name = "John Doe" };
        context.People.Add(person);
        context.SaveChanges();

        // Add AstronautDetail if your handler expects it
        context.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "Commander",
            CurrentDutyTitle = "Pilot"
        });
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetPersonByNameHandler>>();
        var handler = new GetPersonByNameHandler(context, logger);

        var result = await handler.Handle(new GetPersonByName { Name = "John Doe" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Person);
        Assert.Equal("John Doe", result.Person.Name);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenPersonDoesNotExist()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetPersonByNameHandler>>();
        var handler = new GetPersonByNameHandler(context, logger);

        var result = await handler.Handle(new GetPersonByName { Name = "Nonexistent" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Person);
    }

    [Fact]
    public async Task Handle_ReturnsBadRequest_WhenNameIsNullOrEmpty()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        // Clean database before test
        context.People.RemoveRange(context.People);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetPersonByNameHandler>>();
        var handler = new GetPersonByNameHandler(context, logger);

        var result = await handler.Handle(new GetPersonByName { Name = "" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Person);
    }
}