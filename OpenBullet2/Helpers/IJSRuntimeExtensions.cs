using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class IJSRuntimeExtensions
    {
        public async static Task<bool> Confirm(this IJSRuntime jsRuntime, string message)
            => await jsRuntime.InvokeAsync<bool>("confirm", message);

        public async static Task Log(this IJSRuntime jsRuntime, string message)
            => await jsRuntime.InvokeVoidAsync("console.log", message);
    }
}
