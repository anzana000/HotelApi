using HotelApi.Data;
using HotelApi.Data.DTOs;
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
        public async Task<IActionResult> GetAvailableRooms(RoomType roomType, DateTime date)
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.RoomType == roomType && (
                (r.CheckInDate==null&&r.CheckOutDate==null) || (date<r.CheckInDate||date>r.CheckOutDate)))
                .ToListAsync();
            return Ok(availableRooms);
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookRoom([FromBody] BookRoomDTO obj)
        {
            var room = await _context.Rooms.FindAsync(obj.RoomId);
            if (room == null)
            {
                return BadRequest("Room not found or not available");
            }

            // Update room availability and booking details
         
            room.CheckInDate = obj.CheckInDate;
            room.CheckOutDate = obj.CheckInDate.AddDays(obj.Days);
            room.UserId = obj.UserId;

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

        [HttpGet("{userId}/invoice")]
        public async Task<IActionResult> GetInvoice(int userId)
        {
            try
            {
                // Fetch room and user data in a single query
              var room = await _context.Rooms
                     .Where(r => r.UserId == userId)
                     .ToListAsync();

                
                if (room == null)
                {
                    return NotFound("Room not found or not booked");
                }

                var user = await _context.Users.FindAsync(userId);

                // Calculate total cost with optional discount
                //decimal totalCost =  room.Price * (room.CheckOutDate.Value - room.CheckInDate.Value).Days;
                decimal gross = 0;
               
                decimal discount=0;
                foreach(var r in room)
                {
                    decimal totalCost = r.Price * (r.CheckOutDate.Value - r.CheckInDate.Value).Days;
                    gross += totalCost;
                }
                if (room.Count >= 3) // Apply 5% discount for 3 or more rooms booked by the same user
                {

                    discount = gross * 0.05M;
                    

                }


                // Create invoice data
                var invoiceData = new
                {
                    CustomerName = user.Name,
                    BookingDetails =room,
                    TotalRooms = room.Count,
                    GrossAmount = gross,
                    Discount = discount,
                   NetAmount = gross - discount
                   
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
