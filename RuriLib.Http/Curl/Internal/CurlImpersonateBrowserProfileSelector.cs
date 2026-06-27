using System;

namespace RuriLib.Http.Curl.Internal;

internal static class CurlImpersonateBrowserProfileSelector
{
    // Sources accessed on 2026-06-08:
    // - StatCounter desktop browser market share worldwide, May 2026:
    //   https://gs.statcounter.com/browser-market-share/desktop/worldwide
    // - StatCounter mobile browser version market share worldwide, April 2026:
    //   https://gs.statcounter.com/browser-version-market-share/mobile/worldwide
    // - StatCounter desktop/mobile/tablet platform share worldwide, May 2026:
    //   https://gs.statcounter.com/platform-market-share/desktop-mobile-tablet/worldwide
    // - StatCounter tablet browser market share worldwide, May 2026:
    //   https://gs.statcounter.com/browser-market-share/tablet/worldwide
    //
    // Unsupported browser families are omitted and the remaining weights are normalized during
    // selection instead of mapping them to unrelated fingerprints.

    // StatCounter worldwide market share for May 2026.
    private static readonly WeightedProfile[] DesktopProfiles =
    [
        new(CurlImpersonateBrowserProfile.Chrome146, 74.93),
        new(CurlImpersonateBrowserProfile.Edge101, 9.94),
        new(CurlImpersonateBrowserProfile.Safari260, 5.32),
        new(CurlImpersonateBrowserProfile.Firefox147, 3.81)
    ];

    // The April 2026 version split distinguishes Chrome for Android from Chrome on iPhone. Since
    // curl-impersonate has no Chrome-on-iOS profile, iPhone Chrome traffic is represented by WebKit.
    private static readonly WeightedProfile[] MobileProfiles =
    [
        new(CurlImpersonateBrowserProfile.Chrome131Android, 59.97),
        new(CurlImpersonateBrowserProfile.Safari260Ios, 31.37)
    ];

    // May 2026 platform shares combine the desktop/mobile distributions above. Tablet traffic is
    // represented separately using the available Android and iPad fingerprints.
    private static readonly WeightedProfile[] AllProfiles =
    [
        new(CurlImpersonateBrowserProfile.Chrome146, 3614.6232),
        new(CurlImpersonateBrowserProfile.Edge101, 481.0004),
        new(CurlImpersonateBrowserProfile.Safari260, 256.6368),
        new(CurlImpersonateBrowserProfile.Firefox147, 183.7944),
        new(CurlImpersonateBrowserProfile.Chrome131Android, 3111.1737),
        new(CurlImpersonateBrowserProfile.Safari260Ios, 1577.5973),
        new(CurlImpersonateBrowserProfile.SafariIpad156, 46.1908)
    ];

    public static CurlImpersonateBrowserProfile Resolve(CurlImpersonateBrowserProfile profile)
        => Resolve(profile, Random.Shared.NextDouble());

    internal static CurlImpersonateBrowserProfile Resolve(CurlImpersonateBrowserProfile profile, double sample)
    {
        var distribution = profile switch
        {
            CurlImpersonateBrowserProfile.Random => AllProfiles,
            CurlImpersonateBrowserProfile.RandomBrowser => DesktopProfiles,
            CurlImpersonateBrowserProfile.RandomMobile => MobileProfiles,
            _ => null
        };

        return distribution is null ? profile : Select(distribution, sample);
    }

    private static CurlImpersonateBrowserProfile Select(WeightedProfile[] distribution, double sample)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sample);

        if (sample >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sample), sample, "The random sample must be less than 1.");
        }

        double totalWeight = 0;
        foreach (var entry in distribution)
        {
            totalWeight += entry.Weight;
        }

        var value = sample * totalWeight;
        double cumulativeWeight = 0;
        foreach (var entry in distribution)
        {
            cumulativeWeight += entry.Weight;
            if (value < cumulativeWeight)
            {
                return entry.Profile;
            }
        }

        return distribution[^1].Profile;
    }

    private readonly record struct WeightedProfile(CurlImpersonateBrowserProfile Profile, double Weight);
}
