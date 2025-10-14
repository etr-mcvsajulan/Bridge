using System;

namespace FriendlyRS1.ViewModels
{
    public class ChatVM
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        // Optional message
        public string? MessageText { get; set; }

        // Image stored as binary (varbinary in DB)
        public byte[]? ImageData { get; set; }

        public DateTime SentDate { get; set; }

        public bool IsRead { get; set; }

        // For display (optional)
        public string? SenderName { get; set; }

        public string? ReceiverName { get; set; }

        public byte[] SenderProfileImage { get; set; }

        public byte[] ReceiverProfileImage { get; set; }

        public string SentTimeFormatted { get; set; }

        public bool IsMe { get; set; }
    }
}
