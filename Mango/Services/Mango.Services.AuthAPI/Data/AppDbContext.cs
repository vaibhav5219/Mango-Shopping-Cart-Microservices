using Mango.Services.AuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser> // IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {        
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; } // Add this line for Application User, it will not create new table
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);  // used to seed some data
            
        }
    }
}
