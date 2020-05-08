using Microsoft.EntityFrameworkCore;

namespace BookListRazor.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options)
        {

        }

        public DbSet<Book> Book { get; set; }
    }
}
