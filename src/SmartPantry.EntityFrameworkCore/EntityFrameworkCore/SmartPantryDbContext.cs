using Microsoft.EntityFrameworkCore;
using SmartPantry.Authors;
using SmartPantry.Books;
using SmartPantry.Despensa;
using SmartPantry.Productos;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace SmartPantry.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class SmartPantryDbContext :
    AbpDbContext<SmartPantryDbContext>,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Author> Authors { get; set; }

    public DbSet<Book> Books { get; set; }

    public DbSet<Producto> Productos { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    #endregion

    public SmartPantryDbContext(DbContextOptions<SmartPantryDbContext> options)
        : base(options)
    {

    }
    public DbSet<SmartPantry.Despensas.Despensa> Despensas { get; set; }
    public DbSet<ItemDespensa> ItemsDespensa { get; set; }
    public DbSet<AdvertenciaVencimiento> AdvertenciasVencimiento { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Owned<Cantidad>();
        builder.Owned<FechaVencimiento>();
        builder.Owned<Nota>();
        base.OnModelCreating(builder);
        // Mapeo de Despensa e Items
        builder.Entity<SmartPantry.Despensas.Despensa>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "Despensas", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            // Relación 1 a N con ItemDespensa
            b.HasMany(d => d.Items).WithOne().HasForeignKey("DespensaId").IsRequired();
        });

        builder.Entity<ItemDespensa>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "ItemsDespensa", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            // Mapeo de Value Objects embebidos
            b.OwnsOne(i => i.Cantidad, c =>
            {
                c.Property(p => p.Monto).HasColumnName("CantidadMonto");
                c.Property(p => p.Unidad).HasColumnName("CantidadUnidad").HasMaxLength(20);
            });

            b.OwnsOne(i => i.FechaVencimiento, f =>
            {
                f.Property(p => p.FechaVenc).HasColumnName("FechaVencimiento");
            });

            b.OwnsOne(i => i.Nota, n =>
            {
                n.Property(p => p.Texto).HasColumnName("NotaTexto").HasMaxLength(250);
            });
        });

        // Mapeo de Advertencias de Vencimiento
        builder.Entity<AdvertenciaVencimiento>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "AdvertenciasVencimiento", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention();

            // Índice único compuesto para asegurar idempotencia (TP08)
            b.HasIndex(a => new { a.ItemDespensaId, a.TipoAdvertencia }).IsUnique();
        });

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

        builder.Entity<Producto>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "Productos", SmartPantryConsts.DbSchema);
            b.ConfigureByConvention(); // Configura propiedades base de ABP (Id, auditoría)

            b.Property(x => x.Nombre).IsRequired().HasMaxLength(ProductoConsts.MaxNombreLength);
            b.Property(x => x.Marca).IsRequired().HasMaxLength(ProductoConsts.MaxMarcaLength);
            b.Property(x => x.Categoria).IsRequired().HasMaxLength(ProductoConsts.MaxCategoriaLength);
            b.Property(x => x.UrlImagen).HasMaxLength(ProductoConsts.MaxUrlImagenLength);
        });

        builder.Entity<Author>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "Authors",
                SmartPantryConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(AuthorConsts.MaxNameLength);
            b.Property(x => x.ShortBio).HasMaxLength(AuthorConsts.MaxShortBioLength);
        });

        builder.Entity<Book>(b =>
        {
            b.ToTable(SmartPantryConsts.DbTablePrefix + "Books",
                SmartPantryConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.HasOne<Author>().WithMany().HasForeignKey(x => x.AuthorId).IsRequired();
        });


        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(SmartPantryConsts.DbTablePrefix + "YourEntities", SmartPantryConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
