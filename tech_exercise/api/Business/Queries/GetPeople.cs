using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {
    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<GetPeopleHandler> _logger;

        public GetPeopleHandler(StargateContext context, ILogger<GetPeopleHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            GetPeopleResult result = new GetPeopleResult();

            try
            {
                if (_context.Connection == null)
                {
                    throw new InvalidOperationException("Relational database connection is required.");
                }

                List<PersonAstronaut>? peopleList = await _context.People
                    .Select(p => new PersonAstronaut
                    {
                        PersonId = p.Id,
                        Name = p.Name,
                        CurrentRank = p.AstronautDetail != null ? p.AstronautDetail.CurrentRank : null,
                        CurrentDutyTitle = p.AstronautDetail != null ? p.AstronautDetail.CurrentDutyTitle : null,
                        CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                        CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null
                    })
                    .ToListAsync(cancellationToken);

                result.People = peopleList;

                if (result.People.Count == 0)
                {
                    _logger.LogWarning("No people found in the database.");
                    result.Success = false;
                    result.Message = "No people found.";
                    result.ResponseCode = 404;
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved {Count} people.", result.People.Count);
                    result.Success = true;
                    result.Message = "People retrieved successfully.";
                    result.ResponseCode = 200;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving people.");
                result.Success = false;
                result.Message = "An error occurred while retrieving people.";
                result.ResponseCode = 500;
                result.People = new List<PersonAstronaut>();
            }

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut>();
    }
}
