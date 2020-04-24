using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class IJSRuntimeExtensions
    {
        public async static Task<bool> Confirm(this IJSRuntime js, string message)
            => await js.InvokeAsync<bool>("confirm", message);

        public async static Task Log(this IJSRuntime js, string message)
            => await js.InvokeVoidAsync("console.log", message);

        public async static Task Alert(this IJSRuntime js, string message)
            => await js.InvokeVoidAsync("Swal.fire", message);

        public async static Task AlertSuccess(this IJSRuntime js, string title, string message)
            => await js.InvokeVoidAsync("Swal.fire", title, message, "success");

        public async static Task AlertError(this IJSRuntime js, string title, string message)
            => await js.InvokeVoidAsync("Swal.fire", title, message, "error");
    }
}
