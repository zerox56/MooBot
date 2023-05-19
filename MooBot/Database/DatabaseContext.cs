using Microsoft.EntityFrameworkCore;
using Moobot.Database.Configurations;
using Moobot.Database.Models.Entities;

namespace Moobot.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public virtual DbSet<Guild> Guild { get; set; }
        public virtual DbSet<Channel> Channel { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Reminder> Reminder { get; set; }
        public virtual DbSet<UserReminder> UserReminder { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new GuildConfiguration());
            builder.ApplyConfiguration(new ChannelConfiguration());
            builder.ApplyConfiguration(new RoleConfiguration());
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new ReminderConfiguration());
            builder.ApplyConfiguration(new UserReminderConfiguration());
        }

        public override int SaveChanges()
        {
            AddAuditValues();

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            AddAuditValues();

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void AddAuditValues()
        {
            var entities = ChangeTracker.Entries().Where(x => (x.Entity is Entity)
                && (x.State == EntityState.Added || x.State == EntityState.Detached || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.Entity is Entity)
                {
                    if (entity.State == EntityState.Added)
                    {
                        ((Entity)entity.Entity).DateCreated = DateTime.Now;
                    }

                    if (entity.State == EntityState.Modified)
                    {
                        ((Entity)entity.Entity).DateModified = DateTime.Now;
                    }
                }
            }
        }
    }
}