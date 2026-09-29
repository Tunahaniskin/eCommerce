namespace ECommerce.API.Modules.Auth.Entities;

public class RolePermission
{
    public Guid RoleId { get; private set; }
    public string Permission { get; private set; } = default!;

    private RolePermission() { } // For EF Core

    public RolePermission(Guid roleId, string permission)
    {
        RoleId = roleId;
        Permission = permission;
    }
}
