using System;

namespace RuriLib.Http.Curl;

/// <summary>
/// Browser profiles supported by curl-impersonate.
/// </summary>
public enum CurlImpersonateBrowserProfile
{
    /// <summary>Chrome 99.</summary>
    Chrome99,
    /// <summary>Chrome 100.</summary>
    Chrome100,
    /// <summary>Chrome 101.</summary>
    Chrome101,
    /// <summary>Chrome 104.</summary>
    Chrome104,
    /// <summary>Chrome 107.</summary>
    Chrome107,
    /// <summary>Chrome 110.</summary>
    Chrome110,
    /// <summary>Chrome 116.</summary>
    Chrome116,
    /// <summary>Chrome 119.</summary>
    Chrome119,
    /// <summary>Chrome 120.</summary>
    Chrome120,
    /// <summary>Chrome 123.</summary>
    Chrome123,
    /// <summary>Chrome 124.</summary>
    Chrome124,
    /// <summary>Chrome 131.</summary>
    Chrome131,
    /// <summary>Chrome 133a.</summary>
    Chrome133a,
    /// <summary>Chrome 136.</summary>
    Chrome136,
    /// <summary>Chrome 142.</summary>
    Chrome142,
    /// <summary>Chrome 145.</summary>
    Chrome145,
    /// <summary>Chrome 146.</summary>
    Chrome146,
    /// <summary>Chrome 99 on Android.</summary>
    Chrome99Android,
    /// <summary>Chrome 131 on Android.</summary>
    Chrome131Android,
    /// <summary>Edge 99.</summary>
    Edge99,
    /// <summary>Edge 101.</summary>
    Edge101,
    /// <summary>Safari 15.3.</summary>
    Safari153,
    /// <summary>Safari 15.5.</summary>
    Safari155,
    /// <summary>Safari 17.0.</summary>
    Safari170,
    /// <summary>Safari 17.2 on iOS.</summary>
    Safari172Ios,
    /// <summary>Safari 18.0.</summary>
    Safari180,
    /// <summary>Safari 18.0 on iOS.</summary>
    Safari180Ios,
    /// <summary>Safari 18.4.</summary>
    Safari184,
    /// <summary>Safari 18.4 on iOS.</summary>
    Safari184Ios,
    /// <summary>Safari 26.0.</summary>
    Safari260,
    /// <summary>Safari 26.0 on iOS.</summary>
    Safari260Ios,
    /// <summary>Firefox 133.</summary>
    Firefox133,
    /// <summary>Firefox 135.</summary>
    Firefox135,
    /// <summary>Firefox 144.</summary>
    Firefox144,
    /// <summary>Firefox 147.</summary>
    Firefox147,
    /// <summary>Tor Browser 145.</summary>
    Tor145,
    /// <summary>Safari on iPadOS 15.6.</summary>
    SafariIpad156,
    /// <summary>Safari on iOS 15.5.</summary>
    SafariIos155,
    /// <summary>Safari on iOS 15.6.</summary>
    SafariIos156,
    /// <summary>Safari on iOS 16.0.</summary>
    SafariIos160,
    /// <summary>Safari on iOS 17.0.</summary>
    SafariIos170,
    /// <summary>Safari on iOS 18.5.</summary>
    SafariIos185,
    /// <summary>OkHttp 4 on Android 10.</summary>
    Okhttp4Android10,
    /// <summary>OkHttp 4 on Android 11.</summary>
    Okhttp4Android11,
    /// <summary>OkHttp 4 on Android 12.</summary>
    Okhttp4Android12,
    /// <summary>OkHttp 4 on Android 13.</summary>
    Okhttp4Android13
}

/// <summary>
/// Helpers for curl-impersonate browser profiles.
/// </summary>
public static class CurlImpersonateBrowserProfileExtensions
{
    /// <summary>
    /// Gets the target string passed to <c>curl_easy_impersonate</c>.
    /// </summary>
    public static string ToCurlTarget(this CurlImpersonateBrowserProfile profile) => profile switch
    {
        CurlImpersonateBrowserProfile.Chrome99 => "chrome99",
        CurlImpersonateBrowserProfile.Chrome100 => "chrome100",
        CurlImpersonateBrowserProfile.Chrome101 => "chrome101",
        CurlImpersonateBrowserProfile.Chrome104 => "chrome104",
        CurlImpersonateBrowserProfile.Chrome107 => "chrome107",
        CurlImpersonateBrowserProfile.Chrome110 => "chrome110",
        CurlImpersonateBrowserProfile.Chrome116 => "chrome116",
        CurlImpersonateBrowserProfile.Chrome119 => "chrome119",
        CurlImpersonateBrowserProfile.Chrome120 => "chrome120",
        CurlImpersonateBrowserProfile.Chrome123 => "chrome123",
        CurlImpersonateBrowserProfile.Chrome124 => "chrome124",
        CurlImpersonateBrowserProfile.Chrome131 => "chrome131",
        CurlImpersonateBrowserProfile.Chrome133a => "chrome133a",
        CurlImpersonateBrowserProfile.Chrome136 => "chrome136",
        CurlImpersonateBrowserProfile.Chrome142 => "chrome142",
        CurlImpersonateBrowserProfile.Chrome145 => "chrome145",
        CurlImpersonateBrowserProfile.Chrome146 => "chrome146",
        CurlImpersonateBrowserProfile.Chrome99Android => "chrome99_android",
        CurlImpersonateBrowserProfile.Chrome131Android => "chrome131_android",
        CurlImpersonateBrowserProfile.Edge99 => "edge99",
        CurlImpersonateBrowserProfile.Edge101 => "edge101",
        CurlImpersonateBrowserProfile.Safari153 => "safari153",
        CurlImpersonateBrowserProfile.Safari155 => "safari155",
        CurlImpersonateBrowserProfile.Safari170 => "safari170",
        CurlImpersonateBrowserProfile.Safari172Ios => "safari172_ios",
        CurlImpersonateBrowserProfile.Safari180 => "safari180",
        CurlImpersonateBrowserProfile.Safari180Ios => "safari180_ios",
        CurlImpersonateBrowserProfile.Safari184 => "safari184",
        CurlImpersonateBrowserProfile.Safari184Ios => "safari184_ios",
        CurlImpersonateBrowserProfile.Safari260 => "safari260",
        CurlImpersonateBrowserProfile.Safari260Ios => "safari260_ios",
        CurlImpersonateBrowserProfile.Firefox133 => "firefox133",
        CurlImpersonateBrowserProfile.Firefox135 => "firefox135",
        CurlImpersonateBrowserProfile.Firefox144 => "firefox144",
        CurlImpersonateBrowserProfile.Firefox147 => "firefox147",
        CurlImpersonateBrowserProfile.Tor145 => "tor145",
        _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unknown curl-impersonate browser profile")
    };
}
