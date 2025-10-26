using System;
using System.ComponentModel.DataAnnotations;

namespace FriendlyRS1.ViewModels
{
    public class AddAppointmentVM
    {
        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
