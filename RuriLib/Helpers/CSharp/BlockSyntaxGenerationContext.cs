using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;

namespace RuriLib.Helpers.CSharp;

/// <summary>
/// Carries the state required to generate Roslyn syntax for a block.
/// </summary>
public class BlockSyntaxGenerationContext
{
    /// <summary>
    /// The variables already declared in the generated script.
    /// </summary>
    public List<string> DefinedVariables { get; }

    /// <summary>
    /// The config settings that influence generated code.
    /// </summary>
    public ConfigSettings Settings { get; }

    /// <summary>
    /// Gets a value indicating whether step-by-step debugger instrumentation should be emitted.
    /// </summary>
    public bool StepByStep { get; }

    /// <summary>
    /// Initializes a new <see cref="BlockSyntaxGenerationContext"/>.
    /// </summary>
    /// <param name="definedVariables">The variables already declared in the generated script.</param>
    /// <param name="settings">The config settings that influence generated code.</param>
    /// <param name="stepByStep">Whether step-by-step debugger instrumentation should be emitted.</param>
    public BlockSyntaxGenerationContext(List<string> definedVariables, ConfigSettings settings, bool stepByStep = false)
    {
        DefinedVariables = definedVariables ?? throw new ArgumentNullException(nameof(definedVariables));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        StepByStep = stepByStep;
    }
}
