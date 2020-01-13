using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key , DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Bu alan gereklidir!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Bu alan gereklidir!")]
        [RegularExpression(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                            ErrorMessage = "Lütfen geçerli bir E-Mail Adresi giriniz...")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Bu alan gereklidir!")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Bu alan gereklidir!")]
        public string Content { get; set; }


    }
}