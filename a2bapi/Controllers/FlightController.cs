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
        private readonly IAviationStackService _aviationStackService;

        public FlightController(IFlightsService flightsService, IAviationStackService aviationStackService)
        {
            _flightsService = flightsService;
            _aviationStackService = aviationStackService;
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

        [HttpGet("{from}-{to}")]
        public async Task<ActionResult<List<FlightSearchResultDto>>> Search(string from, string to, CancellationToken ct)
        {
            try
            {
                var results = await _aviationStackService.SearchFlightsAsync(from, to, ct);

                if (results.Count == 0)
                {
                    return Ok(new List<FlightSearchResultDto>());
                }
                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to search flights.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSavedFlight(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing claim." });
            }

            try
            {
                var deletedId = await _flightsService.DeleteSavedFlightAsync(userId, id);
                return Ok(new { message = "Flight deleted successfully", id = deletedId });
            } 
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }
    }
}
