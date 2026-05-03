using Domain.Common;

namespace Domain.Entities;

public class Review : BaseAuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string CustomerUserId { get; set; } = string.Empty;

    public int Rating { get; set; }
    public string? Comment { get; set; }
}
