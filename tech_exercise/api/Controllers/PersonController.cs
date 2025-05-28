using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PersonController> _logger;

        public PersonController(IMediator mediator, ILogger<PersonController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                GetPeopleResult result = await _mediator.Send(new GetPeople());
                if (result == null)
                {
                    _logger.LogWarning("No people found.");
                    return NotFound(new BaseResponse
                    {
                        Message = "No people found.",
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.NotFound
                    });
                }
                
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPeople");
                return this.GetResponse(new BaseResponse
                {
                    Message = "An error occurred while retrieving people.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetPersonByName called with empty name.");
                return BadRequest(new BaseResponse
                {
                    Message = "Name must be provided.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                GetPersonByNameResult result = await _mediator.Send(new GetPersonByName { Name = name });

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("Person not found: {Name}", name);
                    return NotFound(new BaseResponse
                    {
                        Message = $"Person '{name}' not found.",
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.NotFound
                    });
                }

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPersonByName for {Name}", name);
                return this.GetResponse(new BaseResponse
                {
                    Message = "An error occurred while retrieving the person.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("CreatePerson called with empty name.");
                return BadRequest(new BaseResponse
                {
                    Message = "Name must be provided.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                CreatePersonResult result = await _mediator.Send(new CreatePerson { Name = name });
                if (result == null || !result.Success)
                {
                    _logger.LogWarning("Failed to create person: {Name}", name);
                    return BadRequest(new BaseResponse
                    {
                        Message = $"Failed to create person '{name}'.",
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.BadRequest
                    });
                }

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePerson for {Name}", name);
                return this.GetResponse(new BaseResponse
                {
                    Message = "An error occurred while creating the person.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePersonByName([FromBody] UpdatePersonRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name) || request.NewName == null)
            {
                _logger.LogWarning("UpdatePersonByName called with invalid input.");
                return BadRequest(new BaseResponse
                {
                    Message = "Name and update data must be provided.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.BadRequest
                });
            }

            try
            {
                UpdatePersonResult result = await _mediator.Send(new UpdatePerson { Name = request.Name, NewName = request.NewName });
                if (result == null || !result.Success)
                {
                    _logger.LogWarning("Failed to update person: {Name}", request.Name);
                    return NotFound(new BaseResponse
                    {
                        Message = $"Person '{request.Name}' not found or update failed.",
                        Success = false,
                        ResponseCode = (int)HttpStatusCode.NotFound
                    });
                }

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePersonByName for {Name}", request.Name);
                return this.GetResponse(new BaseResponse
                {
                    Message = "An error occurred while updating the person.",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}