using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AstronautDutyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AstronautDutyController> _logger;

        public AstronautDutyController(IMediator mediator, ILogger<AstronautDutyController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetAstronautDutiesByName called with empty name.");
                return BadRequest(new BaseResponse
                {
                    Message = "Name must be provided.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                var result = await _mediator.Send(new GetPersonByName { Name = name });
                if (result == null)
                {
                    _logger.LogWarning("No astronaut duties found for name: {Name}", name);
                    return NotFound(new BaseResponse
                    {
                        Message = $"No astronaut duties found for '{name}'.",
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.NotFound
                    });
                }
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAstronautDutiesByName for {Name}", name);
                return this.GetResponse(new BaseResponse
                {
                    Message = "An error occurred while retrieving astronaut duties.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            if (request == null)
            {
                _logger.LogWarning("CreateAstronautDuty called with null request.");
                return BadRequest(new BaseResponse
                {
                    Message = "Request body must be provided.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                var result = await _mediator.Send(request);
                if (result == null || !result.Success)
                {
                    _logger.LogWarning("Failed to create astronaut duty for person: {Name}", request?.Name);
                    return BadRequest(new BaseResponse
                    {
                        Message = "Failed to create astronaut duty.",
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.BadRequest
                    });
                }
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAstronautDuty for {Name}", request?.Name);
                return this.GetResponse(new BaseResponse
                {
                    Message = "An error occurred while creating astronaut duty.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}