using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }
        public required string Rank { get; set; }
        public required string DutyTitle { get; set; }
        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public async Task<Task> Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            Person person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name)
                ?? throw new BadHttpRequestException("Bad Request");
                
            AstronautDuty? existingDuty = await _context.AstronautDuties
                .FirstOrDefaultAsync(d => d.PersonId == person.Id
                    && d.Rank == request.Rank
                    && d.DutyStartDate.Date == request.DutyStartDate.Date
                    && d.DutyTitle == request.DutyTitle, cancellationToken);

            if (existingDuty != null)
                throw new BadHttpRequestException("Astronaut duty already exists for this person and duty title.");

            return Task.CompletedTask;
        }

        Task IRequestPreProcessor<CreateAstronautDuty>.Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            return Process(request, cancellationToken);
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreateAstronautDutyHandler> _logger;

        public CreateAstronautDutyHandler(StargateContext context, ILogger<CreateAstronautDutyHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            CreateAstronautDutyResult result = new CreateAstronautDutyResult();

            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Rank) ||
                    string.IsNullOrWhiteSpace(request.DutyTitle) ||
                    request.DutyStartDate == default)
                {
                    _logger.LogWarning("Invalid CreateAstronautDuty request: {@Request}", request);
                    result.Success = false;
                    result.Message = "Invalid request data.";
                    result.ResponseCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }

                Person? person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

                if (person == null)
                {
                    _logger.LogWarning("Person not found with name: {Name}", request.Name);
                    result.Success = false;
                    result.Message = $"Person '{request.Name}' not found.";
                    result.ResponseCode = (int)HttpStatusCode.NotFound;
                    return result;
                }

                AstronautDetail? astronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(ad => ad.PersonId == person.Id, cancellationToken);

                if (astronautDetail == null)
                {
                    astronautDetail = new AstronautDetail
                    {
                        PersonId = person.Id,
                        CurrentDutyTitle = request.DutyTitle,
                        CurrentRank = request.Rank,
                        CareerStartDate = request.DutyStartDate.Date
                    };
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.Date;
                    }
                    await _context.AstronautDetails.AddAsync(astronautDetail);
                    _logger.LogInformation("Created new AstronautDetail for person: {Name}", request.Name);
                }
                else
                {
                    astronautDetail.CurrentDutyTitle = request.DutyTitle;
                    astronautDetail.CurrentRank = request.Rank;
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                    }
                    _context.AstronautDetails.Update(astronautDetail);
                    _logger.LogInformation("Updated AstronautDetail for person: {Name}", request.Name);
                }

                AstronautDuty? astronautDuty = await _context.AstronautDuties
                    .Where(ad => ad.PersonId == person.Id)
                    .OrderByDescending(ad => ad.DutyStartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (astronautDuty != null)
                {
                    astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                    _context.AstronautDuties.Update(astronautDuty);
                    _logger.LogInformation("Updated previous AstronautDuty end date for person: {Name}", request.Name);
                }

                AstronautDuty newAstronautDuty = new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = request.Rank,
                    DutyTitle = request.DutyTitle,
                    DutyStartDate = request.DutyStartDate.Date,
                    DutyEndDate = null
                };

                await _context.AstronautDuties.AddAsync(newAstronautDuty);

                await _context.SaveChangesAsync();

                result.Id = newAstronautDuty.Id;
                result.Success = true;
                result.Message = "Astronaut duty created successfully.";
                result.ResponseCode = (int)HttpStatusCode.OK;
                _logger.LogInformation("Successfully created new AstronautDuty for person: {Name}", request.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating astronaut duty for person: {Name}", request?.Name);
                result.Success = false;
                result.Message = "An error occurred while creating astronaut duty.";
                result.ResponseCode = (int)HttpStatusCode.InternalServerError;
            }

            return result;
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
