using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.EntityModels
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateUpdated { get; set; } = DateTime.Now;

        // Foreign Keys
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public ApplicationUser Author { get; set; }
    }
}
