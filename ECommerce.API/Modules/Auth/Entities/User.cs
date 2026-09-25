using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    
    // Yeni: Kullanıcının atomik yetkileri
    public List<string> Permissions { get; private set; } = new(); 
    
    // Yeni: Soft-delete bayrağı
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { } // EF Core için

    public User(string email, string passwordHash, UserRole role, List<string>? permissions = null)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;

        // SuperAdmin doğuştan her şeye yetkilidir, kısıtlanamaz.
        if (role == UserRole.SuperAdmin)
        {
            Permissions = new List<string> { Constants.Permissions.Wildcard };
        }
        else
        {
            Permissions = permissions ?? new List<string>();
        }
    }

    public void UpdatePermissions(List<string> permissions)
    {
        // SuperAdmin'in yetkisi değiştirilemez
        if (Role != UserRole.SuperAdmin)
        {
            Permissions = permissions.Distinct().ToList();
        }
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }
}