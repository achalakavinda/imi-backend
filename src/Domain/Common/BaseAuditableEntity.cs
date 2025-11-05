namespace MigratingAssistant.Domain.Common;

public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}

// Keep a non-generic alias. Use bigint (long) for the auditable alias by default so
// audit-specific tables/entities can use a bigint id without affecting domain entities.
public abstract class BaseAuditableEntity : BaseAuditableEntity<long>
{
}
