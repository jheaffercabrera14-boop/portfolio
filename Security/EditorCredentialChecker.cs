using System.Security.Cryptography;
using System.Text;

namespace Portfolio.Security;

/// <summary>
/// Editor credentials come from configuration (user-secrets / environment variables), NEVER from the repository.
///   Editor:Username  /  Editor:Password      (env vars: Editor__Username, Editor__Password)
/// Comparison is done on SHA-256 hashes with FixedTimeEquals so timing does not leak the password.
/// </summary>
public class EditorCredentialChecker
{
    private readonly string? _user;
    private readonly string? _pass;

    public EditorCredentialChecker(IConfiguration config)
    {
        _user = config["Editor:Username"];
        _pass = config["Editor:Password"];
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_pass);

    public bool Validate(string? username, string? password)
    {
        if (!IsConfigured) return false;
        bool userOk = SameHash(username, _user!);
        bool passOk = SameHash(password, _pass!);
        return userOk & passOk;   // single & on purpose: always evaluate both
    }

    private static bool SameHash(string? given, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(given ?? "")),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
}
