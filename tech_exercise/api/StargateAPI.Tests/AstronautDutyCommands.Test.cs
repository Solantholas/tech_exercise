using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Data.Sqlite;

public class Astronaut
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
    public async Task Handle_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        var logger = Mock.Of<ILogger<CreateAstronautDutyHandler>>();
        var handler = new CreateAstronautDutyHandler(context, logger);

        var request = new CreateAstronautDuty
        {
            Name = "", // Invalid
            Rank = "",
            DutyTitle = "",
            DutyStartDate = default
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(400, result.ResponseCode);
        Assert.Equal("Invalid request data.", result.Message);

        connection.Close();
    }

    [Fact]
    public async Task Process_ThrowsBadRequest_WhenPersonNotFound()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();
        var logger = Mock.Of<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(context);

        var request = new CreateAstronautDuty
        {
            Name = "Nonexistent",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2024, 1, 1)
        };

        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(request, CancellationToken.None));

        connection.Close();
    }

    [Fact]
    public async Task Process_ThrowsBadRequest_WhenDutyAlreadyExists()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();

        var fixedDate = new DateTime(2024, 1, 1);

        context.People.Add(new Person { Id = 1, Name = "John Doe" });
        context.AstronautDuties.Add(new AstronautDuty
        {
            Id = 1,
            PersonId = 1,
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = fixedDate
        });
        context.SaveChanges();

        var logger = Mock.Of<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(context);

        var request = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = fixedDate
        };

        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(request, CancellationToken.None));

        connection.Close();
    }

    [Fact]
    public async Task Handle_CreatesDuty_WhenRequestIsValid()
    {
        var options = GetSqliteInMemoryOptions(out var connection);
        using var context = new StargateContext(options);
        context.Database.EnsureCreated();

        context.People.Add(new Person { Id = 1, Name = "John Doe" });
        context.SaveChanges();

        var logger = Mock.Of<ILogger<CreateAstronautDutyHandler>>();
        var handler = new CreateAstronautDutyHandler(context, logger);

        var fixedDate = new DateTime(2024, 1, 2);

        var request = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = fixedDate
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.Equal(200, result.ResponseCode);

        connection.Close();
    }
}