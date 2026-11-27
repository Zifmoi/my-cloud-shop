using Microsoft.EntityFrameworkCore;
using CloudShop.Models;

namespace CloudShop.Data
{
    // Контекст базы данных — связующее звено между C#-классами и таблицами PostgreSQL
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}