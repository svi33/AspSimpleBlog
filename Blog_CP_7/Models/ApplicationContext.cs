using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Blog_CP_7.Models
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<Post> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TagMap> TagMaps { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public ApplicationContext() : base("IdentityDb") { }

        public static ApplicationContext Create()
        {
            return new ApplicationContext();
        }

       // public System.Data.Entity.DbSet<Blog_CP_7.Models.ApplicationUser> ApplicationUsers { get; set; }
    }
}