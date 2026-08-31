using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<KycCase> KycCases => Set<KycCase>();
    public DbSet<KycDocument> KycDocuments => Set<KycDocument>();
    public DbSet<KycAuditEvent> KycAuditEvents => Set<KycAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var builder = modelBuilder.Entity<Customer>();

        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(256).IsRequired();
        builder.Property(customer => customer.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(customer => customer.IsActive).IsRequired();
        builder.Property(customer => customer.CreatedAtUtc).IsRequired();
        builder.Property(customer => customer.UpdatedAtUtc);
        builder.HasIndex(customer => customer.Email).IsUnique();

        builder.OwnsOne(customer => customer.Address, addressBuilder =>
        {
            addressBuilder.Property(address => address.Line1).HasMaxLength(200).HasColumnName("AddressLine1");
            addressBuilder.Property(address => address.Line2).HasMaxLength(200).HasColumnName("AddressLine2");
            addressBuilder.Property(address => address.City).HasMaxLength(100).HasColumnName("AddressCity");
            addressBuilder.Property(address => address.State).HasMaxLength(100).HasColumnName("AddressState");
            addressBuilder.Property(address => address.PostalCode).HasMaxLength(20).HasColumnName("AddressPostalCode");
            addressBuilder.Property(address => address.Country).HasMaxLength(100).HasColumnName("AddressCountry");
        });

        builder.OwnsOne(customer => customer.Nominee, nomineeBuilder =>
        {
            nomineeBuilder.Property(nominee => nominee.FullName).HasMaxLength(150).HasColumnName("NomineeFullName");
            nomineeBuilder.Property(nominee => nominee.Relationship).HasMaxLength(100).HasColumnName("NomineeRelationship");
            nomineeBuilder.Property(nominee => nominee.PhoneNumber).HasMaxLength(30).HasColumnName("NomineePhoneNumber");
            nomineeBuilder.Property(nominee => nominee.Email).HasMaxLength(256).HasColumnName("NomineeEmail");
        });

        builder.OwnsOne(customer => customer.Kyc, kycBuilder =>
        {
            kycBuilder.Property(kyc => kyc.DocumentType).HasMaxLength(100).HasColumnName("KycDocumentType");
            kycBuilder.Property(kyc => kyc.DocumentNumber).HasMaxLength(100).HasColumnName("KycDocumentNumber");
            kycBuilder.Property(kyc => kyc.Status).HasColumnName("KycStatus");
            kycBuilder.Property(kyc => kyc.VerifiedAtUtc).HasColumnName("KycVerifiedAtUtc");
        });

        modelBuilder.Entity<KycCase>(builder =>
        {
            builder.ToTable("KycCases");
            builder.HasKey(kycCase => kycCase.Id);
            builder.Property(kycCase => kycCase.ConsentVersion).HasMaxLength(50).IsRequired();
            builder.Property(kycCase => kycCase.RejectionReason).HasMaxLength(1000);
            builder.HasIndex(kycCase => new { kycCase.CustomerId, kycCase.Status });
        });

        modelBuilder.Entity<KycDocument>(builder =>
        {
            builder.ToTable("KycDocuments");
            builder.HasKey(document => document.Id);
            builder.Property(document => document.DocumentType).HasMaxLength(100).IsRequired();
            builder.Property(document => document.ObjectKey).HasMaxLength(512).IsRequired();
            builder.Property(document => document.ContentType).HasMaxLength(100).IsRequired();
            builder.Property(document => document.Fingerprint).HasMaxLength(64).IsRequired();
            builder.Property(document => document.EncryptedContent).HasColumnType("bytea");
            builder.HasIndex(document => document.Fingerprint).IsUnique();
            builder.HasOne<KycCase>().WithMany().HasForeignKey(document => document.KycCaseId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KycAuditEvent>(builder =>
        {
            builder.ToTable("KycAuditEvents");
            builder.HasKey(auditEvent => auditEvent.Id);
            builder.Property(auditEvent => auditEvent.EventType).HasMaxLength(100).IsRequired();
            builder.Property(auditEvent => auditEvent.Details).HasMaxLength(2000).IsRequired();
            builder.HasIndex(auditEvent => new { auditEvent.KycCaseId, auditEvent.OccurredAtUtc });
            builder.HasOne<KycCase>().WithMany().HasForeignKey(auditEvent => auditEvent.KycCaseId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
