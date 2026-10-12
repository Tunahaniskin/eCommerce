using System;

namespace ECommerce.API.Modules.Auth.Entities;

public class Address
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string RecipientName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string District { get; private set; } = default!;
    public string FullAddress { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = default!;

    private Address() { } // EF Core

    public Address(Guid userId, string title, string recipientName, string phoneNumber, string city, string district, string fullAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        RecipientName = recipientName;
        PhoneNumber = phoneNumber;
        City = city;
        District = district;
        FullAddress = fullAddress;
        CreatedAt = DateTime.UtcNow;
    }
}
