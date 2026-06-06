using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RuriLib.Http.Curl.Internal;

internal sealed record CurlImpersonateCustomProfile(
    string Ja3,
    string Akamai,
    IReadOnlyList<string> TlsSignatureAlgorithms,
    bool TlsGrease,
    string? TlsCertCompression = null,
    int? Http2StreamWeight = null,
    bool? Http2StreamExclusive = null,
    bool Http2NoPriority = false)
{
    private const string SafariIos15To18Ja3 =
        "771," +
        "4865-4866-4867-49196-49195-52393-49200-49199-52392-49162-49161-49172-49171-157-156-53-47-49160-49170-10," +
        "0-23-65281-10-11-16-5-13-18-51-45-43-27-21," +
        "29-23-24-25," +
        "0";

    private const string OkHttp4Android10Ja3 =
        "771," +
        "4865-4866-4867-49195-49196-52393-49199-49200-52392-49171-49172-156-157-47-53," +
        "0-23-65281-10-11-35-16-5-13-51-45-43-21," +
        "29-23-24," +
        "0";

    private static readonly string[] SafariIos15To17SignatureAlgorithms =
    [
        "ecdsa_secp256r1_sha256",
        "rsa_pss_rsae_sha256",
        "rsa_pkcs1_sha256",
        "ecdsa_secp384r1_sha384",
        "ecdsa_sha1",
        "rsa_pss_rsae_sha384",
        "rsa_pss_rsae_sha384",
        "rsa_pkcs1_sha384",
        "rsa_pss_rsae_sha512",
        "rsa_pkcs1_sha512",
        "rsa_pkcs1_sha1"
    ];

    private static readonly string[] SafariIos18SignatureAlgorithms =
    [
        "ecdsa_secp256r1_sha256",
        "rsa_pss_rsae_sha256",
        "rsa_pkcs1_sha256",
        "ecdsa_secp384r1_sha384",
        "rsa_pss_rsae_sha384",
        "rsa_pss_rsae_sha384",
        "rsa_pkcs1_sha384",
        "rsa_pss_rsae_sha512",
        "rsa_pkcs1_sha512",
        "rsa_pkcs1_sha1"
    ];

    private static readonly string[] OkHttp4Android10SignatureAlgorithms =
    [
        "ecdsa_secp256r1_sha256",
        "rsa_pss_rsae_sha256",
        "rsa_pkcs1_sha256",
        "ecdsa_secp384r1_sha384",
        "rsa_pss_rsae_sha384",
        "rsa_pkcs1_sha384",
        "rsa_pss_rsae_sha512",
        "rsa_pkcs1_sha512",
        "rsa_pkcs1_sha1"
    ];

    private static readonly CurlImpersonateCustomProfile SafariIos15To16 = new(
        SafariIos15To18Ja3,
        "4:2097152;3:100|10485760|0|m,s,p,a",
        SafariIos15To17SignatureAlgorithms,
        TlsGrease: true,
        TlsCertCompression: "zlib");

    private static readonly CurlImpersonateCustomProfile SafariIos17 = new(
        SafariIos15To18Ja3,
        "2:0;4:2097152;3:100|10485760|0|m,s,p,a",
        SafariIos15To17SignatureAlgorithms,
        TlsGrease: true,
        TlsCertCompression: "zlib");

    private static readonly CurlImpersonateCustomProfile SafariIos18 = new(
        SafariIos15To18Ja3,
        "2:0;3:100;4:2097152;9:1|10420225|0|m,s,a,p",
        SafariIos18SignatureAlgorithms,
        TlsGrease: true,
        TlsCertCompression: "zlib",
        Http2StreamWeight: 256,
        Http2StreamExclusive: false,
        Http2NoPriority: true);

    private static readonly CurlImpersonateCustomProfile OkHttp4Android10 = new(
        OkHttp4Android10Ja3,
        "4:16777216|16711681|0|m,p,a,s",
        OkHttp4Android10SignatureAlgorithms,
        TlsGrease: false,
        Http2StreamWeight: 1,
        Http2StreamExclusive: false);

    public static bool TryGet(CurlImpersonateBrowserProfile profile,
        [NotNullWhen(true)] out CurlImpersonateCustomProfile? customProfile)
    {
        customProfile = profile switch
        {
            CurlImpersonateBrowserProfile.SafariIpad156
                or CurlImpersonateBrowserProfile.SafariIos155
                or CurlImpersonateBrowserProfile.SafariIos156
                or CurlImpersonateBrowserProfile.SafariIos160 => SafariIos15To16,
            CurlImpersonateBrowserProfile.SafariIos170 => SafariIos17,
            CurlImpersonateBrowserProfile.SafariIos185 => SafariIos18,
            CurlImpersonateBrowserProfile.Okhttp4Android10
                or CurlImpersonateBrowserProfile.Okhttp4Android11
                or CurlImpersonateBrowserProfile.Okhttp4Android12
                or CurlImpersonateBrowserProfile.Okhttp4Android13 => OkHttp4Android10,
            _ => null
        };

        return customProfile is not null;
    }
}
