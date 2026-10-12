using System;

namespace ECommerce.API.Modules.Auth.Features.Profile.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email
);
