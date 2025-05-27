using Application.DTOs;
using Application.Paggings;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        private readonly IUserService _userService;

        /// <summary>  
        /// Initializes a new instance of the <see cref="BookingController"/> class.  
        /// </summary>  
        /// <param name="bookingService">Service for handling booking operations.</param>
        /// <param name="userService"></param>  
        public BookingController(IBookingService bookingService, IUserService userService)
        {
            _bookingService = bookingService;
            _userService = userService;
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
        /// <param name="status">Optional filter by booking status. 
        /// /// Status to update:
        /// 0 = Pending,
        /// 1 = Confirmed,
        /// 2 = Canceled,
        /// 3 = Completed
        /// </param> 
        /// <returns>Paginated list of bookings.</returns>  
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
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
        /// Retrieves bookings for the currently authenticated artist with pagination and optional filters.  
        /// </summary>  
        /// <param name="pageIndex">Page index (default 1).</param>  
        /// <param name="pageSize">Page size (default 10).</param>  
        /// <param name="customerId">Optional filter by customer ID.</param>  
        /// <param name="startDate">Optional filter by start date.</param>  
        /// <param name="endDate">Optional filter by end date.</param>  
        /// <param name="status">Optional filter by booking status. 
        /// /// Status to update:
        /// 0 = Pending,
        /// 1 = Confirmed,
        /// 2 = Canceled,
        /// 3 = Completed
        /// </param> 
        /// <returns>Paginated list of bookings for the artist.</returns>  
        [HttpGet("artist")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Artist")]
        public async Task<ActionResult<PaginatedList<BookingDTO>>> GetBookingsForArtist(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? customerId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] BookingStatus? status = null)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var artistId = _userService.GetCurrentUserId();

            var bookings = await _bookingService.GetBookingsAsync(pageIndex, pageSize, customerId, artistId, startDate, endDate, status);
            return Ok(bookings);
        }

        /// <summary>  
        /// Retrieves bookings for the currently authenticated customer with pagination and optional filters.  
        /// </summary>  
        /// <param name="pageIndex">Page index (default 1).</param>  
        /// <param name="pageSize">Page size (default 10).</param>  
        /// <param name="artistId">Optional filter by artist ID.</param>  
        /// <param name="startDate">Optional filter by start date.</param>  
        /// <param name="endDate">Optional filter by end date.</param>  
        /// <param name="status">Optional filter by booking status. 
        /// /// Status to update:
        /// 0 = Pending,
        /// 1 = Confirmed,
        /// 2 = Canceled,
        /// 3 = Completed
        /// </param>  
        /// <returns>Paginated list of bookings for the customer.</returns>  
        [HttpGet("customer")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "User")]
        public async Task<ActionResult<PaginatedList<BookingDTO>>> GetBookingsForCustomer(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? artistId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] BookingStatus? status = null)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                return BadRequest("Page index and size must be greater than 0.");

            var customerId = _userService.GetCurrentUserId();

            var bookings = await _bookingService.GetBookingsAsync(pageIndex, pageSize, customerId, artistId, startDate, endDate, status);
            return Ok(bookings);
        }

        /// <summary>
        /// Creates a new booking.
        /// </summary>
        /// <param name="bookingDto">Booking data.</param>
        /// <returns>The created booking.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Admin")]
        public async Task<ActionResult<BookingDTO>> CreateBooking([FromBody] BookingCreateDTO bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Booking data is required.");

            var createdBooking = await _bookingService.CreateBookingAsync(bookingDto);
            return CreatedAtAction(nameof(GetBookings), new { id = createdBooking.Id }, createdBooking);
        }

        /// <summary>
        /// Creates a booking for the currently authenticated customer.
        /// </summary>
        /// <param name="bookingDto">Booking details.</param>
        /// <returns>The created booking.</returns>
        [HttpPost("customer")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles ="User")]
        public async Task<ActionResult<BookingDTO>> BookingForCustomer([FromBody] BookingCreateDTO bookingDto)
        {
            if (bookingDto == null)
                return BadRequest("Booking data is required.");

            var createdBooking = await _bookingService.BookingForCustomerAsync(bookingDto);
            return CreatedAtAction(nameof(GetBookings), new { id = createdBooking.Id }, createdBooking);
        }

        /// <summary>
        /// Confirms or cancel a booking by ID.
        /// </summary>
        /// <param name="id">Booking ID.</param>
        /// <param name="status">
        /// Status to update:
        /// 0 = Pending,
        /// 1 = Confirmed,
        /// 2 = Canceled,
        /// 3 = Completed
        /// </param>
        /// <returns>The updated booking.</returns>
        [HttpPut("{id}/confirm")]
        [Authorize(AuthenticationSchemes = "Jwt", Roles = "Artist, User")]
        public async Task<ActionResult<BookingDTO>> ConfirmBooking(Guid id, [FromQuery] BookingStatus status = BookingStatus.Confirmed)
        {
            var confirmedBooking = await _bookingService.StatusBookingAsync(id, status);
            return Ok(confirmedBooking);
        }

    }
}
