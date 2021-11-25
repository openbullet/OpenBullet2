using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class IJSRuntimeExtensions
    {
        public async static Task Log(this IJSRuntime js, string message)
            => await js.InvokeVoidAsync("console.log", message);

        public async static Task LogObject(this IJSRuntime js, object obj)
            => await js.InvokeVoidAsync("console.log", obj);

        public async static Task LogColored(this IJSRuntime js, string message, string foreground = "#000", string background = "#fff")
            => await js.InvokeVoidAsync("console.log", $"%c {message} ", $"background: {background}; color: {foreground}; padding: 5px 0px");

        public async static Task Alert(this IJSRuntime js, string title, string message)
            => await js.InvokeVoidAsync("Swal.fire", title, message, "info");

        public async static Task AlertSuccess(this IJSRuntime js, string title, string message)
            => await js.InvokeVoidAsync("Swal.fire", title, message, "success");

        public async static Task AlertError(this IJSRuntime js, string title, string message)
            => await js.InvokeVoidAsync("Swal.fire", title, message, "error");

        public async static Task AlertWarning(this IJSRuntime js, string title, string message)
            => await js.InvokeVoidAsync("Swal.fire", title, message, "warning");

        public async static Task AlertException(this IJSRuntime js, Exception ex)
        {
#if (DEBUG)
            await js.Log(ex.ToString());
#endif
            await js.AlertError(ex.GetType().Name, ex.Message);
        }

        public async static Task<bool> Confirm(this IJSRuntime js, string question, string message, string cancelText = "Cancel")
        {
            var options = new
            {
                title = question,
                text = message,
                icon = "warning",
                showCancelButton = true,
                confirmButtonColor = "#3085d6",
                cancelButtonColor = "#d33",
                cancelButtonText = cancelText,
                confirmButtonText = "OK"
            };

            var result = await js.InvokeAsync<object>("Swal.fire", options);
            var obj = JObject.Parse(result.ToString());

            if (obj.TryGetValue("value", out JToken value))
                return value.ToObject<bool>();

            return false;
        }

        public async static Task CopyToClipboard(this IJSRuntime js, string text)
            => await js.InvokeVoidAsync("navigator.clipboard.writeText", text);

        public async static Task RegisterLoliCode(this IJSRuntime js)
            => await js.InvokeVoidAsync("registerLoliCode");

        public async static Task RegisterLoliScript(this IJSRuntime js)
            => await js.InvokeVoidAsync("registerLoliScript");
    }
}
