namespace ECommerce.API.Modules.Auth.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    public ICollection<RolePermission> Permissions { get; private set; } = new List<RolePermission>();

    private Role() { } // For EF Core

    public Role(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddPermission(string permission)
    {
        if (Permissions.All(p => p.Permission != permission))
        {
            Permissions.Add(new RolePermission(Id, permission));
        }
    }
}
