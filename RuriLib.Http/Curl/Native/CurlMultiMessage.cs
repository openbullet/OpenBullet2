using System.Runtime.InteropServices;

namespace RuriLib.Http.Curl.Native;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CurlMultiMessage
{
    public readonly CurlMessage Message;
    public readonly nint EasyHandle;
    public readonly CurlCode Result;
}
