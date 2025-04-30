using System.ComponentModel.DataAnnotations;

namespace Ice_task_4.Models
{
    public class Guest
    {
        [Key]
        public int GuestId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}