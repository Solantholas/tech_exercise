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
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<GetAstronautDutiesByNameHandler> _logger;

        public GetAstronautDutiesByNameHandler(StargateContext context, ILogger<GetAstronautDutiesByNameHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            GetAstronautDutiesByNameResult result = new GetAstronautDutiesByNameResult();

            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("GetAstronautDutiesByName called with null or empty name.");
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

                PersonAstronaut? person = await _context.People
                    .Where(p => p.Name == request.Name)
                    .Include(p => p.AstronautDetail)
                    .Select(p => new PersonAstronaut
                    {
                        PersonId = p.Id,
                        Name = p.Name,
                        CurrentRank = p.AstronautDetail.CurrentRank,
                        CurrentDutyTitle = p.AstronautDetail.CurrentDutyTitle,
                        CareerStartDate = p.AstronautDetail.CareerStartDate,
                        CareerEndDate = p.AstronautDetail.CareerEndDate
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                List<AstronautDuty> duties = new();

                if (person != null)
                {
                    duties = await _context.AstronautDuties
                        .Where(d => d.PersonId == person.PersonId)
                        .OrderByDescending(d => d.DutyStartDate)
                        .ToListAsync(cancellationToken);
                }

                if (person == null)
                {
                    _logger.LogWarning("No person found with name: {Name}", request.Name);
                    result.Success = false;
                    result.Message = $"Person '{request.Name}' not found.";
                    result.ResponseCode = 404;
                    result.Person = null;
                    result.AstronautDuties = new List<AstronautDuty>();
                    return result;
                }

                result.Person = person;
                result.AstronautDuties = duties;
                result.Success = true;
                result.Message = "Astronaut duties retrieved successfully.";
                result.ResponseCode = 200;

                _logger.LogInformation("Successfully retrieved astronaut duties for person: {Name}", request.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving astronaut duties for name: {Name}", request?.Name);
                result.Success = false;
                result.Message = "An error occurred while retrieving astronaut duties.";
                result.ResponseCode = 500;
                result.Person = null;
                result.AstronautDuties = new List<AstronautDuty>();
            }

            return result;
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
