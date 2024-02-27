using MediatR;
using Moq;
using Moq.AutoMock;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace StargateUnitTest.Controllers;

[TestClass]
public class AstronautDutyControllerTests
{
    private AutoMocker _mocker;
    private Mock<IMediator> _mediator;
    private AstronautDutyController _astronautDutyController;

    [TestInitialize]
    public void SetUp()
    {
        _mocker = new AutoMocker();
        _mediator = _mocker.GetMock<IMediator>();
        _astronautDutyController = _mocker.CreateInstance<AstronautDutyController>();
    }

    [TestMethod]
    public async Task GetAstronautDutiesByName_NoError()
    {
        var expectedResult = new GetAstronautDutiesByNameResult()
        {
            Person = new PersonAstronaut { PersonId = 1, Name = "Marissa" },

        };

        _mediator.Setup(x => x.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ReturnsAsync(expectedResult);

        var response = await _astronautDutyController.GetAstronautDutiesByName("Marissa") as ObjectResult;

        _mediator.Verify(x => x.Send(It.IsAny<GetAstronautDutiesByName>(), default), Times.Once);
        response?.Value.Should().BeEquivalentTo(expectedResult);
    }

    [TestMethod]
    public async Task GetPeople_Error()
    {
        var expectedResult = new BaseResponse
        {
            Message = "Bad Request",
            Success = false,
            ResponseCode = (int)HttpStatusCode.InternalServerError,
        };

        _mediator.Setup(x => x.Send(It.IsAny<GetAstronautDutiesByName>(), default))
            .ThrowsAsync(new BadHttpRequestException("Bad Request"));

        var response = await _astronautDutyController.GetAstronautDutiesByName("Marissa") as ObjectResult;

        _mediator.Verify(x => x.Send(It.IsAny<GetAstronautDutiesByName>(), default), Times.Once);
        response?.Value.Should().BeEquivalentTo(expectedResult);
    }
}