using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proje.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key , DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }
        public string PostImage { get; set; }

        [Required]
        public string PostTitle { get; set; }

        [Required]
        [AllowHtml]
        public string PostText { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int TotalViews { get; set; }

        public List<Comment> Comments { get; set; }

        public int Comment_Count { get; set; }


    }
}