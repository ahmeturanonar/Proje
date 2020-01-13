using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public string Username { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }

        public DateTime PublishDate { get; set; }




    }
}