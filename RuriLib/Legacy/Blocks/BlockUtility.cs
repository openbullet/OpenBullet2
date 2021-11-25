using RuriLib.Extensions;
using RuriLib.Functions.Files;
using RuriLib.Legacy.Functions.Conditions;
using RuriLib.Legacy.Functions.Conversions;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// The available utility groups.
    /// </summary>
    public enum UtilityGroup
    {
        /// <summary>The group of actions performed on list variables.</summary>
        List,

        /// <summary>The group of actions performed on single variables.</summary>
        Variable,

        /// <summary>The group of action to convert an encoded string.</summary>
        Conversion,

        /// <summary>The group of actions that interact with files.</summary>
        File,

        /// <summary>The group of actions that interact with folders.</summary>
        Folder
    }

    /// <summary>
    /// Actions executed on single variables.
    /// </summary>
    public enum VarAction
    {
        /// <summary>Splits a variable into a list given a separator.</summary>
        Split
    }

    /// <summary>
    /// Actions executed on list variables.
    /// </summary>
    public enum ListAction
    {
        /// <summary>Creates an empty list variable.</summary>
        Create,

        /// <summary>Calculates the length of a list.</summary>
        Length,

        /// <summary>Joins a list into a single string, separating the elements with a separator.</summary>
        Join,

        /// <summary>Sorts a list alphabetically, in ascending or descending order.</summary>
        Sort,

        /// <summary>Concatenates two lists into a longer list variable.</summary>
        Concat,

        /// <summary>Zips two lists into a new list variable where the elements are joined two by two.</summary>
        Zip,

        /// <summary>Maps two lists into a dictionary variable.</summary>
        Map,

        /// <summary>Adds an element to a list variable.</summary>
        Add,

        /// <summary>Removes an element from a list variable given its index.</summary>
        Remove,

        /// <summary>Removes one ore more elements from a list variable given their value.</summary>
        RemoveValues,

        /// <summary>Removes duplicate elements from a list variable, keeping only the first one.</summary>
        RemoveDuplicates,

        /// <summary>Picks a random element from a list variable.</summary>
        Random,

        /// <summary>Randomizes the order of elements in a list.</summary>
        Shuffle
    }

    /// <summary>
    /// Actions that can be performed on files.
    /// </summary>
    public enum FileAction
    {
        /// <summary>Checks if a file exists.</summary>
        Exists,

        /// <summary>Reads a file to a single variable.</summary>
        Read,

        /// <summary>Reads a file to a list variable.</summary>
        ReadLines,

        /// <summary>Writes a single variable to a file.</summary>
        Write,

        /// <summary>Writes a list variable to a file.</summary>
        WriteLines,

        /// <summary>Appends a single variable to a file.</summary>
        Append,

        /// <summary>Appends a list variable to a file.</summary>
        AppendLines,

        /// <summary>Copies a file to a new file.</summary>
        Copy,

        /// <summary>Moves a file to a different location.</summary>
        Move,

        /// <summary>Deletes a file in the OB folder.</summary>
        Delete
    }

    /// <summary>
    /// Actions that can be performed on folders.
    /// </summary>
    public enum FolderAction
    {
        /// <summary>Checks if a folder exists.</summary>
        Exists,

        /// <summary>Creates a folder.</summary>
        Create,

        /// <summary>Deletes a folder.</summary>
        Delete
    }

    /// <summary>
    /// A block that performs actions on variables, converts values and operates on files.
    /// </summary>
    public class BlockUtility : BlockBase
    {
        // General
        /// <summary>The utility group.</summary>
        public UtilityGroup Group { get; set; } = UtilityGroup.List;

        /// <summary>The name of the output variable.</summary>
        public string VariableName { get; set; } = "";

        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get; set; } = false;

        /// <summary>The input to process.</summary>
        public string InputString { get; set; } = "";

        // Lists
        /// <summary>The action to be performed on a list variable.</summary>
        public ListAction ListAction { get; set; } = ListAction.Join;

        /// <summary>The name of the target list variable.</summary>
        public string ListName { get; set; } = "";

        #region ListUtility Specific Variables
        /// <summary>The separator for joining a list.</summary>
        public string Separator { get; set; } = ",";

        /// <summary>Whether the sort should happen in ascending order.</summary>
        public bool Ascending { get; set; } = true;

        /// <summary>Whether a list is made of numeric values.</summary>
        public bool Numeric { get; set; } = false;

        /// <summary>The name of the second list variable.</summary>
        public string SecondListName { get; set; } = "";

        /// <summary>The list item to add.</summary>
        public string ListItem { get; set; } = "";

        /// <summary>The list index where an item can be added/removed. -1 = end, 0 = start.</summary>
        public string ListIndex { get; set; } = "-1";

        /// <summary>The comparer to use when removing or modifying one or more elements of a list.</summary>
        public Comparer ListElementComparer { get; set; } = Comparer.EqualTo;

        /// <summary>The string that elements in a list should be compared to.</summary>
        public string ListComparisonTerm { get; set; } = "";
        #endregion

        // Variables
        /// <summary>The action to be performed on a single variable.</summary>
        public VarAction VarAction { get; set; } = VarAction.Split;

        /// <summary>The name of the single variable.</summary>
        public string VarName { get; set; } = "";

        #region Variables Specific
        /// <summary>The separator to split a string into a list.</summary>
        public string SplitSeparator { get; set; } = "";
        #endregion

        // Conversion
        /// <summary>The encoding to convert from.</summary>
        public Encoding ConversionFrom { get; set; } = Encoding.HEX;

        /// <summary>The encoding to convert to.</summary>
        public Encoding ConversionTo { get; set; } = Encoding.BASE64;

        // Files
        /// <summary>The path to the file to interact with.</summary>
        public string FilePath { get; set; } = "text.txt";

        /// <summary>The action to be performed on the file.</summary>
        public FileAction FileAction { get; set; } = FileAction.Read;

        // Folders
        /// <summary>The path to the folder to interact with.</summary>
        public string FolderPath { get; set; } = "TestFolder";

        /// <summary>The action to be performed on the folder.</summary>
        public FolderAction FolderAction { get; set; } = FolderAction.Create;

        /// <summary>
        /// Creates a Utility block.
        /// </summary>
        public BlockUtility()
        {
            Label = "UTILITY";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            // Parse the function
            Group = (UtilityGroup)LineParser.ParseEnum(ref input, "Group", typeof(UtilityGroup));

            // Parse specific function parameters
            switch (Group)
            {
                case UtilityGroup.List:

                    ListName = LineParser.ParseLiteral(ref input, "List Name");
                    ListAction = (ListAction)LineParser.ParseEnum(ref input, "List Action", typeof(ListAction));

                    switch (ListAction)
                    {
                        case ListAction.Join:
                            Separator = LineParser.ParseLiteral(ref input, "Separator");
                            break;

                        case ListAction.Sort:
                            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                                LineParser.SetBool(ref input, this);
                            break;

                        case ListAction.Concat:
                        case ListAction.Zip:
                        case ListAction.Map:
                            SecondListName = LineParser.ParseLiteral(ref input, "Second List Name");
                            break;

                        case ListAction.Add:
                            ListItem = LineParser.ParseLiteral(ref input, "Item");
                            ListIndex = LineParser.ParseLiteral(ref input, "Index");
                            break;

                        case ListAction.Remove:
                            ListIndex = LineParser.ParseLiteral(ref input, "Index");
                            break;

                        case ListAction.RemoveValues:
                            ListElementComparer = LineParser.ParseEnum(ref input, "Comparer", typeof(Comparer));
                            ListComparisonTerm = LineParser.ParseLiteral(ref input, "Comparison Term");
                            break;
                    }
                    break;

                case UtilityGroup.Variable:
                    VarName = LineParser.ParseLiteral(ref input, "Var Name");
                    VarAction = (VarAction)LineParser.ParseEnum(ref input, "Var Action", typeof(VarAction));

                    switch (VarAction)
                    {
                        case VarAction.Split:
                            SplitSeparator = LineParser.ParseLiteral(ref input, "SplitSeparator");
                            break;
                    }
                    break;

                case UtilityGroup.Conversion:
                    ConversionFrom = (Encoding)LineParser.ParseEnum(ref input, "Conversion From", typeof(Encoding));
                    ConversionTo = (Encoding)LineParser.ParseEnum(ref input, "Conversion To", typeof(Encoding));
                    InputString = LineParser.ParseLiteral(ref input, "Input");
                    break;

                case UtilityGroup.File:
                    FilePath = LineParser.ParseLiteral(ref input, "File Name");
                    FileAction = (FileAction)LineParser.ParseEnum(ref input, "File Action", typeof(FileAction));

                    switch (FileAction)
                    {
                        case FileAction.Write:
                        case FileAction.WriteLines:
                        case FileAction.Append:
                        case FileAction.AppendLines:
                        case FileAction.Copy:
                        case FileAction.Move:
                            InputString = LineParser.ParseLiteral(ref input, "Input String");
                            break;
                    }
                    break;

                case UtilityGroup.Folder:
                    FolderPath = LineParser.ParseLiteral(ref input, "Folder Name");
                    FolderAction = (FolderAction)LineParser.ParseEnum(ref input, "Folder Action", typeof(FolderAction));
                    break;
            }

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
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("UTILITY")
                .Token(Group);

            switch (Group)
            {
                case UtilityGroup.List:
                    writer
                        .Literal(ListName)
                        .Token(ListAction);

                    switch (ListAction)
                    {
                        case ListAction.Join:
                            writer
                                .Literal(Separator);
                            break;

                        case ListAction.Sort:
                            writer
                                .Boolean(Ascending, "Ascending")
                                .Boolean(Numeric, "Numeric");
                            break;

                        case ListAction.Concat:
                        case ListAction.Zip:
                        case ListAction.Map:
                            writer
                                .Literal(SecondListName);
                            break;

                        case ListAction.Add:
                            writer
                                .Literal(ListItem)
                                .Literal(ListIndex);
                            break;

                        case ListAction.Remove:
                            writer
                                .Literal(ListIndex);
                            break;

                        case ListAction.RemoveValues:
                            writer
                                .Token(ListElementComparer)
                                .Literal(ListComparisonTerm);
                            break;
                    }
                    break;

                case UtilityGroup.Variable:
                    writer
                        .Literal(VarName)
                        .Token(VarAction);

                    switch (VarAction)
                    {
                        case VarAction.Split:
                            writer
                                .Literal(SplitSeparator);
                            break;
                    }
                    break;

                case UtilityGroup.Conversion:
                    writer
                        .Token(ConversionFrom)
                        .Token(ConversionTo)
                        .Literal(InputString);
                    break;

                case UtilityGroup.File:
                    writer
                        .Literal(FilePath)
                        .Token(FileAction);

                    switch (FileAction)
                    {
                        case FileAction.Write:
                        case FileAction.WriteLines:
                        case FileAction.Append:
                        case FileAction.AppendLines:
                        case FileAction.Copy:
                        case FileAction.Move:
                            writer
                                .Literal(InputString);
                            break;
                    }
                    break;

                case UtilityGroup.Folder:
                    writer
                        .Literal(FolderPath)
                        .Token(FolderAction);
                    break;
            }

            if (!writer.CheckDefault(VariableName, "VariableName"))
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName);

            return writer.ToString();
        }

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var replacedInput = ReplaceValues(InputString, ls);
            var variablesList = GetVariables(data);
            Variable variableToAdd = null;
            var logColor = IsCapture ? LogColors.Tomato : LogColors.Yellow;

            switch (Group)
            {
                case UtilityGroup.List:
                    var list = variablesList.Get<ListOfStringsVariable>(ListName)?.AsListOfStrings();
                    var list2 = variablesList.Get<ListOfStringsVariable>(SecondListName)?.AsListOfStrings();
                    var item = ReplaceValues(ListItem, ls);
                    var index = int.Parse(ReplaceValues(ListIndex, ls));

                    switch (ListAction)
                    {
                        case ListAction.Create:
                            variableToAdd = new ListOfStringsVariable(new List<string>());
                            break;

                        case ListAction.Length:
                            variableToAdd = new StringVariable(list.Count.ToString());
                            break;

                        case ListAction.Join:
                            variableToAdd = new StringVariable(string.Join(Separator, list));
                            break;

                        case ListAction.Sort:
                            var sorted = list.Select(e => e).ToList(); // Clone the list so we don't edit the original one

                            if (Numeric)
                            {
                                var nums = sorted.Select(e => double.Parse(e, CultureInfo.InvariantCulture)).ToList();
                                nums.Sort();
                                sorted = nums.Select(e => e.ToString()).ToList();
                            }
                            else
                            {
                                sorted.Sort();
                            }

                            if (!Ascending)
                            {
                                sorted.Reverse();
                            }

                            variableToAdd = new ListOfStringsVariable(sorted);
                            break;

                        case ListAction.Concat:
                            variableToAdd = new ListOfStringsVariable(list.Concat(list2).ToList());
                            break;

                        case ListAction.Zip:
                            variableToAdd = new ListOfStringsVariable(list.Zip(list2, (a, b) => a + b).ToList());
                            break;

                        case ListAction.Map:
                            variableToAdd = new DictionaryOfStringsVariable(list.Zip(list2, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v));
                            break;

                        case ListAction.Add:
                            // Handle negative indices
                            index = index switch
                            {
                                0 => 0,
                                < 0 => index + list.Count,
                                _ => index
                            };
                            list.Insert(index, item);
                            break;

                        case ListAction.Remove:
                            // Handle negative indices
                            index = index switch
                            {
                                0 => 0,
                                < 0 => index + list.Count,
                                _ => index
                            };
                            list.RemoveAt(index);
                            break;

                        case ListAction.RemoveValues:
                            variableToAdd = new ListOfStringsVariable(list.Where(l => !Condition.Verify(new KeycheckCondition
                            {
                                Left = ReplaceValues(l, ls),
                                Comparer = ListElementComparer,
                                Right = ListComparisonTerm
                            })).ToList());
                            break;

                        case ListAction.RemoveDuplicates:
                            variableToAdd = new ListOfStringsVariable(list.Distinct().ToList());
                            break;

                        case ListAction.Random:
                            variableToAdd = new StringVariable(list[data.Random.Next(list.Count)]);
                            break;

                        case ListAction.Shuffle:
                            // This makes a copy of the original list
                            var listCopy = new List<string>(list);
                            listCopy.Shuffle(data.Random);
                            variableToAdd = new ListOfStringsVariable(listCopy);
                            break;

                        default:
                            break;
                    }
                    data.Logger.Log($"Executed action {ListAction} on list {ListName}", logColor);
                    break;

                case UtilityGroup.Variable:
                    string single = variablesList.Get<StringVariable>(VarName).AsString();
                    switch (VarAction)
                    {
                        case VarAction.Split:
                            variableToAdd = new ListOfStringsVariable(single.Split(new string[]
                            { ReplaceValues(SplitSeparator, ls) }, StringSplitOptions.None).ToList());
                            break;
                    }
                    data.Logger.Log($"Executed action {VarAction} on variable {VarName}", logColor);
                    break;

                case UtilityGroup.Conversion:
                    var conversionInputBytes = replacedInput.ConvertFrom(ConversionFrom);
                    var conversionResult = conversionInputBytes.ConvertTo(ConversionTo);
                    variableToAdd = new StringVariable(conversionResult);
                    data.Logger.Log($"Executed conversion {ConversionFrom} to {ConversionTo} on input {replacedInput} with outcome {conversionResult}", logColor);
                    break;

                case UtilityGroup.File:
                    var file = ReplaceValues(FilePath, ls);
                    FileUtils.ThrowIfNotInCWD(file);

                    switch (FileAction)
                    {
                        case FileAction.Exists:
                            variableToAdd = new StringVariable(File.Exists(file).ToString());
                            break;

                        case FileAction.Read:
                            lock (FileLocker.GetHandle(file))
                                variableToAdd = new StringVariable(File.ReadAllText(file));
                            break;

                        case FileAction.ReadLines:
                            lock (FileLocker.GetHandle(file))
                                variableToAdd = new ListOfStringsVariable(File.ReadAllLines(file).ToList());
                            break;

                        case FileAction.Write:
                            FileUtils.CreatePath(file);
                            lock (FileLocker.GetHandle(file))
                                File.WriteAllText(file, replacedInput.Unescape());
                            break;

                        case FileAction.WriteLines:
                            FileUtils.CreatePath(file);
                            lock (FileLocker.GetHandle(file))
                                File.WriteAllLines(file, ReplaceValuesRecursive(InputString, ls).Select(i => i.Unescape()));
                            break;

                        case FileAction.Append:
                            FileUtils.CreatePath(file);
                            lock (FileLocker.GetHandle(file))
                                File.AppendAllText(file, replacedInput.Unescape());
                            break;

                        case FileAction.AppendLines:
                            FileUtils.CreatePath(file);
                            lock (FileLocker.GetHandle(file))
                                File.AppendAllLines(file, ReplaceValuesRecursive(InputString, ls).Select(i => i.Unescape()));
                            break;

                        case FileAction.Copy:
                            var fileCopyLocation = ReplaceValues(InputString, ls);
                            FileUtils.ThrowIfNotInCWD(fileCopyLocation);
                            FileUtils.CreatePath(fileCopyLocation);
                            lock (FileLocker.GetHandle(file))
                                lock (FileLocker.GetHandle(fileCopyLocation))
                                    File.Copy(file, fileCopyLocation);
                            break;

                        case FileAction.Move:
                            var fileMoveLocation = ReplaceValues(InputString, ls);
                            FileUtils.ThrowIfNotInCWD(fileMoveLocation);
                            FileUtils.CreatePath(fileMoveLocation);
                            lock (FileLocker.GetHandle(file))
                                lock (FileLocker.GetHandle(fileMoveLocation))
                                    File.Move(file, fileMoveLocation);
                            break;

                        case FileAction.Delete:
                            // No deletion if the file is in use (DB/OpenBullet.db cannot be deleted but instead DB/OpenBullet-BackupCopy.db)
                            // If another process is just reading the file it will be deleted
                            lock (FileLocker.GetHandle(file))
                                File.Delete(file);
                            break;
                    }

                    data.Logger.Log($"Executed action {FileAction} on file {file}", logColor);
                    break;

                case UtilityGroup.Folder:
                    var folder = ReplaceValues(FolderPath, ls);
                    FileUtils.ThrowIfNotInCWD(folder);

                    switch (FolderAction)
                    {
                        case FolderAction.Exists:
                            variableToAdd = new StringVariable(Directory.Exists(folder).ToString());
                            break;

                        case FolderAction.Create:
                            variableToAdd = new StringVariable(Directory.CreateDirectory(folder).ToString());
                            break;

                        case FolderAction.Delete:
                            Directory.Delete(folder, true);
                            break;
                    }

                    data.Logger.Log($"Executed action {FolderAction} on folder {folder}", logColor);
                    break;

                default:
                    break;
            }

            if (variableToAdd is not null)
            {
                variableToAdd.Name = VariableName;
                variableToAdd.MarkedForCapture = IsCapture;
                variablesList.Set(variableToAdd);
            }
        }
    }
}
