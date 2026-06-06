using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace RuriLib.Tests.Utils;

public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Requires Windows GDI+ support";
        }
    }
}
