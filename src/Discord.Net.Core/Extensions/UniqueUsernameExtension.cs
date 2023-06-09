using System.Text.RegularExpressions;

namespace Discord;

public static class UniqueUsernameExtension
{
    // According to the new username restrictions, a valid username must:
    // - Be between 2 and 32 characters long.
    // - Contain only lower-case alphanumeric characters, underscores, dots and dashes.
    // - Not contain two consecutive dots.
    private static bool ValidateUsername(string name) =>
        Regex.IsMatch(name, "^(?!.*\\.\\.)[a-z0-9\\._]{2,32}$");

    public static bool HasValidUniqueUsername(this IUser user) =>
        ValidateUsername(user.Username);

    public static bool HasLegacyUsername(this IUser user) =>
        user.Discriminator != null && user.Discriminator != "0";

    public static bool HasUniqueUsername(this IUser user) => !user.HasLegacyUsername();
}
