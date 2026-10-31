using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

using ChatServer.Entities;

namespace ChatServer.Context
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserRefreshToken> UsersRefreshToken { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("citext");

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .HasColumnType("citext");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<UserRefreshToken>()
                .HasIndex(t => t.TokenHesh)
                .IsUnique();

        }







    }
}
