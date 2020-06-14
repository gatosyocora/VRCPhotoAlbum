using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; internal set; }
        public DbSet<UserNameHistory> UserNameHistories { get; internal set; }
        public DbSet<World> Worlds { get; internal set; }
        public DbSet<WorldNameHistory> WorldNameHistories { get; internal set; }
        public DbSet<Photo> Photos { get; internal set; }
        public DbSet<PhotoUser> PhotoUsers { get; internal set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = @"Cache\cache.db"
            }.ToString();

            optionsBuilder.UseSqlite(new SqliteConnection(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            // UserNameHistories
            modelBuilder.Entity<UserNameHistory>()
                .Property<int>(dnh => dnh.UserNameHistoryId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UserNameHistory>()
                .Property<int>("UserId");

            modelBuilder.Entity<UserNameHistory>()
                .HasKey(new string[] { "UserNameHistoryId", "UserId" });

            // World
            modelBuilder.Entity<World>()
                .HasKey(u => u.WorldId);

            // WorldNameHistories
            modelBuilder.Entity<WorldNameHistory>()
                .Property<int>(dnh => dnh.WorldNameHistoryId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<WorldNameHistory>()
                .Property<int>("WorldId");

            modelBuilder.Entity<WorldNameHistory>()
                .HasKey(new string[] { "WorldNameHistoryId", "WorldId" });

            // Photos
            modelBuilder.Entity<Photo>()
                .HasKey(p => p.FilePath);

            // PhotoUsers
            modelBuilder.Entity<PhotoUser>()
                .Property<string>("FilePath");

            modelBuilder.Entity<PhotoUser>()
                .Property<int>("UserId");

            modelBuilder.Entity<PhotoUser>()
                .HasKey(new string[] { "FilePath", "UserId" });
        }
    }
}
