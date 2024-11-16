using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
using Sample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Products> Products { get; set; }
        public DbSet<Parts> Parts { get; set; }
        public DbSet<Options> Options { get; set; }
        public DbSet<OptionParts> OptionParts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Products>()
                .HasKey(u => u.Id); // 主キーを明示的に設定

            //modelBuilder.Entity<Parts>()
            //   .HasOne(o => o.Name)        // Order が User に属する
            //   .WithMany(u => u.Orders)    // User は複数の Order を持つ
            //   .HasForeignKey(o => o.UserId); // 外部キーを明示
        }
    }
}
