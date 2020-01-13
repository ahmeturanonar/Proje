using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proje.Models
{

    [Table("Announcements")]
    public class Announcement
    {
        [Key , DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnnouncementId { get; set; }

        [AllowHtml]
        public string AnnouncementText { get; set; }
    }
}