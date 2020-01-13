using System.Collections.Generic;

namespace Proje.Models
{
    public class ViewModel
    {
        public PagedList.IPagedList<Post> Posts { get; set; }
        public Announcement Announcement { get; set; }
        public Message Message { get; set; }
        public Setting Setting { get; set; }
    }
}