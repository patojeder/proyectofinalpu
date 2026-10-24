using System.Text.RegularExpressions;
using VentaStock.Domain.Constants;

namespace VentaStock.Domain.Validation;

public static class CustomerDocumentValidator
{
    public static bool TryNormalize(string? type, string? value, out string number, out string? error)
    {
        number = string.Empty;
        error = null;
        value = value?.Trim();
        if (string.IsNullOrEmpty(value) || value.Length > 30)
        {
            error = "Indicá un número de documento válido.";
            return false;
        }
        if (type == DocumentTypes.Dni)
        {
            number = value.Replace(".", "").Replace(" ", "");
            if (number.Length is < 1 or > 8 || number.Any(c => c is < '0' or > '9') || number.All(c => c == '0'))
            {
                error = "El DNI debe tener hasta 8 dígitos y no puede ser cero.";
                return false;
            }
            number = number.PadLeft(8, '0');
            return true;
        }
        if (type is not (DocumentTypes.Cuil or DocumentTypes.Cuit))
        {
            error = "Seleccioná DNI, CUIL o CUIT.";
            return false;
        }
        if (!Regex.IsMatch(value, @"\A(?:[0-9]{11}|[0-9]{2}-[0-9]{8}-[0-9])\z"))
        {
            error = "El CUIL/CUIT debe tener 11 dígitos, con o sin guiones (XX-XXXXXXXX-X).";
            return false;
        }
        number = value.Replace("-", "");
        var prefix = number[..2];
        var validPrefix = type == DocumentTypes.Cuil
            ? prefix is "20" or "23" or "24" or "27"
            : prefix is "20" or "23" or "24" or "27" or "30" or "33" or "34" or "50" or "51" or "55";
        int[] weights = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += (number[i] - '0') * weights[i];
        var digit = 11 - sum % 11;
        if (digit == 11) digit = 0;
        if (!validPrefix || digit != number[10] - '0')
        {
            error = "El CUIL/CUIT no es válido. Revisá el prefijo y el dígito verificador.";
            return false;
        }
        return true;
    }
}
