using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Proje.Models
{
    public class ProjeContext : DbContext
    {
        public ProjeContext():base("Default")
        {

        }
        
        public DbSet<Post> Post { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<User> User { get; set; }

        public DbSet<Comment> Comment { get; set; }

        public DbSet<Announcement> Announcement { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<UserMessage> UserMessages { get; set; }

        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }

    }
}