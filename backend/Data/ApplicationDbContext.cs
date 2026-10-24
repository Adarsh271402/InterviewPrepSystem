using InterviewPrepSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrepSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<MockTest> MockTests { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Name).HasMaxLength(100).IsRequired();
                e.Property(u => u.Email).HasMaxLength(100).IsRequired();
                e.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("User");
                e.Property(u => u.CreatedDate).HasDefaultValueSql(Database.IsNpgsql() ? "CURRENT_TIMESTAMP" : "GETDATE()");
            });

            // Category
            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            });

            // Question
            modelBuilder.Entity<Question>(e =>
            {
                e.HasKey(q => q.Id);
                e.Property(q => q.DifficultyLevel).HasMaxLength(50).HasDefaultValue("Medium");
                e.HasOne(q => q.Category)
                    .WithMany(c => c.Questions)
                    .HasForeignKey(q => q.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MockTest
            modelBuilder.Entity<MockTest>(e =>
            {
                e.HasKey(m => m.Id);
                e.Property(m => m.Status).HasMaxLength(50).HasDefaultValue("InProgress");
                e.Property(m => m.CreatedDate).HasDefaultValueSql(Database.IsNpgsql() ? "CURRENT_TIMESTAMP" : "GETDATE()");
                e.HasOne(m => m.User)
                    .WithMany(u => u.MockTests)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(m => m.Category)
                    .WithMany(c => c.MockTests)
                    .HasForeignKey(m => m.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // UserAnswer
            modelBuilder.Entity<UserAnswer>(e =>
            {
                e.HasKey(ua => ua.Id);
                e.HasOne(ua => ua.MockTest)
                    .WithMany(m => m.UserAnswers)
                    .HasForeignKey(ua => ua.MockTestId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ua => ua.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(ua => ua.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Seed Data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Java" },
                new Category { Id = 2, Name = "SQL" },
                new Category { Id = 3, Name = "ASP.NET Core" },
                new Category { Id = 4, Name = "Python" },
                new Category { Id = 5, Name = "HR Interview" },
                new Category { Id = 6, Name = "C#" },
                new Category { Id = 7, Name = "React.js" }
            );
        }
    }
}
