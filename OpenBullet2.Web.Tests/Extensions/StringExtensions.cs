using System.Collections.Specialized;

namespace OpenBullet2.Web.Tests.Extensions;

public static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
    
    private static Uri ToUri(this string relativePath, NameValueCollection queryParams)
    {
        var queryString = string.Join('&',
            queryParams.AllKeys
                .Where(k => k is not null && queryParams[k] is not null)
                .Select(k => $"{Uri.EscapeDataString(k!)}={Uri.EscapeDataString(queryParams[k]!)}"));
        
        return new Uri($"{relativePath}?{queryString}", UriKind.Relative);
    }
    
    public static Uri ToUri<T>(this string relativePath, T dto) where T : class
    {
        var queryParams = new NameValueCollection();
        
        foreach (var property in dto.GetType().GetProperties())
        {
            var value = property.GetValue(dto);
            string? stringValue;
            
            // If it's an enum, convert it to a camel case string
            if (value is Enum enumValue)
                stringValue = enumValue.ToString().ToCamelCase();
            else if (value is DateTime dateTimeValue)
                stringValue = dateTimeValue.ToString("O");
            else
                stringValue = value?.ToString();
            
            queryParams.Add(property.Name.ToCamelCase(), stringValue);
        }
        
        return relativePath.ToUri(queryParams);
    }
}
