using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RuriLib.Functions.Browser;
using RuriLib.Functions.Puppeteer;
using RuriLib.Models.Bots;

namespace RuriLib.Blocks.Puppeteer.Elements;

/// <summary>
/// Compatibility wrappers for legacy Puppeteer element methods.
/// </summary>
[Obsolete("Use the generic Browser blocks instead. This compatibility API is kept for existing compiled configs.")]
public static class Methods
{
    public static Task PuppeteerSetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index, string attributeName,
        string value)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserSetAttributeValue(data, findBy, identifier, index, attributeName, value);

    public static Task PuppeteerTypeElement(BotData data, FindElementBy findBy, string identifier, int index, string text,
        int timeBetweenKeystrokes = 0)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserTypeElement(data, findBy, identifier, index, text, timeBetweenKeystrokes);

    public static Task PuppeteerTypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index, string text)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserTypeElementHuman(data, findBy, identifier, index, text);

    public static Task PuppeteerClick(BotData data, FindElementBy findBy, string identifier, int index,
        PuppeteerSharp.Input.MouseButton mouseButton = PuppeteerSharp.Input.MouseButton.Left, int clickCount = 1, int timeBetweenClicks = 0)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserClick(data, findBy, identifier, index, ToBrowserMouseButton(mouseButton),
            clickCount, timeBetweenClicks);

    public static Task PuppeteerSubmit(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserSubmit(data, findBy, identifier, index);

    public static Task PuppeteerSelect(BotData data, FindElementBy findBy, string identifier, int index, string value)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserSelect(data, findBy, identifier, index, value);

    public static Task PuppeteerSelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserSelectByIndex(data, findBy, identifier, index, selectionIndex);

    public static Task PuppeteerSelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserSelectByText(data, findBy, identifier, index, text);

    public static Task<string> PuppeteerGetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
        string attributeName = "innerText")
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserGetAttributeValue(data, findBy, identifier, index, attributeName);

    public static Task<List<string>> PuppeteerGetAttributeValueAll(BotData data, FindElementBy findBy, string identifier,
        string attributeName = "innerText")
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserGetAttributeValueAll(data, findBy, identifier, attributeName);

    public static Task<bool> PuppeteerIsDisplayed(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserIsDisplayed(data, findBy, identifier, index);

    public static Task<bool> PuppeteerExists(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserExists(data, findBy, identifier, index);

    public static Task PuppeteerUploadFiles(BotData data, FindElementBy findBy, string identifier, int index, List<string> filePaths)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserUploadFiles(data, findBy, identifier, index, filePaths);

    public static Task<int> PuppeteerGetPositionX(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserGetPositionX(data, findBy, identifier, index);

    public static Task<int> PuppeteerGetPositionY(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserGetPositionY(data, findBy, identifier, index);

    public static Task<int> PuppeteerGetWidth(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserGetWidth(data, findBy, identifier, index);

    public static Task<int> PuppeteerGetHeight(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserGetHeight(data, findBy, identifier, index);

    public static Task PuppeteerScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index, string fileName,
        bool fullPage = false, bool omitBackground = false)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserScreenshotElement(data, findBy, identifier, index, fileName, fullPage,
            omitBackground);

    public static Task<string> PuppeteerScreenshotBase64(BotData data, FindElementBy findBy, string identifier, int index,
        bool fullPage = false, bool omitBackground = false)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserScreenshotBase64(data, findBy, identifier, index, fullPage, omitBackground);

    public static Task PuppeteerSwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserSwitchToFrame(data, findBy, identifier, index);

    public static Task PuppeteerWaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
        int timeout = 30000)
        => global::RuriLib.Blocks.Browser.Elements.Methods.BrowserWaitForElement(data, findBy, identifier, hidden, visible, timeout);

    private static BrowserMouseButton ToBrowserMouseButton(PuppeteerSharp.Input.MouseButton mouseButton)
        => Enum.Parse<BrowserMouseButton>(mouseButton.ToString());
}
