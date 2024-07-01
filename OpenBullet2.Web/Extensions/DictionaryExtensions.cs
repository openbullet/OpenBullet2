namespace OpenBullet2.Web.Extensions;

static internal class DictionaryExtensions
{
    static internal Dictionary<T, T> MapReverse<T>(this Dictionary<T, T> dict) where T : notnull
    {
        foreach (var kvp in dict.ToArray())
        {
            dict[kvp.Value] = kvp.Key;
        }

        return dict;
    }
}
