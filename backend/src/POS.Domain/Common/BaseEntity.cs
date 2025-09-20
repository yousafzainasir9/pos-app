namespace POS.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public long? ModifiedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public long? DeletedByUserId { get; set; }
}
