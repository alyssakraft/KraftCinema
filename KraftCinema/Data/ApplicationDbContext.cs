using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KraftCinema.Models;

namespace KraftCinema.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<KraftCinema.Models.Movie> Movie { get; set; } = default!;
        public DbSet<KraftCinema.Models.Actor> Actor { get; set; } = default!;
        public DbSet<KraftCinema.Models.MovieActor> MovieActor { get; set; } = default!;
    }
}
