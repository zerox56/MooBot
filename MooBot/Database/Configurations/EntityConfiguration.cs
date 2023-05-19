using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moobot.Database.Models.Entities;

namespace Moobot.Database.Configurations
{
    public class EntityConfiguration<T> : IEntityTypeConfiguration<T>
        where T : Entity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.DateCreated).HasColumnType(DataConstants.SqlServer.DateTime2).HasDefaultValueSql(DataConstants.SqlServer.SysDateTime);
            builder.Property(e => e.DateModified).HasColumnType(DataConstants.SqlServer.DateTime2);
        }
    }
}