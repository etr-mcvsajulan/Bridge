using System;
using System.ComponentModel.DataAnnotations;

namespace FriendlyRS1.ViewModels
{
    public enum AppointmentStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Completed = 3,
        Cancelled = 4
    }

    public class AppointmentVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Author ID")]
        public int AuthorId { get; set; }

        [Required]
        [Display(Name = "Receiver ID")]
        public int ReceiverId { get; set; }

        [Display(Name = "Author Name")]
        public string? AuthorName { get; set; }

        [Display(Name = "Receiver Name")]
        public string? ReceiverName { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Status")]
        public AppointmentStatus Status { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }

        public string StatusText => Status.ToString();
        public string StartTimeFormatted => StartTime.ToString("yyyy-MM-dd HH:mm");
        public string EndTimeFormatted => EndTime.ToString("yyyy-MM-dd HH:mm");
    }
}
