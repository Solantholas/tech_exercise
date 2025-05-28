using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using System.Data;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<GetPersonByNameHandler> _logger;

        public GetPersonByNameHandler(StargateContext context, ILogger<GetPersonByNameHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            GetPersonByNameResult result = new GetPersonByNameResult();

            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("GetPersonByName called with null or empty name.");
                result.Success = false;
                result.Message = "Name must be provided.";
                result.ResponseCode = 400;
                return result;
            }

            try
            {
                if (_context.Connection == null)
                {
                    throw new InvalidOperationException("Relational database connection is required.");
                }

                PersonAstronaut? personResult = await _context.People
                    .Where(p => p.Name == request.Name)
                    .Select(p => new PersonAstronaut
                    {
                        PersonId = p.Id,
                        Name = p.Name,
                        CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : null,
                        CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : null,
                        CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                        CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                result.Person = personResult;

                if (result.Person == null)
                {
                    _logger.LogWarning("No person found with name: {Name}", request.Name);
                    result.Success = false;
                    result.Message = $"Person '{request.Name}' not found.";
                    result.ResponseCode = 404;
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved person with name: {Name}", request.Name);
                    result.Success = true;
                    result.Message = "Person found.";
                    result.ResponseCode = 200;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving person with name: {Name}", request?.Name);
                result.Success = false;
                result.Message = "An error occurred while retrieving the person.";
                result.ResponseCode = 500;
            }

            return result;
        }
    }
    
    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
