using ECommerce.API.Modules.Auth.Constants;

namespace ECommerce.API.Modules.Auth.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public Guid RoleId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Role Role { get; private set; } = default!;
    public ICollection<UserTemporaryPermission> TemporaryPermissions { get; private set; } = new List<UserTemporaryPermission>();
    public ICollection<Address> Addresses { get; private set; } = new List<Address>();

    private User() { } // EF Core için

    public User(string firstName, string lastName, string email, string passwordHash, Guid roleId)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        RoleId = roleId;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(Guid newRoleId)
    {
        RoleId = newRoleId;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdateEmail(string email)
    {
        Email = email;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void GrantTemporaryPermission(string permission, TimeSpan duration, Guid grantedBy)
    {
        TemporaryPermissions.Add(new UserTemporaryPermission(Id, permission, DateTime.UtcNow.Add(duration), grantedBy));
    }
}