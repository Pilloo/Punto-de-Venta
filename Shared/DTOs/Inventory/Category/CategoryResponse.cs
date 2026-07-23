using Models.Inventory;

namespace DTOs.Inventory.Category;

/// <summary>
/// Represents the response object for a category in the inventory system.
/// </summary>
/// <remarks>
/// This class is used to encapsulate information about a category, including its unique identifier, name,
/// and activation status, when returning data from the inventory system.
/// </remarks>
/// <seealso cref="Category" />
public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }

    public static CategoryResponse FromEntity(Models.Inventory.Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Active = category.IsActive
    };
}