# LoliCode Basics

LoliCode is the main scripting language of OpenBullet 2. It is the representation used by Stacker, but it is not limited to blocks. It compiles to C# when the config runs, and valid C# can be mixed directly into the same script.

Practical consequences:

- a script can contain blocks, LoliCode statements, and plain C# side by side
- blocks are great for standard OB2 operations
- inline C# is the escape hatch for custom logic, helper methods, or .NET APIs that no block exposes

Example shape:

```loli
int Add(int first, int second)
{
  return first + second;
}

BLOCK:RandomInteger
  minimum = 0
  maximum = 10
  => VAR @num1
ENDBLOCK

int result = Add(num1, 5);
LOG $"Result: {result}"
```

Use `convert_lolicode_to_csharp` if you need to inspect the final emitted C# that will execute.
