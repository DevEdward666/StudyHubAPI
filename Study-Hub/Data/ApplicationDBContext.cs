using Microsoft.EntityFrameworkCore;
using Study_Hub.Models.Entities;
using System.Text.Json;

namespace Study_Hub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<AuthAccount> AuthAccounts { get; set; }
        //public DbSet<AuthSession> AuthSessions { get; set; }
        public DbSet<StudyTable> StudyTables { get; set; }
        public DbSet<UserCredit> UserCredit { get; set; }
        public DbSet<CreditTransaction> CreditTransactions { get; set; }
        public DbSet<TableSession> TableSessions { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<PremiseActivation> PremiseActivations { get; set; }
        public DbSet<PremiseQrCode> PremiseQrCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure enum conversions
            modelBuilder.Entity<CreditTransaction>()
                .Property(e => e.Status)
                .HasConversion<string>();

            modelBuilder.HasPostgresEnum<SessionStatus>("session_status");

            modelBuilder.Entity<TableSession>()
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),                            // to database: "active"
                    v => (SessionStatus)Enum.Parse(typeof(SessionStatus), v)) // from db
                .HasColumnType("session_status");

            modelBuilder.Entity<AdminUser>()
                .Property(e => e.Role)
                 .HasConversion(
                    v => v.ToString().ToLower(),  // to DB
                    v => Enum.Parse<UserRole>(v.ToLower(), true)); // from DB
            // Configure unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<StudyTable>()
                .HasIndex(st => st.TableNumber)
                .IsUnique();

            modelBuilder.Entity<StudyTable>()
                .HasIndex(st => st.QrCode)
                .IsUnique();

            modelBuilder.Entity<UserCredit>()
                .HasIndex(uc => uc.UserId)
                .IsUnique();

            modelBuilder.Entity<AdminUser>()
                .HasIndex(au => au.UserId)
                .IsUnique();

            modelBuilder.Entity<PremiseQrCode>()
                .HasIndex(pqr => pqr.Code)
                .IsUnique();

            modelBuilder.Entity<AuthAccount>()
                .HasIndex(aa => new { aa.Provider, aa.ProviderAccountId })
                .IsUnique();

            // Configure other indexes for performance
            modelBuilder.Entity<CreditTransaction>()
                .HasIndex(ct => ct.Status);

            modelBuilder.Entity<TableSession>()
                .HasIndex(ts => ts.Status);

            modelBuilder.Entity<PremiseActivation>()
                .HasIndex(pa => pa.IsActive);

            modelBuilder.Entity<PremiseActivation>()
                .HasIndex(pa => pa.ExpiresAt);

            // Configure relationships
            modelBuilder.Entity<StudyTable>()
                .HasOne(st => st.CurrentUser)
                .WithMany()
                .HasForeignKey(st => st.CurrentUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CreditTransaction>()
                .HasOne(ct => ct.ApprovedByUser)
                .WithMany()
                .HasForeignKey(ct => ct.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure default values
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("uuid_generate_v4()");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Apply similar default configurations to other entities
            var entityTypes = new[]
            {
                typeof(AuthAccount), typeof(StudyTable), typeof(UserCredit),
                typeof(CreditTransaction), typeof(TableSession), typeof(AdminUser),
                typeof(PremiseActivation), typeof(PremiseQrCode)
            };

            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType)
                    .Property("Id")
                    .HasDefaultValueSql("uuid_generate_v4()");

                modelBuilder.Entity(entityType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                modelBuilder.Entity(entityType)
                    .Property("UpdatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entities = ChangeTracker
                .Entries()
                .Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;

                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).CreatedAt = now;
                }

                ((BaseEntity)entity.Entity).UpdatedAt = now;
            }
        }
    }

    // Base entity class for common properties
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}