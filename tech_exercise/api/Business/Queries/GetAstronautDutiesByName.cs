using Dapper;
using MediatR;
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
            var result = new GetAstronautDutiesByNameResult();

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

                var query = @"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate 
                              FROM [Person] a 
                              LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id 
                              WHERE a.Name = @Name";

                var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query, new { Name = request.Name });

                List<AstronautDuty> duties = new();

                if (person != null)
                {
                    query = @"SELECT * FROM [AstronautDuty] WHERE PersonId = @PersonId ORDER BY DutyStartDate DESC";
                    var dutiesResult = await _context.Connection.QueryAsync<AstronautDuty>(query, new { PersonId = person.PersonId });
                    duties = dutiesResult?.ToList() ?? new List<AstronautDuty>();
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
