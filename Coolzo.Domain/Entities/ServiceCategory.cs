namespace Coolzo.Domain.Entities;

public sealed class ServiceCategory : AuditableEntity
{
    public long ServiceCategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    /// <summary>
    /// Optional admin-managed AI image-generation prompt for this category's image. Lets an admin
    /// store a tuned, reusable prompt so regenerating the image is one-click and consistent.
    /// Null/empty = the admin UI shows a generated suggested prompt instead.
    /// </summary>
    public string? ImageAIPrompt { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
