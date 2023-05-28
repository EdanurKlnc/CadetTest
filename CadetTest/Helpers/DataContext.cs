using Microsoft.EntityFrameworkCore;
using CadetTest.Entities;

namespace CadetTest.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Consent> Consents { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}
