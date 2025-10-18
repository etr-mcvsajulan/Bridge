using System;

namespace FriendlyRS1.ViewModels
{
    public class ChatListVM
    {
        public int UserId { get; set; }         
        public string Name { get; set; }         
        public string LastMessage { get; set; }    
        public byte[] ProfileImage { get; set; }  
        public DateTime LastMessageDate { get; set; }
    }
}
