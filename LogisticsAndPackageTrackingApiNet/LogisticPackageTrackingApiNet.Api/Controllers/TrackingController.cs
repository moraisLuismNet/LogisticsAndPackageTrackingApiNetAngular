using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Features.Tracking.Handlers;
using LogisticPackageTrackingApiNet.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace LogisticPackageTrackingApiNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly ITrackingHandler _trackingHandler;

    public TrackingController(ITrackingHandler trackingHandler)
    {
        _trackingHandler = trackingHandler;
    }

    [HttpPost("{shipmentId}/updates")]
    public async Task<ActionResult<ApiResponse<TrackingUpdateDTO>>> AddUpdate(int shipmentId, [FromBody] AddTrackingUpdateDTO dto)
    {
        // Automatically derive location from IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _trackingHandler.AddTrackingUpdate(shipmentId, null, dto.Description, ipAddress);
        
        if (result == null) return NotFound(ApiResponse<TrackingUpdateDTO>.FailureResponse("Shipment not found"));
        return Ok(ApiResponse<TrackingUpdateDTO>.SuccessResponse(result, "Tracking update added successfully"));
    }

    [HttpGet("{shipmentId}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TrackingUpdateDTO>>>> GetHistory(int shipmentId)
    {
        var result = await _trackingHandler.GetHistory(shipmentId);
        return Ok(ApiResponse<IEnumerable<TrackingUpdateDTO>>.SuccessResponse(result));
    }
}
