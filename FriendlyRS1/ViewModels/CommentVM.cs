using System;

namespace FriendlyRS1.ViewModels
{
    public class CommentVM
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public byte[] ProfileImage { get; set; }
        public string DateCreated { get; set; }
        public string DateUpdated { get; set; }
        public bool IsMe { get; set; }
    }
}
