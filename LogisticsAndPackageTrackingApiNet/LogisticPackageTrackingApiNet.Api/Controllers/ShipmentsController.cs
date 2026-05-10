using LogisticPackageTrackingApiNet.Application.DTOs;
using LogisticPackageTrackingApiNet.Application.Features.Shipments.Handlers;
using LogisticPackageTrackingApiNet.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogisticPackageTrackingApiNet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentHandler _shipmentHandler;

    public ShipmentsController(IShipmentHandler shipmentHandler)
    {
        _shipmentHandler = shipmentHandler;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShipmentDTO>>>> GetAll()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        IEnumerable<ShipmentDTO> result;
        if (isAdmin)
        {
            result = await _shipmentHandler.GetAllShipments();
        }
        else if (!string.IsNullOrEmpty(userEmail))
        {
            result = await _shipmentHandler.GetAllShipmentsByMailAsync(userEmail);
        }
        else
        {
            result = await _shipmentHandler.GetAllShipments();
        }

        return Ok(ApiResponse<IEnumerable<ShipmentDTO>>.SuccessResponse(result, "Shipments retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ShipmentDTO>>> Create(CreateShipmentDTO dto)
    {
        var result = await _shipmentHandler.CreateShipment(dto);
        return Ok(ApiResponse<ShipmentDTO>.SuccessResponse(result!, "Shipment created successfully"));
    }

    [AllowAnonymous]
    [HttpGet("{trackingNumber}")]
    public async Task<ActionResult<ApiResponse<ShipmentDTO>>> GetByTrackingNumber(string trackingNumber)
    {
        var result = await _shipmentHandler.GetShipmentByTrackingNumber(trackingNumber);
        if (result == null) return NotFound(ApiResponse<ShipmentDTO>.FailureResponse("Shipment not found"));
        return Ok(ApiResponse<ShipmentDTO>.SuccessResponse(result));
    }

    [HttpPut("{trackingNumber}/status")]
    public async Task<ActionResult<ApiResponse<ShipmentDTO>>> UpdateStatus(string trackingNumber, UpdateShipmentStatusDTO dto)
    {
        var result = await _shipmentHandler.UpdateStatus(trackingNumber, dto);
        if (result == null) return NotFound(ApiResponse<ShipmentDTO>.FailureResponse("Shipment not found"));
        return Ok(ApiResponse<ShipmentDTO>.SuccessResponse(result, "Status updated successfully"));
    }
}
