#nullable enable
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    public static class RunScript
    {
        public static Task<string?> RunScriptAndGetStdOut(string scriptPath)
        {
            TaskCompletionSource<string?> tcs = new();
            var fileExtension = Path.GetExtension(scriptPath).ToLower();
            ProcessStartInfo? startInfo;
            switch (fileExtension)
            {
                case ".bat":
                    startInfo = new("cmd.exe", $@"/C ""{scriptPath}""")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    break;
                case ".ps1":
                    startInfo = new("powershell.exe", $@"""&'{scriptPath}'""")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    break;
                case ".sh":
                    startInfo = new("/bin/bash", scriptPath)
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    break;
                default:
                    // un-supported script.
                    tcs.SetResult(null);
                    return tcs.Task;
            }

            StringBuilder stdOut = new();
            Process process = new() { EnableRaisingEvents = true };

            process.StartInfo = startInfo;
            process.OutputDataReceived += (_, args) => stdOut.AppendLine(args.Data);
            process.Exited += (_, args) =>
            {
                tcs.SetResult(stdOut.Length > 0 ? stdOut.ToString() : null);
                process.Dispose();
            };
            try
            {
                process.Start();
                process.BeginOutputReadLine();
            }
            catch
            {
                // ignored
            }

            return tcs.Task;
        }
    }
}