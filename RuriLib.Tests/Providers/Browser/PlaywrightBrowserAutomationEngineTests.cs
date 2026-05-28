using RuriLib.Providers.Browser;
using System;
using Xunit;

namespace RuriLib.Tests.Providers.Browser;

public class PlaywrightBrowserAutomationEngineTests
{
    [Theory]
    [InlineData("Unable to retrieve body")]
    [InlineData("Protocol error (Network.getResponseBody): No resource with given identifier found")]
    [InlineData("Protocol error (Network.getResponseBody): Request content was evicted from inspector cache")]
    [InlineData("Protocol error (Network.getResponseBody): Component returned failure code: 0x80004005 (NS_ERROR_FAILURE) [nsIStreamListener.onDataAvailable]")]
    public void IsMissingResponseBodyException_WithUnavailableBodyErrors_ReturnsTrue(string message)
    {
        var ex = new InvalidOperationException("outer", new Exception(message));

        Assert.True(PlaywrightBrowserAutomationEngine.IsMissingResponseBodyException(ex));
    }

    [Fact]
    public void IsMissingResponseBodyException_WithUnrelatedError_ReturnsFalse()
    {
        var ex = new InvalidOperationException("navigation failed");

        Assert.False(PlaywrightBrowserAutomationEngine.IsMissingResponseBodyException(ex));
    }
}
