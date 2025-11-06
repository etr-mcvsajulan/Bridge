using System;
using System.ComponentModel.DataAnnotations;

namespace FriendlyRS1.ViewModels
{

    public class AppointmentPaymentVM
    {
        public int Id { get; set; }
        public string? TransactionId { get; set; }

        public decimal ProfessionalFee { get; set; }
        public decimal ServiceFee { get; set; }

        public decimal TotalAmount => ProfessionalFee + ServiceFee;

        public string? PaymentMethod { get; set; }
        public PaymentStatus? Status { get; set; }
        public string StatusText => Status?.ToString() ?? PaymentStatus.Pending.ToString();

        public DateTime? PaidAt { get; set; }

        public string PaidAtFormatted => PaidAt.HasValue ? PaidAt?.ToString("yyyy-MM-dd HH:mm") : "";
    }
}
