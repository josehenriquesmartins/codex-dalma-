using System.Security.Cryptography;
using System.Text;

namespace Dalba.Financeiro.Application.Common;

public static class SecurityHelper
{
    public static string ComputeSha256(string value)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}
