namespace ECommerce.API.Modules.Auth.Entities;

public class UserTemporaryPermission
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Permission { get; private set; } = default!;
    public DateTime? ExpiresAt { get; private set; }
    public Guid? GrantedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserTemporaryPermission() { } // For EF Core

    public UserTemporaryPermission(Guid userId, string permission, DateTime? expiresAt, Guid? grantedBy)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Permission = permission;
        ExpiresAt = expiresAt;
        GrantedBy = grantedBy;
        CreatedAt = DateTime.UtcNow;
    }
}
