using System.Text.RegularExpressions;

namespace Dalba.Financeiro.Application.Common;

public static class ValidationHelper
{
    public static string SomenteDigitos(string? value) => Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);

    public static bool IsValidCep(string? cep) => SomenteDigitos(cep).Length == 8;

    public static string FormatCep(string? cep)
    {
        var digits = SomenteDigitos(cep);
        return digits.Length == 8 ? $"{digits[..5]}-{digits[5..]}" : digits;
    }

    /// <summary>
    /// Normaliza CPF/CNPJ removendo pontuação e mantendo alfanuméricos em maiúsculo.
    /// CPF continua só com dígitos; CNPJ preserva letras (novo padrão alfanumérico).
    /// </summary>
    public static string NormalizarDocumento(string? value) =>
        Regex.Replace(value ?? string.Empty, "[^0-9A-Za-z]", string.Empty).ToUpperInvariant();

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email.Trim(), @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$");

    public static bool IsValidCpf(string cpf)
    {
        cpf = SomenteDigitos(cpf);
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;
        return CheckCpfDigit(cpf, 9) == cpf[9] - '0' && CheckCpfDigit(cpf, 10) == cpf[10] - '0';
    }

    public static bool IsValidCnpj(string cnpj)
    {
        cnpj = NormalizarDocumento(cnpj);
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1) return false;
        // 12 primeiros: alfanuméricos (0-9, A-Z); 2 últimos: dígitos verificadores numéricos.
        for (var i = 0; i < 12; i++)
        {
            var c = cnpj[i];
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z'))) return false;
        }
        if (!char.IsDigit(cnpj[12]) || !char.IsDigit(cnpj[13])) return false;
        return CheckCnpjDigit(cnpj, 12) == cnpj[12] - '0' && CheckCnpjDigit(cnpj, 13) == cnpj[13] - '0';
    }

    private static int CheckCpfDigit(string cpf, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++) sum += (cpf[i] - '0') * (length + 1 - i);
        var result = 11 - sum % 11;
        return result >= 10 ? 0 : result;
    }

    private static int CheckCnpjDigit(string cnpj, int length)
    {
        var weights = length == 12 ? new[] { 5,4,3,2,9,8,7,6,5,4,3,2 } : new[] { 6,5,4,3,2,9,8,7,6,5,4,3,2 };
        var sum = 0;
        for (var i = 0; i < length; i++) sum += (cnpj[i] - '0') * weights[i];
        var result = sum % 11;
        return result < 2 ? 0 : 11 - result;
    }
}
