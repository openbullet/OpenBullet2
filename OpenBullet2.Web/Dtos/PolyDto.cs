using OpenBullet2.Web.Attributes;
using System.Text.Json.Serialization;

namespace OpenBullet2.Web.Dtos;

/// <summary>
/// Base type for polymorphic classes that can be decorated with
/// a <see cref="PolyTypeAttribute" /> for type discrimination
/// during (de)serialization.
/// </summary>
public class PolyDto
{
    /// <summary>
    /// The polymorphic type name.
    /// </summary>
    [JsonPropertyName("_polyTypeName")]
    [JsonPropertyOrder(-1000)]
    public string PolyTypeName { get; set; } = string.Empty;
}
