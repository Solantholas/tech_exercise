using MediatR;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
        public required string NewName { get; set; } = string.Empty;
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;
        private readonly ILogger<UpdatePersonHandler> _logger;

        public UpdatePersonHandler(StargateContext context, ILogger<UpdatePersonHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            UpdatePersonResult result = new UpdatePersonResult();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.NewName))
                {
                    result.Success = false;
                    result.Message = "Both current name and new name must be provided.";
                    result.ResponseCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }

                Person? person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

                if (person == null)
                {
                    result.Success = false;
                    result.Message = $"Person '{request.Name}' not found.";
                    result.ResponseCode = (int)HttpStatusCode.NotFound;
                    _logger.LogWarning("Person not found for update: {Name}", request.Name);
                    return result;
                }
                
                person.Name = request.NewName;

                await _context.SaveChangesAsync(cancellationToken);

                result.OldName = request.Name;
                result.NewName = request.NewName;
                result.Success = true;
                result.Message = "Person name updated successfully.";
                result.ResponseCode = (int)HttpStatusCode.OK;
                _logger.LogInformation("Successfully updated person name from {OldName} to {NewName}", request.Name, request.NewName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating person name: {Name}", request?.Name);
                result.Success = false;
                result.Message = "An error occurred while updating the person name.";
                result.ResponseCode = (int)HttpStatusCode.InternalServerError;
            }

            return result;
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public string? OldName { get; set; }
        public string? NewName { get; set; }
    }

    public class UpdatePersonRequest
    {
        public string? Name { get; set; }
        public string? NewName { get; set; }
    }
}