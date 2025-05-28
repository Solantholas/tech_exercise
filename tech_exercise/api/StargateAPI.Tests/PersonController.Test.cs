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

public class PersonControllerTests
{
    private static PersonController GetController(Mock<IMediator> mediatorMock = null)
    {
        mediatorMock ??= new Mock<IMediator>();
        var logger = Mock.Of<ILogger<PersonController>>();
        return new PersonController(mediatorMock.Object, logger);
    }

    [Fact]
    public async Task GetPeople_ReturnsOk_WhenPeopleExist()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync(new GetPeopleResult { Success = true, People = new() { new PersonAstronaut() } });

        var controller = GetController(mediator);

        var result = await controller.GetPeople();

        var resultObj = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<GetPeopleResult>(resultObj.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetPeople_ReturnsNotFound_WhenNoPeople()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
            .ReturnsAsync((GetPeopleResult)null);

        var controller = GetController(mediator);

        var result = await controller.GetPeople();

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, response.ResponseCode);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsOk_WhenPersonExists()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ReturnsAsync(new GetPersonByNameResult { Success = true, Person = new PersonAstronaut() });

        var controller = GetController(mediator);

        var result = await controller.GetPersonByName("John");

        var resultObj = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<GetPersonByNameResult>(resultObj.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var controller = GetController();

        var result = await controller.GetPersonByName("");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsNotFound_WhenPersonNotFound()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
            .ReturnsAsync(new GetPersonByNameResult { Success = false });

        var controller = GetController(mediator);

        var result = await controller.GetPersonByName("Nonexistent");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, response.ResponseCode);
    }

    [Fact]
    public async Task CreatePerson_ReturnsOk_WhenCreated()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ReturnsAsync(new CreatePersonResult { Success = true });

        var controller = GetController(mediator);

        var result = await controller.CreatePerson("John");

        var resultObj = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<CreatePersonResult>(resultObj.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreatePerson_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var controller = GetController();

        var result = await controller.CreatePerson("");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
    }

    [Fact]
    public async Task CreatePerson_ReturnsBadRequest_WhenCreationFails()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
            .ReturnsAsync(new CreatePersonResult { Success = false });

        var controller = GetController(mediator);

        var result = await controller.CreatePerson("John");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseResponse>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, response.ResponseCode);
    }
}