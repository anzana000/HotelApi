using HotelApi.Data;
using HotelApi.Models;
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

        [HttpPost]
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

        [HttpGet("{id}")]
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

    }
}
