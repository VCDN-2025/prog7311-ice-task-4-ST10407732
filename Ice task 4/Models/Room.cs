using System.ComponentModel.DataAnnotations;

namespace Ice_task_4.Models
{
    public class Room
    {
        public Room()
        {
            // Initialize the collection
            Bookings = new List<Booking>();
        }

        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [Required]
        public string RoomType { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}