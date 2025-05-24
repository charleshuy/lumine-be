using Application.DTOs;
using Application.Paggings;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>  
    /// Controller for managing booking-related operations.  
    /// </summary>  
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="BookingController"/> class.  
        /// </summary>  
        /// <param name="bookingService">Service for handling booking operations.</param>  
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>  
        /// Retrieves bookings with pagination and optional filters.  
        /// </summary>  
        /// <param name="pageIndex">Page index (default 1).</param>  
        /// <param name="pageSize">Page size (default 10).</param>  
        /// <param name="customerId">Optional filter by customer ID.</param>
        /// <param name="artistId"></param>  
        /// <param name="startDate">Optional filter by start date.</param>  
        /// <param name="endDate">Optional filter by end date.</param>  
        /// <param name="status">Optional filter by booking status.</param>  
        /// <returns>Paginated list of bookings.</returns>  
        [HttpGet]
        public async Task<ActionResult<PaginatedList<BookingDTO>>> GetBookings(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? customerId = null,
            [FromQuery] Guid? artistId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] BookingStatus? status = null)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var bookings = await _bookingService.GetBookingsAsync(pageIndex, pageSize, customerId, artistId, startDate, endDate, status);
            return Ok(bookings);
        }

        /// <summary>
        /// Creates a new booking.
        /// </summary>
        /// <param name="bookingDto">Booking data.</param>
        /// <returns>The created booking.</returns>
        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking([FromBody] BookingCreateDTO bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Booking data is required.");

            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(bookingDto);
                return CreatedAtAction(nameof(GetBookings), new { id = createdBooking.Id }, createdBooking);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the booking.");
            }
        }
    }
}
