namespace DotPulsar.Internal;

using DotPulsar.Abstractions;
using System.Text;

public class AuthenticationBasic : IAuthentication
{
    private readonly Func<CancellationToken, ValueTask<string>> _authenticationDataGetter;
    public string AuthenticationMethodName => "Basic";
    private string _userId;
    private string _password;
    public AuthenticationBasic(string userId, string password)
    {
        _userId = userId;
        _password = password;
        _authenticationDataGetter = (cancellationToken) => new ValueTask<string>($"{userId}:{password}");
    }
    public async ValueTask<byte[]> GetAuthenticationData(CancellationToken cancellationToken)
    {
        string authData = await _authenticationDataGetter(cancellationToken);
        string base64AuthData = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));
        return Encoding.UTF8.GetBytes($"Authorization Basic {base64AuthData}");
    }
}
