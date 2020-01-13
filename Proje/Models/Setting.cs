using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    [Table("Settings")]
    public class Setting
    {
        public int Id { get; set; }
        public string SiteTitle { get; set; }
        public string TagLine { get; set; }
        public bool AnyoneCanRegister { get; set; }
        public string NewUserDefaultRole { get; set; }

    }
}