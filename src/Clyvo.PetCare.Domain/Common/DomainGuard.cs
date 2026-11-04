using System.Text.RegularExpressions;
using Clyvo.PetCare.Domain.Exceptions;

namespace Clyvo.PetCare.Domain.Common;

/// <summary>
/// Cláusulas de guarda reutilizadas pelas entidades para proteger suas invariantes.
/// Cada método valida e devolve o valor já normalizado (trim, caixa, nulos),
/// lançando <see cref="DomainException"/> quando a regra é violada.
/// </summary>
public static class DomainGuard
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string TextoObrigatorio(string? valor, string campo, int tamanhoMaximo, int tamanhoMinimo = 1)
    {
        var texto = valor?.Trim();

        if (string.IsNullOrEmpty(texto))
            throw new DomainException($"{campo} é obrigatório.");

        if (texto.Length < tamanhoMinimo || texto.Length > tamanhoMaximo)
            throw new DomainException($"{campo} deve ter entre {tamanhoMinimo} e {tamanhoMaximo} caracteres.");

        return texto;
    }

    public static string? TextoOpcional(string? valor, string campo, int tamanhoMaximo)
    {
        var texto = valor?.Trim();

        if (string.IsNullOrEmpty(texto))
            return null;

        if (texto.Length > tamanhoMaximo)
            throw new DomainException($"{campo} deve ter no máximo {tamanhoMaximo} caracteres.");

        return texto;
    }

    public static string Email(string? valor, string campo, int tamanhoMaximo)
    {
        var email = TextoObrigatorio(valor, campo, tamanhoMaximo).ToLowerInvariant();

        if (!EmailRegex.IsMatch(email))
            throw new DomainException($"{campo} em formato inválido.");

        return email;
    }

    public static TEnum EnumDefinido<TEnum>(TEnum valor, string campo) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(valor))
            throw new DomainException($"{campo} possui valor inválido: {valor}.");

        return valor;
    }

    public static decimal Intervalo(decimal valor, decimal minimo, decimal maximo, string campo)
    {
        if (valor < minimo || valor > maximo)
            throw new DomainException($"{campo} deve estar entre {minimo} e {maximo}.");

        return valor;
    }

    public static DateTime DataInformada(DateTime valor, string campo)
    {
        if (valor == default)
            throw new DomainException($"{campo} é obrigatória.");

        return valor;
    }

    public static DateTime NaoFutura(DateTime valor, string campo)
    {
        DataInformada(valor, campo);

        if (valor.Date > DateTime.Today)
            throw new DomainException($"{campo} não pode ser uma data futura.");

        return valor;
    }

    public static T NaoNulo<T>(T? valor, string campo) where T : class
        => valor ?? throw new DomainException($"{campo} é obrigatório.");
}
