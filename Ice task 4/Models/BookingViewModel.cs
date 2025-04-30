using System.ComponentModel.DataAnnotations;

namespace Ice_task_4.Models
{
    public class BookingViewModel
    {
        public int RoomId { get; set; }

        public string RoomNumber { get; set; }

        public string RoomType { get; set; }

        public decimal PricePerNight { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOutDate { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string GuestEmail { get; set; }
    }
}