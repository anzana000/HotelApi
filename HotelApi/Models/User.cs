using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}
