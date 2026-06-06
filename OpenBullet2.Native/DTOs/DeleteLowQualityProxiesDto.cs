namespace OpenBullet2.Native.DTOs;

public class DeleteLowQualityProxiesDto
{
    public bool DeleteUnknown { get; set; } = true;
    public bool DeleteTransparent { get; set; } = true;
    public bool DeleteAnonymous { get; set; } = true;
}
