using System;

namespace FriendlyRS1.ViewModels
{
    public class CreateChatVM
    {
        public int ReceiverId { get; set; }
        public string? MessageText { get; set; }
        public byte[]? ImageData { get; set; }
    }
}
