using Microsoft.AspNetCore.Mvc;
using a2bapi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using a2bapi.Dtos.Hotels;

namespace a2bapi.Controllers
{
    [ApiController]
    [Route("api/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelsService _hotelsService;

        public HotelController(IHotelsService hotelsService)
        {
            _hotelsService = hotelsService;
        }

        [Authorize]
        [HttpGet("saved")]
        public async Task<ActionResult<SavedHotelDto>> GetSavedHotels()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing claim." });
            }

            try
            {
                var savedHotels = await _hotelsService.GetSavedHotelsAsync(userId);
                return Ok(savedHotels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch saved hotels.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("save")]
        public async Task<IActionResult> SaveHotel([FromBody] SaveHotelRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid or missing claim." });
            }

            try
            {
                var savedId = await _hotelsService.SaveHotelAsync(userId, request);
                return Ok(new {message = "Hotel saved successfully.", id = savedId });
            }
            catch (Exception ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
