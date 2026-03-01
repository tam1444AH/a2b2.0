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
        private readonly IAmadeusService _amadeusService;

        public HotelController(IHotelsService hotelsService, IAmadeusService amadeusService)
        {
            _hotelsService = hotelsService;
            _amadeusService = amadeusService;
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

        [Authorize]
        [HttpGet("{to}-{dist}-{stars}")]
        public async Task<ActionResult<List<HotelSearchResultDto>>> Search(string to, string dist, string stars)
        {
            try
            {
                var results = await _amadeusService.SearchHotelsAsync(to, dist, stars);

                if (results.Count == 0)
                {
                    return Ok(new List<HotelSearchResultDto>());
                }

                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to search hotels.", detail = ex.Message });
            }

        }
    }
}
