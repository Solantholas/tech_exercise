using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using StargateAPI.Controllers;
using MediatR;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Commands;
using System.Net;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Data;

public class AstronautDutyControllerTests
{
    private static AstronautDutyController GetController(Mock<IMediator> mediatorMock = null)
    {
        mediatorMock ??= new Mock<IMediator>();
        var logger = Mock.Of<ILogger<AstronautDutyController>>();
        return new AstronautDutyController(mediatorMock.Object, logger);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsOk_WhenPersonFound()
    {
        var mediator = new Mock<IMediator>();
        var logger = Mock.Of<ILogger<AstronautDutyController>>();
        var expectedResult = new GetPersonByNameResult
        {
            Success = true,
            Person = new PersonAstronaut { PersonId = 1, Name = "John Doe" }
        };

        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ReturnsAsync(expectedResult);

        var controller = new AstronautDutyController(mediator.Object, logger);

        var result = await controller.GetAstronautDutiesByName("John Doe");

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(200, objectResult.StatusCode ?? 200);
        Assert.Same(expectedResult, objectResult.Value);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var controller = GetController();

        var result = await controller.GetAstronautDutiesByName("");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
    }

    [Fact]
    public async Task GetAstronautDutiesByName_ReturnsNotFound_WhenPersonNotFound()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ReturnsAsync(new GetAstronautDutiesByNameResult { Success = false });

        var controller = GetController(mediator);

        var result = await controller.GetAstronautDutiesByName("Nonexistent");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, response.ResponseCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsOk_WhenCreated()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(new CreateAstronautDutyResult { Success = true });

        var controller = GetController(mediator);

        var request = new CreateAstronautDuty { Name = "John", Rank = "Captain", DutyTitle = "Pilot", DutyStartDate = System.DateTime.UtcNow };
        var result = await controller.CreateAstronautDuty(request);

        var resultObj = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<CreateAstronautDutyResult>(resultObj.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsBadRequest_WhenRequestIsNull()
    {
        var controller = GetController();

        var result = await controller.CreateAstronautDuty(null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
    }

    [Fact]
    public async Task CreateAstronautDuty_ReturnsBadRequest_WhenCreationFails()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), default))
            .ReturnsAsync(new CreateAstronautDutyResult { Success = false });

        var controller = GetController(mediator);

        var request = new CreateAstronautDuty { Name = "John", Rank = "Captain", DutyTitle = "Pilot", DutyStartDate = System.DateTime.UtcNow };
        var result = await controller.CreateAstronautDuty(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
    }
}