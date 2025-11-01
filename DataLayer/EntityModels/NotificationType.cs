using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataLayer.EntityModels
{
    public class NotificationType
    {
        [Key]
        public int Id { get; set; }
        public string NotificationDescription { get; set; }
        public string NotificationMessage { get; set; }
    }

    public enum EnumNotificationType
    {
        FriendRequest = 1,
        FriendAccepted = 2,
        AppointmentCreated = 3,
        AppointmentAccepted = 4,
        AppointmentCompleted = 5,
        AppointmentCancelled = 6,
        AppointmentPaid = 7
    }
}
