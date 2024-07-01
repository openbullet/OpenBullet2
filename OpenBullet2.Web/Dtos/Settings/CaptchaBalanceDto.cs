namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// The result of a captcha balance request.
/// </summary>
public class CaptchaBalanceDto
{
    /// <summary>
    /// The current balance (usually in USD).
    /// </summary>
    public decimal Balance { get; set; }
}
