using Microsoft.Extensions.Logging;
using PortalContracts;

namespace PortalEngine;

public class PortalEngineService : IPortalEngine
{
    private readonly ILogger<PortalEngineService> _logger;

    public PortalEngineService(ILogger<PortalEngineService> logger)
    {
        _logger = logger;
    }

    public PortalResponse Process(PortalRequest request)
    {
        _logger.LogInformation("Running engine process for customer {CustomerId} and action {Action}", request.CustomerId, request.Action);

        var data = new PortalData(
            Id: Guid.NewGuid().ToString("D"),
            Name: $"Portal-{request.CustomerId}",
            Status: request.Action switch
            {
                "Create" => "Created",
                "Update" => "Updated",
                "Delete" => "Deleted",
                _ => "Processed"
            });

        return new PortalResponse(
            Success: true,
            Message: $"Request for {request.CustomerId} processed successfully.",
            Data: data,
            Timestamp: DateTime.UtcNow);
    }
}
