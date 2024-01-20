using HotelApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelApi.Data
{
    public class HotelDbContext: DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) :base(options)
        {
                
        }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
