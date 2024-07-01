using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// A byte array block parameter.
/// </summary>
[PolyType("byteArrayParam")]
[MapsFrom(typeof(ByteArrayParameter))]
public class ByteArrayBlockParameterDto : BlockParameterDto
{
    /// <summary>
    /// The default value.
    /// </summary>
    public byte[] DefaultValue { get; set; } = Array.Empty<byte>();
}
