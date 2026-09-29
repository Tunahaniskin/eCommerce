namespace ECommerce.API.Modules.Auth.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;

    public ICollection<RolePermission> Permissions { get; private set; } = new List<RolePermission>();

    private Role() { } // For EF Core

    public Role(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
