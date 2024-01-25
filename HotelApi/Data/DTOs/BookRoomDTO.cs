using System.Security.Principal;

namespace HotelApi.Data.DTOs
{
    public class BookRoomDTO
    {
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
