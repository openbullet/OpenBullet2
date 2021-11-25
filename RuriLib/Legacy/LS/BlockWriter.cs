using System;
using System.IO;

namespace RuriLib.Legacy.LS
{
    /// <summary>
    /// Provides useful functions for writing a block as a piece of LoliScript code.
    /// </summary>
    public class BlockWriter : StringWriter
    {
        private bool Indented { get; set; }
        private Type Type { get; set; }
        private object Block { get; set; }
        private bool Disabled { get; set; }

        /// <summary>
        /// Creates a new BlockWriter for the given block type and with the given settings.
        /// </summary>
        /// <param name="blockType">The Type of the block to write</param>
        /// <param name="indented">Whether the code can be indented on multiple lines for readability</param>
        /// <param name="disabled">Whether the block to write is disabled</param>
        public BlockWriter(Type blockType, bool indented = true, bool disabled = false)
        {
            Type = blockType;
            Block = Activator.CreateInstance(blockType);
            Indented = indented;
            Disabled = disabled;
            if (Disabled) Write("!");
        }

        /// <summary>
        /// Writes any type of variable as a token (by calling its default ToString method) and a space.
        /// </summary>
        /// <param name="token">The variable to write</param>
        /// <param name="property">The name of the property of the block. If the value is the default one, it will not be written. Do not set this parameter to always write the variable.</param>
        /// <returns>The BlockWriter itself</returns>
        public BlockWriter Token(dynamic token, string property = "")
        {
            if (property != string.Empty && CheckDefault(token, property)) return this;
            Write($"{token.ToString()} ");
            return this;
        }

        /// <summary>
        /// Writes an integer value and a space.
        /// </summary>
        /// <param name="integer">The integer value to write</param>
        /// <param name="property">The name of the property of the block. If the value is the default one, it will not be written. Do not set this parameter to always write the variable.</param>
        /// <returns>The BlockWriter itself</returns>
        public BlockWriter Integer(int integer, string property = "")
        {
            if (property != string.Empty && CheckDefault(integer, property)) return this;
            Write($"{integer} ");
            return this;
        }

        /// <summary>
        /// Writes a literal value (with escaped double-quotes) wrapped by double-quotes and a space.
        /// </summary>
        /// <param name="literal">The literal value to write</param>
        /// <param name="property">The name of the property of the block. If the value is the default one, it will not be written. Do not set this parameter to always write the variable.</param>
        /// <returns>The BlockWriter itself</returns>
        public BlockWriter Literal(string literal, string property = "")
        {
            if (property != string.Empty && CheckDefault(literal, property)) return this;
            Write($"\"{literal.Replace("\\", "\\\\").Replace("\"", "\\\"")}\" ");
            return this;
        }

        /// <summary>
        /// Writes an Arrow (->) and a space.
        /// </summary>
        /// <returns>The BlockWriter itself</returns>
        public BlockWriter Arrow()
        {
            Write("-> ");
            return this;
        }

        /// <summary>
        /// Writes a block label as a # sign, the label name and a space.
        /// </summary>
        /// <param name="label">The label of the block</param>
        /// <returns>The BlockWriter itself</returns>
        public BlockWriter Label(string label)
        {
            if (CheckDefault(label, "Label")) return this;
            Write($"#{label.Replace(" ", "_")} ");
            return this;
        }

        /// <summary>
        /// Writes a boolean with the syntax Name=Value and a space.
        /// </summary>
        /// <param name="boolean">The boolean value to write</param>
        /// <param name="property">The name of the property</param>
        /// <returns></returns>
        public BlockWriter Boolean(bool boolean, string property)
        {
            if (property != string.Empty && CheckDefault(boolean, property)) return this;
            Write($"{property}={boolean.ToString().ToUpper()} ");
            return this;
        }

        /// <summary>
        /// Writes a linebreak and a given number of spaces on the next line.
        /// </summary>
        /// <param name="spacing">The amount of spacing to perform. A spacing of value 1 means two blank space characters, 2 means 4 etc.</param>
        /// <returns></returns>
        public BlockWriter Indent(int spacing = 1)
        {
            if (Indented)
            {
                WriteLine();
                if (Disabled) Write("!");
                for (var i = 0; i < spacing; i++)
                    Write("  ");
            }
            return this;
        }

        /// <summary>
        /// Writes a linebreak.
        /// </summary>
        /// <returns></returns>
        public BlockWriter Return()
        {
            WriteLine();
            return this;
        }

        /// <summary>
        /// Checks if a property has the default value.
        /// </summary>
        /// <param name="value">The value that needs to be checked</param>
        /// <param name="property">The name of the property that contains that value</param>
        /// <returns>Whether the property has the default value or not</returns>
        public bool CheckDefault(object value, string property)
        {
            var prop = Type.GetProperty(property);
            var val = prop.GetValue(Block);
            return value.Equals(val);
        }
    }
}
