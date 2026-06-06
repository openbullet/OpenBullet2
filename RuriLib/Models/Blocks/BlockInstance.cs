using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks;

/// <summary>
/// Base class for a configured block instance inside a config.
/// </summary>
public abstract class BlockInstance
{
    /// <summary>
    /// Gets the descriptor identifier of the block.
    /// </summary>
    public string Id { get; protected set; }
    /// <summary>
    /// Gets or sets a value indicating whether the block is disabled.
    /// </summary>
    public bool Disabled { get; set; }
    /// <summary>
    /// Gets or sets the runtime label of the block.
    /// </summary>
    public string Label { get; set; }
    /// <summary>
    /// Gets the readable default name of the block.
    /// </summary>
    public string ReadableName { get; protected set; }
    /// <summary>
    /// Gets the configured block settings.
    /// </summary>
    public Dictionary<string, BlockSetting> Settings { get; protected set; } = [];
    /// <summary>
    /// Gets the descriptor used to create this instance.
    /// </summary>
    public BlockDescriptor Descriptor { get; protected set; }

    /// <summary>
    /// Initializes a block instance from its descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor that defines the block shape.</param>
    public BlockInstance(BlockDescriptor descriptor)
    {
        Descriptor = descriptor;
        Id = descriptor.Id;
        Label = descriptor.Name;
        ReadableName = descriptor.Name;

        Settings = Descriptor.Parameters.Values.Select(p => p.ToBlockSetting())
            .ToDictionary(p => p.Name, p => p);
    }

    /// <summary>
    /// Serializes the block instance to LoliCode.
    /// </summary>
    /// <param name="printDefaultParams">Whether settings with default values should be printed.</param>
    /// <returns>The serialized LoliCode block body.</returns>
    public virtual string ToLC(bool printDefaultParams = false)
    {
        /*
         * BLOCK:BlockId
         * DISABLED
         * LABEL:My Label
         */

        using var writer = new LoliCodeWriter();

        if (Disabled)
        {
            writer.WriteLine("DISABLED");
        }

        if (Label != ReadableName)
        {
            writer.WriteLine($"LABEL:{Label}");
        }

        // Write all the settings
        foreach (var setting in Settings.Values)
        {
            if (!Descriptor.Parameters.ContainsKey(setting.Name))
            {
                throw new Exception($"This setting is not a valid input parameter: {setting.Name}");
            }

            writer.AppendSetting(setting, Descriptor.Parameters[setting.Name], 2, printDefaultParams);
        }

        return writer.ToString();
    }

    /// <summary>
    /// Reads common block metadata from LoliCode.
    /// </summary>
    /// <param name="script">The remaining block script to parse.</param>
    /// <param name="lineNumber">The current line number, updated while parsing.</param>
    public virtual void FromLC(ref string script, ref int lineNumber)
    {
        /*
         * DISABLED
         * LABEL:My Label
         * ...
         */

        using var reader = new StringReader(script);
        using var writer = new StringWriter();

        while (reader.ReadLine() is { } line)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("DISABLED"))
            {
                Disabled = true;
                lineNumber++;
            }

            else if (trimmedLine.StartsWith("LABEL:"))
            {
                var match = Regex.Match(trimmedLine, "^LABEL:(.*)$");
                Label = match.Groups[1].Value;
                lineNumber++;
            }

            else
            {
                writer.WriteLine(line);
            }
        }

        // Edit the original script that is passed down the pipeline
        script = writer.ToString();
    }

    /// <summary>
    /// Transpiles the block instance to Roslyn statements.
    /// </summary>
    /// <param name="context">The state required during syntax generation.</param>
    /// <returns>The generated Roslyn statements.</returns>
    public abstract IEnumerable<StatementSyntax> ToSyntax(BlockSyntaxGenerationContext context);

    /// <summary>
    /// Gets a fixed setting cast to the requested type.
    /// </summary>
    /// <typeparam name="T">The expected setting type.</typeparam>
    /// <param name="name">The setting name.</param>
    /// <returns>The fixed setting if present and compatible; otherwise <see langword="null"/>.</returns>
    public T? GetFixedSetting<T>(string name) where T : Setting
        => Settings[name].FixedSetting as T;
}
