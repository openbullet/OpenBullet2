namespace RuriLib.Models.Variables;

/// <summary>The supported variable types.</summary>
public enum VariableType
{
    /// <summary>A string value.</summary>
    String,

    /// <summary>An integer value.</summary>
    Int,

    /// <summary>A floating-point value.</summary>
    Float,

    /// <summary>A boolean value.</summary>
    Bool,

    /// <summary>A list of strings.</summary>
    ListOfStrings,

    /// <summary>A dictionary of strings.</summary>
    DictionaryOfStrings,

    /// <summary>A byte-array value.</summary>
    ByteArray
}
