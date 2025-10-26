using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.EntityModels
{
    public class Appointments
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; } 

        [Required]
        public int ReceiverId { get; set; } 

        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser Receiver { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int Status { get; set; } 

        public DateTime CreatedAt { get; set; }
    }
}
