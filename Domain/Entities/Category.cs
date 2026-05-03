using Domain.Common;

namespace Domain.Entities;

public class Category : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public int DisplayOrder { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
