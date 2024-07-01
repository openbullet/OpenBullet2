using CommandLine;

namespace OpenBullet2.Web.Updater;

public class CliOptions
{
    [Option('r', "repository", Required = false, HelpText = "The repository, e.g. openbullet/OpenBullet2")]
    public string Repository { get; set; } = "openbullet/OpenBullet2";
    
    [Option('u', "username", Required = false, HelpText = "The username to authenticate to the repository if private")]
    public string? Username { get; set; }
    
    [Option('t', "token", Required = false, HelpText = "The token to authenticate to the repository if private")]
    public string? Token { get; set; }
    
    [Option('c', "channel", Required = false, HelpText = "The channel to use for updates (staging, release)")]
    public BuildChannel? Channel { get; set; } = null;
}
