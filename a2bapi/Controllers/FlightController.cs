using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using a2bapi.Services;
using a2bapi.Dtos.Flights;
using Microsoft.AspNetCore.Authorization;

namespace a2bapi.Controllers
{
    [ApiController]
    [Route("api/flights")]
    public class FlightController : ControllerBase
    {
        private readonly IFlightsService _flightsService;

        public FlightController(IFlightsService flightsService)
        {
            _flightsService = flightsService;
        }

        [Authorize]
        [HttpGet("saved")]
        public async Task<ActionResult<SavedFlightDto>> GetSavedFlights()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing claim." });
            }

            try
            {
                var savedFlights = await _flightsService.GetSavedFlightsAsync(userId);
                return Ok(savedFlights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch saved flights.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("save")]
        public async Task<IActionResult> SaveFlight([FromBody] SaveFlightRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing claim." });
            }

            try
            {
                var savedId = await _flightsService.SaveFlightAsync(userId, request);
                return Ok(new { message = "Flight saved successfully.", id = savedId });
            }
            catch (Exception ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
