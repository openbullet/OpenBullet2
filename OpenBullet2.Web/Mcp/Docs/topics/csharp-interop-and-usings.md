# C# Interop And Usings

In OB2, LoliCode and C# are meant to coexist.

Use inline C# when:

- no suitable block exists
- a small helper method is cleaner than many blocks
- you need regex, LINQ, parsing, or .NET APIs directly
- you need logic that would be awkward in blocks alone

If the C# uses types from namespaces that are not already imported, add them through the config's custom usings.

Example need:

```csharp
string match = new Regex(@"^\d{4}").Match(input.DATA).ToString();
```

This requires:

- `System.Text.RegularExpressions` in custom usings

Practical workflow:

1. add or inspect custom usings through config settings / LoliCode tools
2. write the mixed script
3. call `convert_lolicode_to_csharp` if compilation or emitted structure is unclear
4. call `debug_config` to validate runtime behavior

Inline C# is a first-class authoring tool in OB2, not a hack.
