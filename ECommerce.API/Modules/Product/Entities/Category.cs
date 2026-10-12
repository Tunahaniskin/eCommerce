using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Product.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() 
    { 
        Name = default!;
    }

    public Category(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }
}
