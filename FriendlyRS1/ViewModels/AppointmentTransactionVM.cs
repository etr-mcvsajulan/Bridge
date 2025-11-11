using System;

namespace FriendlyRS1.ViewModels
{
    public class AppointmentTransactionVM
    {
        public int Id { get; set; }
        public string TransanctionId { get; set; }
        public string AuthorName { get; set; }
        public string ReceiverName { get; set; }
        public AppointmentStatus? AppointmentStatus { get; set; }
        public string AppointmentStatusString { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public string PaymentStatusString { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
