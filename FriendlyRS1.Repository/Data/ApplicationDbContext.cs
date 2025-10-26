using DataLayer.EntityModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FriendlyRS1.Repository
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUserHobby>().HasKey(x => new { x.HobbyId, x.ApplicationUserId });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasOne(f => f.User1)
                      .WithMany()
                      .HasForeignKey(f => f.User1Id)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict to avoid multiple cascade paths

                entity.HasOne(f => f.User2)
                      .WithMany()
                      .HasForeignKey(f => f.User2Id)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict

                entity.HasOne(f => f.ActionUser)
                      .WithMany()
                      .HasForeignKey(f => f.ActionUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict

                entity.HasOne(f => f.Status)
                      .WithMany()
                      .HasForeignKey(f => f.StatusId)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade is okay here
            });

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.Property(c => c.MessageText)
                      .HasMaxLength(2000)
                      .IsUnicode(true);

                entity.Property(c => c.ImageData)
                      .HasColumnType("varbinary(max)");

                entity.HasOne(c => c.Sender)
                      .WithMany()
                      .HasForeignKey(c => c.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Receiver)
                      .WithMany()
                      .HasForeignKey(c => c.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Appointments>(entity =>
            {
                entity.HasOne(a => a.Author)
                      .WithMany()
                      .HasForeignKey(a => a.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Receiver)
                      .WithMany()
                      .HasForeignKey(a => a.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(a => a.Description)
                      .HasMaxLength(500);
            });
        }



        public DbSet<Hobby> Hobby { get; set; }
        public DbSet<HobbyCategory> HobbyCategory { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<FriendshipStatus> FriendshipStatus { get; set; }
        public DbSet<Friendship> Friendship { get; set; }
        public DbSet<ApplicationUserHobby> ApplicationUserHobbies { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<NotificationType> NotificationType { get; set; }
        public DbSet<BellNotification> BellNotification { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<Appointments> Appointments { get; set; }
    }
}
