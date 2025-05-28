using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreatePersonPreProcessor> _logger;

        public CreatePersonPreProcessor(StargateContext context, ILogger<CreatePersonPreProcessor> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                _logger.LogWarning("CreatePersonPreProcessor received invalid request: {@Request}", request);
                throw new BadHttpRequestException("Name must be provided.");
            }

            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null)
            {
                _logger.LogWarning("Attempt to create duplicate person with name: {Name}", request.Name);
                throw new BadHttpRequestException("Person already exists.");
            }

            return Task.CompletedTask;
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<CreatePersonHandler> _logger;

        public CreatePersonHandler(StargateContext context, ILogger<CreatePersonHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            var result = new CreatePersonResult();

            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("CreatePersonHandler received invalid request: {@Request}", request);
                    result.Success = false;
                    result.Message = "Name must be provided.";
                    result.ResponseCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }

                var newPerson = new Person()
                {
                    Name = request.Name
                };

                await _context.People.AddAsync(newPerson, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                result.Id = newPerson.Id;
                result.Success = true;
                result.Message = "Person created successfully.";
                result.ResponseCode = (int)HttpStatusCode.OK;
                _logger.LogInformation("Successfully created person with name: {Name}, Id: {Id}", newPerson.Name, newPerson.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating person with name: {Name}", request?.Name);
                result.Success = false;
                result.Message = "An error occurred while creating the person.";
                result.ResponseCode = (int)HttpStatusCode.InternalServerError;
            }

            return result;
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
