using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RuriLib.Functions.Files;
using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// The available ways to address an element.
    /// </summary>
    public enum ElementLocator
    {
        /// <summary>The id of the element.</summary>
        Id,

        /// <summary>The class of the element.</summary>
        Class,

        /// <summary>The name of the element.</summary>
        Name,

        /// <summary>The tag of the element.</summary>
        Tag,

        /// <summary>The CSS selector of the element.</summary>
        Selector,

        /// <summary>The xpath of the element.</summary>
        XPath
    }

    /// <summary>
    /// The actions that can be performed on an element.
    /// </summary>
    public enum ElementAction
    {
        /// <summary>Clears the text of an input element.</summary>
        Clear,

        /// <summary>Sends keystrokes to an input element.</summary>
        SendKeys,

        /// <summary>Types keystrokes into an input element with random delays between each keystroke, like a human would.</summary>
        SendKeysHuman,

        /// <summary>Clicks an element.</summary>
        Click,

        /// <summary>Submits a form element.</summary>
        Submit,

        /// <summary>Selects an option from a select element by visible text.</summary>
        SelectOptionByText,

        /// <summary>Selects an option from a select element by index.</summary>
        SelectOptionByIndex,

        /// <summary>Selects an option from a select element by value.</summary>
        SelectOptionByValue,

        /// <summary>Gets the text inside an element.</summary>
        GetText,

        /// <summary>Gets a given attribute of an element.</summary>
        GetAttribute,

        /// <summary>Checks if the element is currently displayed on the page.</summary>
        IsDisplayed,

        /// <summary>Checks if the element is enabled on the page.</summary>
        IsEnabled,

        /// <summary>Checks if the element is selected.</summary>
        IsSelected,

        /// <summary>Retrieves the X coordinate of the top-left corner of the element.</summary>
        LocationX,

        /// <summary>Retrieves the Y coordinate of the top-left corner of the element.</summary>
        LocationY,

        /// <summary>Retrieves the width of the element.</summary>
        SizeX,

        /// <summary>Retrieves the height of the element.</summary>
        SizeY,

        /// <summary>Takes a screenshot of the element and saves it as an image.</summary>
        Screenshot,

        /// <summary>Takes a screenshot of the element and saves it as a base64-encoded string.</summary>
        ScreenshotBase64,

        /// <summary>Switches to the iframe element.</summary>
        SwitchToFrame,

        /// <summary>Waits until the element appears in the DOM (up to a specified timeout).</summary>
        WaitForElement
    }

    /// <summary>
    /// A block that can perform an action on an element inside an HTML page.
    /// </summary>
    public class SBlockElementAction : BlockBase
    {
        private ElementLocator locator = ElementLocator.Id;
        /// <summary>The element locator.</summary>
        public ElementLocator Locator { get { return locator; } set { locator = value; OnPropertyChanged(); } }

        private string elementString = "";
        /// <summary>The value of the locator.</summary>
        public string ElementString { get { return elementString; } set { elementString = value; OnPropertyChanged(); } }

        private int elementIndex = 0;
        /// <summary>The index of the element in case the locator matches more than one.</summary>
        public int ElementIndex { get { return elementIndex; } set { elementIndex = value; OnPropertyChanged(); } }

        private ElementAction action = ElementAction.Clear;
        /// <summary>The action to be performed on the element.</summary>
        public ElementAction Action { get { return action; } set { action = value; OnPropertyChanged(); } }

        private string input = "";
        /// <summary>The input data.</summary>
        public string Input { get { return input; } set { input = value; OnPropertyChanged(); } }

        private string outputVariable = "";
        /// <summary>The name of the output variable.</summary>
        public string OutputVariable { get { return outputVariable; } set { outputVariable = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private bool recursive = false;
        /// <summary>Whether the GetText and GetAttribute actions should be executed on all the elements matched by the locator and return a list of values.</summary>
        public bool Recursive { get { return recursive; } set { recursive = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates an element action block.
        /// </summary>
        public SBlockElementAction()
        {
            Label = "ELEMENT ACTION";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax:
             * ELEMENTACTION ID "id" 0 RECURSIVE? ACTION ["INPUT"] [-> VAR/CAP "OUTPUT"]
             * */

            Locator = (ElementLocator)LineParser.ParseEnum(ref input, "LOCATOR", typeof(ElementLocator));
            ElementString = LineParser.ParseLiteral(ref input, "STRING");
            if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);
            else if (LineParser.Lookahead(ref input) == TokenType.Integer)
                ElementIndex = LineParser.ParseInt(ref input, "INDEX");
            Action = (ElementAction)LineParser.ParseEnum(ref input, "ACTION", typeof(ElementAction));

            if (input == string.Empty) return this;
            if (LineParser.Lookahead(ref input) == TokenType.Literal)
                Input = LineParser.ParseLiteral(ref input, "INPUT");

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == string.Empty)
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { OutputVariable = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("ELEMENTACTION")
                .Token(Locator)
                .Literal(ElementString);

            if (Recursive) writer.Boolean(Recursive, "Recursive");
            else if (ElementIndex != 0) writer.Integer(ElementIndex, "ElementIndex");

            writer
                .Indent()
                .Token(Action)
                .Literal(Input, "Input");

            if (!writer.CheckDefault(OutputVariable, "OutputVariable"))
            {
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(OutputVariable);
            }

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            if (data.Driver == null)
            {
                data.Log(new LogEntry("Open a browser first!", Colors.White));
                throw new Exception("Browser not open");
            }

            // Find the element
            IWebElement element = null;
            ReadOnlyCollection<IWebElement> elements = null;
            try
            {
                if(action != ElementAction.WaitForElement)
                {
                    elements = FindElements(data);
                    element = elements[elementIndex];
                }
            }
            catch { data.Log(new LogEntry("Cannot find element on the page", Colors.White)); }

            List<string> outputs = new List<string>();
            try
            {
                switch (action)
                {
                    case ElementAction.Clear:
                        element.Clear();
                        break;

                    case ElementAction.SendKeys:
                        element.SendKeys(ReplaceValues(input, data));
                        break;

                    case ElementAction.Click:
                        element.Click();
                        UpdateSeleniumData(data);
                        break;

                    case ElementAction.Submit:
                        element.Submit();
                        UpdateSeleniumData(data);
                        break;

                    case ElementAction.SelectOptionByText:
                        new SelectElement(element).SelectByText(ReplaceValues(input, data));
                        break;

                    case ElementAction.SelectOptionByIndex:
                        new SelectElement(element).SelectByIndex(int.Parse(ReplaceValues(input, data)));
                        break;

                    case ElementAction.SelectOptionByValue:
                        new SelectElement(element).SelectByValue(ReplaceValues(input, data));
                        break;

                    case ElementAction.GetText:
                        if (recursive) foreach (var elem in elements) outputs.Add(elem.Text);
                        else outputs.Add(element.Text);
                        break;

                    case ElementAction.GetAttribute:
                        if (recursive) foreach (var elem in elements) outputs.Add(elem.GetAttribute(ReplaceValues(input, data)));
                        else outputs.Add(element.GetAttribute(ReplaceValues(input, data)));
                        break;

                    case ElementAction.IsDisplayed:
                        outputs.Add(element.Displayed.ToString());
                        break;

                    case ElementAction.IsEnabled:
                        outputs.Add(element.Enabled.ToString());
                        break;

                    case ElementAction.IsSelected:
                        outputs.Add(element.Selected.ToString());
                        break;

                    case ElementAction.LocationX:
                        outputs.Add(element.Location.X.ToString());
                        break;

                    case ElementAction.LocationY:
                        outputs.Add(element.Location.Y.ToString());
                        break;

                    case ElementAction.SizeX:
                        outputs.Add(element.Size.Width.ToString());
                        break;

                    case ElementAction.SizeY:
                        outputs.Add(element.Size.Height.ToString());
                        break;

                    case ElementAction.Screenshot:
                        var image = GetElementScreenShot(data.Driver, element);
                        Files.SaveScreenshot(image, data);
                        break;

                    case ElementAction.ScreenshotBase64:
                        var image2 = GetElementScreenShot(data.Driver, element);
                        var memStream = new MemoryStream();
                        image2.Save(memStream, ImageFormat.Jpeg);
                        outputs.Add(Convert.ToBase64String(memStream.ToArray()));
                        break;

                    case ElementAction.SwitchToFrame:
                        data.Driver.SwitchTo().Frame(element);
                        break;

                    case ElementAction.WaitForElement:
                        var ms = 0; // Currently waited milliseconds
                        var max = 10000;
                        try { max = int.Parse(input) * 1000; } catch { }// Max ms to wait
                        var found = false;
                        while(ms < max)
                        {
                            try
                            {
                                elements = FindElements(data);
                                element = elements[0];
                                found = true;
                                break;
                            }
                            catch { ms += 200; Thread.Sleep(200); }
                        }
                        if (!found) { data.Log(new LogEntry("Timeout while waiting for element", Colors.White)); }
                        break;

                    case ElementAction.SendKeysHuman:
                        var toSend = ReplaceValues(input, data);
                        var rand = new Random();
                        foreach(char c in toSend) {
                            element.SendKeys(c.ToString());
                            Thread.Sleep(rand.Next(100, 300));
                        }
                        break;
                }
            }
            catch { data.Log(new LogEntry("Cannot execute action on the element", Colors.White)); }

            data.Log(new LogEntry(string.Format("Executed action {0} on the element with input {1}", action, ReplaceValues(input, data)), Colors.White));

            if (outputs.Count != 0)
            {
                InsertVariable(data, isCapture, recursive, outputs, outputVariable, "", "", false, true);
            }
        }

        /// <summary>
        /// Screenshots an element on the page.
        /// </summary>
        /// <param name="driver">The selenium driver</param>
        /// <param name="element">The element to screenshot</param>
        /// <returns>The bitmap screenshot of the element</returns>
        public static Bitmap GetElementScreenShot(IWebDriver driver, IWebElement element)
        {
            Screenshot sc = ((ITakesScreenshot)driver).GetScreenshot();
            var img = Image.FromStream(new MemoryStream(sc.AsByteArray)) as Bitmap;
            return img.Clone(new Rectangle(element.Location, element.Size), img.PixelFormat);
        }

        private ReadOnlyCollection<IWebElement> FindElements(BotData data)
        {
            switch (locator)
            {
                case ElementLocator.Id:
                    return data.Driver.FindElements(By.Id(ReplaceValues(elementString, data)));

                case ElementLocator.Class:
                    return data.Driver.FindElements(By.ClassName(ReplaceValues(elementString, data)));

                case ElementLocator.Name:
                    return data.Driver.FindElements(By.Name(ReplaceValues(elementString, data)));

                case ElementLocator.Tag:
                    return data.Driver.FindElements(By.TagName(ReplaceValues(elementString, data)));

                case ElementLocator.Selector:
                    return data.Driver.FindElements(By.CssSelector(ReplaceValues(elementString, data)));

                case ElementLocator.XPath:
                    return data.Driver.FindElements(By.XPath(ReplaceValues(elementString, data)));

                default:
                    return null;
            }
        }
    }
}
