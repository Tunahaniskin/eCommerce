using System;

namespace ECommerce.API.Modules.Auth.Features.Profile.GetProfile;

public record GetProfileQuery(Guid UserId);

public record ProfileResponseDto(
    string FirstName,
    string LastName,
    string Email
);
