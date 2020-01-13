using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    [Table("UserMessages")]
    public class UserMessage
    {
        [Key , DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

    }
}