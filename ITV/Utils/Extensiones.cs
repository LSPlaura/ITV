using System.Text;

namespace ITV.Utils;

public static class Extensiones
{
    /// <summary>
    /// Capitaliza (Poner la primera letra en mayúscula y el resto en minúscula) un string
    /// </summary>
    /// <param name="cadena">El string que se quiere capitalizar</param>
    /// <returns>El string capitalizado</returns>
    public static string ToCapitalize(this string cadena)
    {
        if (string.IsNullOrWhiteSpace(cadena)) return cadena;
        var sb = new StringBuilder();
        sb.Append(cadena.ToUpper()[0]);
        sb.Append(cadena.ToLower().Substring(1, cadena.Length - 1));
        return sb.ToString();
    }
}