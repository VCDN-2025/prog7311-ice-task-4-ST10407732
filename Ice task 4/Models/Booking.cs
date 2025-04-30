using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ice_task_4.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [ForeignKey("Guest")]
        public int GuestId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        public bool IsCancelled { get; set; } = false;

        // Navigation properties
        public virtual Room Room { get; set; }
        public virtual Guest Guest { get; set; }
    }
}