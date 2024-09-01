using Microsoft.EntityFrameworkCore;
using MoviesService.Models;

namespace MoviesService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {

        }

        public DbSet<Movie> Movies{ get; set; }

        public DbSet<Serial> Serials{ get; set; }
    }
}
