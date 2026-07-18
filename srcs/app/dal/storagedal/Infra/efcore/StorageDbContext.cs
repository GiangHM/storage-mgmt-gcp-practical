using Microsoft.EntityFrameworkCore;
using sharedentities.DBEntities;

namespace storagedal.Infra.efcore
{
    public class StorageDbContext : DbContext
    {
        public StorageDbContext(DbContextOptions<StorageDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        public DbSet<StorageDocument> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StorageDocument>(entity =>
            {
                entity.ToTable("storagedocument");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DocTypeCode).HasColumnName("doc_type_code");
                entity.Property(e => e.DocUrl).HasColumnName("doc_url");
                entity.Property(e => e.IsActivated).HasColumnName("is_activated");
                entity.Property(e => e.CreationDate).HasColumnName("creation_date");
                entity.Property(e => e.CreationUser).HasColumnName("creation_user");
                entity.Property(e => e.ModificationDate).HasColumnName("modification_date");
                entity.Property(e => e.ModificationUser).HasColumnName("modification_user");
            });
        }
    }
}
