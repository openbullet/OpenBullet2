using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RuriLib.LS
{
    /// <summary>
    /// Parses a MOUSEACTION command.
    /// </summary>
    class MouseActionParser
    {
        /// <summary>
        /// Gets the Action that needs to be executed.
        /// </summary>
        /// <param name="line">The data line to parse</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns>The Action to execute</returns>
        public static Action Parse(string line, BotData data)
        {
            // Trim the line
            var input = line.Trim();

            // Initialize the action chain
            Actions actions = null;
            try
            {
                actions = new Actions(data.Driver);
            }
            catch { throw new Exception("No Browser initialized!"); }

            // Build it
            var offsetX = 0;
            var offsetY = 0;
            var point1X = 0;
            var point1Y = 0;
            var point2X = 0;
            var point2Y = 0;
            var key = "";
            var gravity = 1;
            var wind = 1;
            var qty = 0;
            IWebElement elem1 = null;
            IWebElement elem2 = null;
            Line newLine = null;
            while (input != string.Empty)
            {
                var parsed = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();
                switch (parsed)
                {
                    case "SPAWN":
                        // Spawn a div in a certain position so you can hook to it later via id
                        if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        {
                            point1X = LineParser.ParseInt(ref input, "X");
                            point1Y = LineParser.ParseInt(ref input, "Y");
                        }
                        else if (LineParser.Lookahead(ref input) == TokenType.Literal)
                        {
                            point1X = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "X"), data));
                            point1Y = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "Y"), data));
                        }
                        SpawnDiv(data.Driver, point1X, point1Y, LineParser.ParseLiteral(ref input, "ID"));
                        break;

                    case "CLICK":
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.Click();
                        else
                            actions.Click(ParseElement(ref input, data));
                        break;

                    case "CLICKANDHOLD":
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.ClickAndHold();
                        else
                            actions.ClickAndHold(ParseElement(ref input, data));
                        break;

                    case "RIGHTCLICK":
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.ContextClick();
                        else
                            actions.ContextClick(ParseElement(ref input, data));
                        break;

                    case "DOUBLECLICK":
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.DoubleClick();
                        else
                            actions.DoubleClick(ParseElement(ref input, data));
                        break;

                    case "DRAGANDDROP":
                        elem1 = ParseElement(ref input, data);
                        LineParser.ParseToken(ref input, TokenType.Arrow, true);
                        elem2 = ParseElement(ref input, data);
                        actions.DragAndDrop(elem1, elem2);
                        break;

                    case "DRAGANDDROPWITHOFFSET":
                        if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        {
                            offsetX = LineParser.ParseInt(ref input, "OFFSET X");
                            offsetY = LineParser.ParseInt(ref input, "OFFSET Y");
                        }
                        else if (LineParser.Lookahead(ref input) == TokenType.Literal)
                        {
                            offsetX = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "OFFSET X"), data));
                            offsetY = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "OFFSET Y"), data));
                        }
                        actions.DragAndDropToOffset(ParseElement(ref input, data), offsetX, offsetY);
                        break;

                    case "KEYDOWN":
                        key = LineParser.ParseLiteral(ref input, "KEY", true, data);
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.KeyDown(key);
                        else
                            actions.KeyDown(ParseElement(ref input, data), key);
                        break;

                    case "KEYUP":
                        key = LineParser.ParseLiteral(ref input, "KEY", true, data);
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.KeyUp(key);
                        else
                            actions.KeyUp(ParseElement(ref input, data), key);
                        break;

                    case "MOVEBY":
                        if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        {
                            offsetX = LineParser.ParseInt(ref input, "OFFSET X");
                            offsetY = LineParser.ParseInt(ref input, "OFFSET Y");
                        }
                        else if (LineParser.Lookahead(ref input) == TokenType.Literal)
                        {
                            offsetX = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "OFFSET X"), data));
                            offsetY = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "OFFSET Y"), data));
                        }
                        actions.MoveByOffset(offsetX, offsetY);
                        break;

                    case "MOVETO":
                        actions.MoveToElement(ParseElement(ref input, data));
                        break;

                    case "RELEASE":
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.Release();
                        else
                            actions.Release(ParseElement(ref input, data));
                        break;

                    case "SENDKEYS":
                        key = LineParser.ParseLiteral(ref input, "KEY", true, data);
                        if (!LineParser.CheckIdentifier(ref input, "ELEMENT"))
                            actions.SendKeys(key);
                        else
                            actions.SendKeys(ParseElement(ref input, data), key);
                        break;

                    case "DRAWPOINTS":
                        offsetX = LineParser.ParseInt(ref input, "MAX WIDTH");
                        offsetY = LineParser.ParseInt(ref input, "MAX HEIGHT");
                        var amount = LineParser.ParseInt(ref input, "AMOUNT");
                        Random rand = new Random();
                        var previousx = 0;
                        var previousy = 0;
                        // Move to the first point
                        actions.MoveToElement(data.Driver.FindElementByTagName("body"), point1X, point1Y);
                        List<Point> points = new List<Point>();
                        for (int i = 0; i < amount; i++)
                        {
                            var x = rand.Next(0, offsetX);
                            var y = rand.Next(0, offsetY);
                            actions.MoveByOffset(x - previousx, y - previousy);
                            previousx = x;
                            previousy = y;
                            points.Add(new Point(x, y));
                        }
                        if (data.GlobalSettings.Selenium.DrawMouseMovement)
                            DrawRedDots(data.Driver, points.ToArray(), 5);
                        break;

                    case "DRAWLINE":
                        // DRAWLINE 10 10 -> 20 20
                        if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        {
                            point1X = LineParser.ParseInt(ref input, "X1");
                            point1Y = LineParser.ParseInt(ref input, "Y1");
                            LineParser.ParseToken(ref input, TokenType.Arrow, true);
                            point2X = LineParser.ParseInt(ref input, "X2");
                            point2Y = LineParser.ParseInt(ref input, "Y2");
                        }
                        // DRAWLINE "10" "20" -> "<MY_X>" "<MY_Y>"
                        else if (LineParser.Lookahead(ref input) == TokenType.Literal)
                        {
                            point1X = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "X1"), data));
                            point1Y = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "Y1"), data));
                            LineParser.ParseToken(ref input, TokenType.Arrow, true);
                            point2X = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "X2"), data));
                            point2Y = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "Y2"), data));
                        }
                        else
                        {
                            elem1 = ParseElement(ref input, data);
                            point1X = elem1.Location.X;
                            point1Y = elem1.Location.Y;
                            LineParser.ParseToken(ref input, TokenType.Arrow, true);
                            elem2 = ParseElement(ref input, data);
                            point2X = elem2.Location.X;
                            point2Y = elem2.Location.Y;
                        }
                        LineParser.EnsureIdentifier(ref input, ":");
                        qty = LineParser.ParseInt(ref input, "QUANTITY");
                        // Move to the first point
                        actions.MoveToElement(data.Driver.FindElementByTagName("body"), point1X, point1Y);
                        newLine = new Line(new Point(point1X, point1Y), new Point(point2X, point2Y));
                        if (data.GlobalSettings.Selenium.DrawMouseMovement)
                            DrawRedDots(data.Driver, newLine.getPoints(qty), 5);
                        foreach (var p in newLine.getOffsets(qty))
                        {
                            actions.MoveByOffset(p.X, p.Y);
                        }
                        break;

                    case "DRAWLINEHUMAN":
                        // DRAWLINEHUMAN 10 10 -> 20 20
                        if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        {
                            point1X = LineParser.ParseInt(ref input, "X1");
                            point1Y = LineParser.ParseInt(ref input, "Y1");
                            LineParser.ParseToken(ref input, TokenType.Arrow, true);
                            point2X = LineParser.ParseInt(ref input, "X2");
                            point2Y = LineParser.ParseInt(ref input, "Y2");
                        }
                        // DRAWLINEHUMAN "10" "20" -> "<MY_X>" "<MY_Y>"
                        else if (LineParser.Lookahead(ref input) == TokenType.Literal)
                        {
                            point1X = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "X1"), data));
                            point1Y = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "Y1"), data));
                            LineParser.ParseToken(ref input, TokenType.Arrow, true);
                            point2X = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "X2"), data));
                            point2Y = int.Parse(BlockBase.ReplaceValues(LineParser.ParseLiteral(ref input, "Y2"), data));
                        }
                        else
                        {
                            elem1 = ParseElement(ref input, data);
                            point1X = elem1.Location.X;
                            point1Y = elem1.Location.Y;
                            LineParser.ParseToken(ref input, TokenType.Arrow, true);
                            elem2 = ParseElement(ref input, data);
                            point2X = elem2.Location.X;
                            point2Y = elem2.Location.Y;
                        }
                        LineParser.EnsureIdentifier(ref input, ":");
                        qty = LineParser.ParseInt(ref input, "QUANTITY");
                        if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        {
                            gravity = LineParser.ParseInt(ref input, "GRAVITY");
                            wind = LineParser.ParseInt(ref input, "WIND");
                        }
                        // Move to the first point
                        actions.MoveToElement(data.Driver.FindElementByTagName("body"), point1X, point1Y);
                        newLine = new Line(new Point(point1X, point1Y), new Point(point2X, point2Y));
                        var array = newLine.HumanWindMouse(point1X, point1Y, point2X, point2Y, gravity, wind, 1);
                        var shrinked = ShrinkArray(array, qty);
                        if (data.GlobalSettings.Selenium.DrawMouseMovement)
                            DrawRedDots(data.Driver, shrinked, 5);
                        foreach (var p in GetOffsets(shrinked))
                        {
                            actions.MoveByOffset(p.X, p.Y);
                        }
                        break;
                }
            }

            return new Action(() =>
            {
                actions.Build();
                actions.Perform();
                data.Log(new LogEntry("Executed Mouse Actions", Colors.White));
            });
        }

        #region Action Creators
        /// <summary>
        /// Draws a red dot at the specified coordinates.
        /// </summary>
        /// <param name="driver">The selenium driver</param>
        /// <param name="x">The x coordinate of the dot</param>
        /// <param name="y">The y coordinate of the dot</param>
        /// <param name="thickness">The thickness in pixels of the dot</param>
        private static void DrawRedDot(RemoteWebDriver driver, int x, int y, int thickness)
        {
            try
            {
                var executor = driver as IJavaScriptExecutor;
                executor.ExecuteScript(RedDotScript(x, y, thickness));
            }
            catch { }
        }

        /// <summary>
        /// Shrinks an array of points by removing random elements to fit a target size.
        /// </summary>
        /// <param name="originArray">The original array of points</param>
        /// <param name="targetSize">The target size of the array being returned</param>
        /// <returns>An array of points with the target size</returns>
        public static Point[] ShrinkArray(Point[] originArray, int targetSize)
        {
            Random rand = new Random();
            List<Point> origin = originArray.ToList();
            while (origin.Count > targetSize)
            {
                origin.RemoveAt(rand.Next(1, origin.Count - 2));
            }
            return origin.ToArray();
        }

        /// <summary>
        /// Given an array of points, gets the offsets between one point and the next.
        /// </summary>
        /// <param name="originArray">The original array of points</param>
        /// <returns>The array of offsets</returns>
        private static Point[] GetOffsets(Point[] originArray)
        {
            List<Point> points = new List<Point>();
            var prev = originArray[0];
            foreach (var point in originArray)
            {
                points.Add(new Point(point.X - prev.X, point.Y - prev.Y));
                prev = point;
            }
            return points.ToArray();
        }

        /// <summary>
        /// Builds the js needed to draw a red dot on the screen with the specified options.
        /// </summary>
        /// <param name="x">The x coordinate of the dot</param>
        /// <param name="y">The y coordinate of the dot</param>
        /// <param name="thickness">The thickness of the dot</param>
        /// <param name="id">The id of the created div element</param>
        /// <returns>The script to execute inside the the browser</returns>
        private static string RedDotScript(int x, int y, int thickness, string id = "reddot")
        {
            return "		var div = document.createElement('div');" +
                        "		div.style.backgroundColor = 'red';" +
                        "       div.id = '" + id + "';" +
                        "		div.style.position = 'absolute';" +
                        "		div.style.left = '" + x + "px';" +
                        "       div.style.top = '" + y + "px';" +
                        "	    div.style.height = '" + thickness + "px';" +
                        "		div.style.width = '" + thickness + "px';" +
                        "		document.getElementsByTagName('body')[0].appendChild(div);";
        }

        /// <summary>
        /// Spawns a 1x1 div element at the specified coordinates.
        /// </summary>
        /// /// <param name="driver">The selenium driver</param>
        /// <param name="x">The x coordinate of the div</param>
        /// <param name="y">The y coordinate of the div</param>
        /// /// <param name="id">The id of the div element</param>
        private static void SpawnDiv(RemoteWebDriver driver, int x, int y, string id)
        {
            try
            {
                var executor = driver as IJavaScriptExecutor;
                executor.ExecuteScript(RedDotScript(x, y, 1, id));
            }
            catch { }
        }

        /// <summary>
        /// Draws an array of red dots on the screen.
        /// </summary>
        /// <param name="driver">The selenium driver</param>
        /// <param name="points">The array of points to draw</param>
        /// <param name="thickness">The thickness of the points</param>
        public static void DrawRedDots(RemoteWebDriver driver, Point[] points, int thickness)
        {
            var script = "";
            for (int i = 0; i < points.Count(); i++)
                script += RedDotScript(points[i].X, points[i].Y, thickness);

            try
            {
                var executor = driver as IJavaScriptExecutor;
                executor.ExecuteScript(script);
            }
            catch { }
        }

        /// <summary>
        /// Parses an html element from LoliScript code.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns>The parsed IWebElement</returns>
        public static IWebElement ParseElement(ref string input, BotData data)
        {
            LineParser.EnsureIdentifier(ref input, "ELEMENT");
            var locator = (ElementLocator)LineParser.ParseEnum(ref input, "Element Locator", typeof(ElementLocator));
            var elemstring = LineParser.ParseLiteral(ref input, "Element Identifier");
            var index = 0;
            if (LineParser.Lookahead(ref input) == TokenType.Integer)
                index = LineParser.ParseInt(ref input, "Element Index");
            switch (locator)
            {
                case ElementLocator.Id:
                    return data.Driver.FindElementsById(elemstring)[index];

                case ElementLocator.Class:
                    return data.Driver.FindElementsByClassName(elemstring)[index];

                case ElementLocator.Name:
                    return data.Driver.FindElementsByName(elemstring)[index];

                case ElementLocator.Selector:
                    return data.Driver.FindElementsByCssSelector(elemstring)[index];

                case ElementLocator.Tag:
                    return data.Driver.FindElementsByTagName(elemstring)[index];

                case ElementLocator.XPath:
                    return data.Driver.FindElementsByXPath(elemstring)[index];

                default:
                    throw new Exception("Element not found on the page");
            }
        }
        #endregion
    }

    #region Line Class
    /// <summary>
    /// <para>Represents a line drawn between two points.</para>
    /// <para>This class provides useful methods to simulate mouse movement across the screen.</para>
    /// </summary>
    public class Line
    {
        private Point p1, p2;
        private Random rand;

        /// <summary>
        /// Creates a line between two points.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Line(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            rand = new Random();
        }

        /// <summary>
        /// Gets a given number of equally distant points in the line.
        /// </summary>
        /// <param name="quantity">The amount of points to generate</param>
        /// <returns>The array of the generated points</returns>
        public Point[] getPoints(int quantity)
        {
            var points = new Point[quantity];
            int ydiff = p2.Y - p1.Y, xdiff = p2.X - p1.X;
            double slope = (double)(p2.Y - p1.Y) / (p2.X - p1.X);
            double x, y;

            --quantity;

            for (double i = 0; i < quantity; i++)
            {
                y = slope == 0 ? 0 : ydiff * (i / quantity);
                x = slope == 0 ? xdiff * (i / quantity) : y / slope;
                points[(int)i] = new Point((int)Math.Round(x) + p1.X, (int)Math.Round(y) + p1.Y);
            }

            points[quantity] = p2;
            return points;
        }

        /// <summary>
        /// Gets a given number of offsets from equally distant points in the line.
        /// </summary>
        /// <param name="quantity">The amount of offsets to generate</param>
        /// <returns>The array of the generated offsets</returns>
        public Point[] getOffsets(int quantity)
        {
            var points = new Point[quantity];
            int ydiff = p2.Y - p1.Y, xdiff = p2.X - p1.X;
            double slope = (double)(p2.Y - p1.Y) / (p2.X - p1.X);
            double x, y;

            --quantity;

            for (double i = 0; i < quantity; i++)
            {
                y = slope == 0 ? 0 : ydiff * (i / quantity);
                x = slope == 0 ? xdiff * (i / quantity) : y / slope;
                points[(int)i] = new Point((int)Math.Round(x), (int)Math.Round(y));
            }

            points[quantity] = p2;
            return points;
        }

        /// <summary>
        /// The carthesian distance between two points.
        /// </summary>
        /// <param name="x1">The x coordinate of the first point</param>
        /// <param name="y1">The y coordinate of the first point</param>
        /// <param name="x2">The x coordinate of the second point</param>
        /// <param name="y2">The y coordinate of the second point</param>
        /// <returns>The distance between the points</returns>
        private static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Gets the hypotenuse of a triangle given its legs.
        /// </summary>
        /// <param name="x">The first leg of the triangle</param>
        /// <param name="y">The second leg of the triangle</param>
        /// <returns>The value of the hypotenuse</returns>
        private static double Hypot(double x, double y)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        /// <summary>
        /// Gets the points drawn by a mouse when a human moves it across the screen.
        /// </summary>
        /// <param name="xs">The x coordinate of the starting point</param>
        /// <param name="ys">The y coordinate of the starting point</param>
        /// <param name="xe">The x coordinate of the ending point</param>
        /// <param name="ye">The y coordinate of the ending point</param>
        /// <param name="gravity">The gravity the movement is subject to</param>
        /// <param name="wind">The non linearity of the movement</param>
        /// <param name="targetArea">The target area to reach</param>
        /// <returns>The set of points to draw in order to emulate a humanlike movement of the mouse</returns>
        public Point[] HumanWindMouse(double xs, double ys, double xe, double ye, double gravity, double wind, double targetArea)
        {
            double veloX = 0,
                veloY = 0,
                windX = 0,
                windY = 0;

            var sqrt2 = Math.Sqrt(2);
            var sqrt3 = Math.Sqrt(3);
            var sqrt5 = Math.Sqrt(5);

            var tDist = (int)Distance(Math.Round(xs), Math.Round(ys), Math.Round(xe), Math.Round(ye));
            var t = (uint)(Environment.TickCount + 10000);

            List<Point> points = new List<Point>();
            do
            {
                if (Environment.TickCount > t)
                    break;

                var dist = Hypot(xs - xe, ys - ye);
                wind = Math.Min(wind, dist);

                if (dist < 1)
                    dist = 1;

                var d = (Math.Round(Math.Round((double)tDist) * 0.3) / 7);

                if (d > 25)
                    d = 25;

                if (d < 5)
                    d = 5;

                double rCnc = rand.Next(6);

                if (rCnc == 1)
                    d = 2;

                double maxStep;

                if (d <= Math.Round(dist))
                    maxStep = d;
                else
                    maxStep = Math.Round(dist);

                if (dist >= targetArea)
                {
                    windX = windX / sqrt3 + (rand.Next((int)(Math.Round(wind) * 2 + 1)) - wind) / sqrt5;
                    windY = windY / sqrt3 + (rand.Next((int)(Math.Round(wind) * 2 + 1)) - wind) / sqrt5;
                }
                else
                {
                    windX = windX / sqrt2;
                    windY = windY / sqrt2;
                }

                veloX = veloX + windX;
                veloY = veloY + windY;
                veloX = veloX + gravity * (xe - xs) / dist;
                veloY = veloY + gravity * (ye - ys) / dist;

                if (Hypot(veloX, veloY) > maxStep)
                {
                    var randomDist = maxStep / 2.0 + rand.Next((int)(Math.Round(maxStep) / 2));
                    var veloMag = Math.Sqrt(veloX * veloX + veloY * veloY);
                    veloX = (veloX / veloMag) * randomDist;
                    veloY = (veloY / veloMag) * randomDist;
                }

                var lastX = (int)Math.Round(xs);
                var lastY = (int)Math.Round(ys);
                xs = xs + veloX;
                ys = ys + veloY;

                // Set cursor on next step
                if (lastX != Math.Round(xs) || (lastY != Math.Round(ys)))
                    points.Add(new Point((int)Math.Round(xs), (int)Math.Round(ys)));

            } while (!(Hypot(xs - xe, ys - ye) < 1));

            // Set cursor on end
            if (Math.Round(xe) != Math.Round(xs) || (Math.Round(ye) != Math.Round(ys)))
                points.Add(new Point((int)Math.Round(xe), (int)Math.Round(ye)));

            return points.ToArray();
        }
    }
    #endregion
}
