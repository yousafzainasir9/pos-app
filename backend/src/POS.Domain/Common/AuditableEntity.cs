namespace POS.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string? DeletedBy { get; set; }
}
