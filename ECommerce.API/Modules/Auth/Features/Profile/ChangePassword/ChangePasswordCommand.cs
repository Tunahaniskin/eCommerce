using System;

namespace ECommerce.API.Modules.Auth.Features.Profile.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
);
