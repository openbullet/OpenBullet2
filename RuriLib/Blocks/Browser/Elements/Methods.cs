using System.Collections.Generic;
using System.Threading.Tasks;
using RuriLib.Attributes;
using RuriLib.Functions.Browser;
using RuriLib.Functions.Puppeteer;
using RuriLib.Models.Bots;

namespace RuriLib.Blocks.Browser.Elements;

/// <summary>
/// Blocks for interacting with elements on a browser page.
/// </summary>
[BlockCategory("Elements", "Blocks for interacting with elements on a browser page", "#e9967a")]
public static class Methods
{
    /// <summary>
    /// Sets an attribute value on an element.
    /// </summary>
    [Block("Sets the value of the specified attribute of an element", name = "Set Attribute Value",
        aliases = ["PuppeteerSetAttributeValue"])]
    public static Task BrowserSetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
        string attributeName, string value)
        => data.Providers.BrowserAutomation.Resolve(data).SetAttributeValue(data, findBy, identifier, index, attributeName, value);

    /// <summary>
    /// Types text into an element.
    /// </summary>
    [Block("Types text in an input field", name = "Type", aliases = ["PuppeteerTypeElement"])]
    public static Task BrowserTypeElement(BotData data, FindElementBy findBy, string identifier, int index,
        string text, int timeBetweenKeystrokes = 0)
        => data.Providers.BrowserAutomation.Resolve(data).TypeElement(data, findBy, identifier, index, text, timeBetweenKeystrokes);

    /// <summary>
    /// Types text into an element with human-like delays.
    /// </summary>
    [Block("Types text in an input field with human-like random delays", name = "Type Human", aliases = ["PuppeteerTypeElementHuman"])]
    public static Task BrowserTypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index, string text)
        => data.Providers.BrowserAutomation.Resolve(data).TypeElementHuman(data, findBy, identifier, index, text);

    /// <summary>
    /// Clicks an element.
    /// </summary>
    [Block("Clicks an element", name = "Click", aliases = ["PuppeteerClick"])]
    public static Task BrowserClick(BotData data, FindElementBy findBy, string identifier, int index,
        BrowserMouseButton mouseButton = BrowserMouseButton.Left, int clickCount = 1, int timeBetweenClicks = 0)
        => data.Providers.BrowserAutomation.Resolve(data).Click(data, findBy, identifier, index, mouseButton, clickCount, timeBetweenClicks);

    /// <summary>
    /// Submits the form that contains the element.
    /// </summary>
    [Block("Submits a form", name = "Submit", aliases = ["PuppeteerSubmit"])]
    public static Task BrowserSubmit(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).Submit(data, findBy, identifier, index);

    /// <summary>
    /// Selects a value in a select element.
    /// </summary>
    [Block("Selects a value in a select element", name = "Select", aliases = ["PuppeteerSelect"])]
    public static Task BrowserSelect(BotData data, FindElementBy findBy, string identifier, int index, string value)
        => data.Providers.BrowserAutomation.Resolve(data).Select(data, findBy, identifier, index, value);

    /// <summary>
    /// Selects an option by index in a select element.
    /// </summary>
    [Block("Selects a value by index in a select element", name = "Select by Index", aliases = ["PuppeteerSelectByIndex"])]
    public static Task BrowserSelectByIndex(BotData data, FindElementBy findBy, string identifier, int index, int selectionIndex)
        => data.Providers.BrowserAutomation.Resolve(data).SelectByIndex(data, findBy, identifier, index, selectionIndex);

    /// <summary>
    /// Selects an option by visible text in a select element.
    /// </summary>
    [Block("Selects a value by text in a select element", name = "Select by Text", aliases = ["PuppeteerSelectByText"])]
    public static Task BrowserSelectByText(BotData data, FindElementBy findBy, string identifier, int index, string text)
        => data.Providers.BrowserAutomation.Resolve(data).SelectByText(data, findBy, identifier, index, text);

    /// <summary>
    /// Gets an attribute value from an element.
    /// </summary>
    [Block("Gets the value of an attribute of an element", name = "Get Attribute Value", aliases = ["PuppeteerGetAttributeValue"])]
    public static Task<string> BrowserGetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
        string attributeName = "innerText")
        => data.Providers.BrowserAutomation.Resolve(data).GetAttributeValue(data, findBy, identifier, index, attributeName);

    /// <summary>
    /// Gets an attribute value from all matching elements.
    /// </summary>
    [Block("Gets the values of an attribute of multiple elements", name = "Get Attribute Value All",
        aliases = ["PuppeteerGetAttributeValueAll"])]
    public static Task<List<string>> BrowserGetAttributeValueAll(BotData data, FindElementBy findBy, string identifier,
        string attributeName = "innerText")
        => data.Providers.BrowserAutomation.Resolve(data).GetAttributeValueAll(data, findBy, identifier, attributeName);

    /// <summary>
    /// Checks whether an element is displayed.
    /// </summary>
    [Block("Checks if an element is currently being displayed on the page", name = "Is Displayed", aliases = ["PuppeteerIsDisplayed"])]
    public static Task<bool> BrowserIsDisplayed(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).IsDisplayed(data, findBy, identifier, index);

    /// <summary>
    /// Checks whether an element exists.
    /// </summary>
    [Block("Checks if an element exists on the page", name = "Exists", aliases = ["PuppeteerExists"])]
    public static Task<bool> BrowserExists(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).Exists(data, findBy, identifier, index);

    /// <summary>
    /// Uploads files through an element.
    /// </summary>
    [Block("Uploads one or more files to the selected element", name = "Upload Files", aliases = ["PuppeteerUploadFiles"])]
    public static Task BrowserUploadFiles(BotData data, FindElementBy findBy, string identifier, int index, List<string> filePaths)
        => data.Providers.BrowserAutomation.Resolve(data).UploadFiles(data, findBy, identifier, index, filePaths);

    /// <summary>
    /// Gets the horizontal position of an element.
    /// </summary>
    [Block("Gets the X coordinate of the element in pixels", name = "Get Position X", aliases = ["PuppeteerGetPositionX"])]
    public static Task<int> BrowserGetPositionX(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).GetPositionX(data, findBy, identifier, index);

    /// <summary>
    /// Gets the vertical position of an element.
    /// </summary>
    [Block("Gets the Y coordinate of the element in pixels", name = "Get Position Y", aliases = ["PuppeteerGetPositionY"])]
    public static Task<int> BrowserGetPositionY(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).GetPositionY(data, findBy, identifier, index);

    /// <summary>
    /// Gets the width of an element.
    /// </summary>
    [Block("Gets the width of the element in pixels", name = "Get Width", aliases = ["PuppeteerGetWidth"])]
    public static Task<int> BrowserGetWidth(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).GetWidth(data, findBy, identifier, index);

    /// <summary>
    /// Gets the height of an element.
    /// </summary>
    [Block("Gets the height of the element in pixels", name = "Get Height", aliases = ["PuppeteerGetHeight"])]
    public static Task<int> BrowserGetHeight(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).GetHeight(data, findBy, identifier, index);

    /// <summary>
    /// Saves a screenshot of an element to a file.
    /// </summary>
    [Block("Takes a screenshot of the element and saves it to an output file", name = "Screenshot Element",
        aliases = ["PuppeteerScreenshotElement"])]
    public static Task BrowserScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index,
        string fileName, bool fullPage = false, bool omitBackground = false)
        => data.Providers.BrowserAutomation.Resolve(data).ScreenshotElement(data, findBy, identifier, index, fileName, fullPage, omitBackground);

    /// <summary>
    /// Captures an element screenshot and returns it as a base64 string.
    /// </summary>
    [Block("Takes a screenshot of the element and converts it to a base64 string", name = "Screenshot Element Base64",
        aliases = ["PuppeteerScreenshotBase64"])]
    public static Task<string> BrowserScreenshotBase64(BotData data, FindElementBy findBy, string identifier, int index,
        bool fullPage = false, bool omitBackground = false)
        => data.Providers.BrowserAutomation.Resolve(data).ScreenshotElementBase64(data, findBy, identifier, index, fullPage, omitBackground);

    /// <summary>
    /// Switches the current context to an iframe.
    /// </summary>
    [Block("Switches to a different iframe", name = "Switch to Frame", aliases = ["PuppeteerSwitchToFrame"])]
    public static Task BrowserSwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index)
        => data.Providers.BrowserAutomation.Resolve(data).SwitchToFrame(data, findBy, identifier, index);

    /// <summary>
    /// Waits for an element to reach the requested visibility state.
    /// </summary>
    [Block("Waits for an element to appear on the page", name = "Wait for Element", aliases = ["PuppeteerWaitForElement"])]
    public static Task BrowserWaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
        int timeout = 30000)
        => data.Providers.BrowserAutomation.Resolve(data).WaitForElement(data, findBy, identifier, hidden, visible, timeout);
}
