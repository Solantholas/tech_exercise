using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Data.Sqlite;

public class AstronautDutyQueriesTests
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
    public async Task Handle_ReturnsBadRequest_WhenNameIsNullOrEmpty()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        context.People.RemoveRange(context.People);
        context.AstronautDetails.RemoveRange(context.AstronautDetails);
        context.AstronautDuties.RemoveRange(context.AstronautDuties);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetAstronautDutiesByNameHandler>>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);

        var request = new GetAstronautDutiesByName { Name = null };
        var result = await handler.Handle(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.ResponseCode);
        Assert.Equal("Name must be provided.", result.Message);
    }

    [Fact]
    public async Task Handle_ReturnsNotFound_WhenPersonDoesNotExist()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        context.People.RemoveRange(context.People);
        context.AstronautDetails.RemoveRange(context.AstronautDetails);
        context.AstronautDuties.RemoveRange(context.AstronautDuties);
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetAstronautDutiesByNameHandler>>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);

        var request = new GetAstronautDutiesByName { Name = "Nonexistent" };
        var result = await handler.Handle(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(404, result.ResponseCode);
        Assert.Equal($"Person 'Nonexistent' not found.", result.Message);
        Assert.Null(result.Person);
        Assert.Empty(result.AstronautDuties);
    }

    [Fact]
    public async Task Handle_ReturnsDuties_WhenPersonExists()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        context.People.RemoveRange(context.People);
        context.AstronautDetails.RemoveRange(context.AstronautDetails);
        context.AstronautDuties.RemoveRange(context.AstronautDuties);
        context.SaveChanges();

        var person = new Person { Name = "Jane Doe" };
        context.People.Add(person);
        context.SaveChanges();

        context.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "Major",
            CurrentDutyTitle = "Engineer",
            CareerStartDate = new DateTime(2020, 1, 1)
        });
        context.SaveChanges();

        context.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            Rank = "Major",
            DutyTitle = "Engineer",
            DutyStartDate = new DateTime(2020, 1, 1)
        });
        context.SaveChanges();

        var logger = Mock.Of<ILogger<GetAstronautDutiesByNameHandler>>();
        var handler = new GetAstronautDutiesByNameHandler(context, logger);

        var request = new GetAstronautDutiesByName { Name = "Jane Doe" };
        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(200, result.ResponseCode);
        Assert.NotNull(result.Person);
        Assert.Equal("Jane Doe", result.Person.Name);
        Assert.NotEmpty(result.AstronautDuties);
        Assert.Equal("Engineer", result.AstronautDuties[0].DutyTitle);
    }
}