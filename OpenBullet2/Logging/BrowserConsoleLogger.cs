using Microsoft.JSInterop;
using Microsoft.Scripting.Utils;
using OpenBullet2.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Logging
{
    public class BrowserConsoleLogger
    {
        private readonly IJSRuntime js;
        private readonly string background = "#222";

        public BrowserConsoleLogger(IJSRuntime js)
        {
            this.js = js;
        }

        public async Task LogInfo(string message)
            => await Log(message, "white");

        public async Task LogWarning(string message)
            => await Log(message, "yellow");

        public async Task LogError(string message)
            => await Log(message, "orange");

        public async Task LogException(Exception ex)
        {
            await LogError($"{ex.GetType().Name}: {ex.Message}");
            await js.LogObject(ConvertException(ex));
        }

        private object ConvertException(Exception ex)
        {
            if (ex is AggregateException agg)
            {
                return new
                {
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace,
                    Aggregate = agg.InnerExceptions
                        .Select(inner => ConvertException(inner))
                        .ToArray()
                };
            }

            return new
            {
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace,
                InnerException = ex.InnerException == null
                    ? null
                    : ConvertException(ex.InnerException)
            };
        }

        private async Task Log(string message, string color)
            => await js.LogColored($"[{DateTime.Now.ToLongTimeString()}] {message}", color, background);
    }
}
