﻿namespace HotelApi.Models
{
    public class Room
    {
        public int Id { get; set; }
        public RoomType RoomType { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int? UserId { get; set; }

    }

    public enum RoomType
    {
        Single,
        Double,
        Suit
    }
}
