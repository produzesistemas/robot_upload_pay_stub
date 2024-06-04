
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;

namespace racservice.Services
{
    public class AppDbContext: DbContext
    {
        private IConfiguration _configuration;
        public DbSet<Establishment> Establishment { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<Log> Log { get; set; }

        public AppDbContext(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("defaultConn"));
        }
    }
}
