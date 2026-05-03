using Domain.Common;

namespace Domain.Entities;

public class ProductImage : BaseAuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
