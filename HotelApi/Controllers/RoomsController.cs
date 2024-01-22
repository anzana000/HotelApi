using HotelApi.Data;
using HotelApi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class RoomsController : ControllerBase
    {
        private readonly HotelDbContext _context;
        public RoomsController(HotelDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(RoomType roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.RoomType == roomType && r.IsAvailable && !r.CheckInDate.HasValue && !r.CheckOutDate.HasValue)
                .ToListAsync();
            return Ok(availableRooms);
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookRoom(int roomId, int userId, DateTime checkInDate, DateTime checkOutDate)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null || !room.IsAvailable)
            {
                return BadRequest("Room not found or not available");
            }

            // Update room availability and booking details
            room.IsAvailable = false;
            room.CheckInDate = checkInDate;
            room.CheckOutDate = checkOutDate;
            room.UserId = userId;

            await _context.SaveChangesAsync();
            return Ok("Room booked successfully");
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(Room),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int Id)
        {
            var room = await _context.Rooms.FindAsync(Id);
            return room == null ? NotFound(): Ok(room);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateRoom( Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [HttpGet("{roomId}/invoice")]
        public async Task<IActionResult> GetInvoice(int roomId)
        {
            try
            {
                // Fetch room and user data in a single query
                var room = await _context.Rooms
                    .Include(r => r.User)
                    .SingleOrDefaultAsync(r => r.Id == roomId);

                if (room == null || room.UserId == null)
                {
                    return NotFound("Room not found or not booked");
                }

                var user = room.User;

                // Calculate total cost with optional discount
                decimal totalCost = room.Price * (room.CheckOutDate.Value - room.CheckInDate.Value).Days;
                if (room.UserId.Value >= 3) // Apply 5% discount for 3 or more rooms booked by the same user
                {
                    
                    totalCost = totalCost * 0.95M;

                }
                

                // Create invoice data
                var invoiceData = new
                {
                    CustomerName = user.Name,
                    RoomType = room.RoomType.ToString(),
                    CheckInDate = room.CheckInDate.Value,
                    CheckOutDate = room.CheckOutDate.Value,
                    TotalCost = totalCost
                };

                return Ok(invoiceData);
            }
            catch (Exception ex)
            {
                // Handle potential exceptions gracefully
                return StatusCode(500, "Error retrieving invoice: " + ex.Message);
            }
        }



    }
}
