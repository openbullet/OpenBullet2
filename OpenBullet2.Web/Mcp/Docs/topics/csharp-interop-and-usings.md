# Interop And Usings

In OB2, LoliCode can interoperate with:

- inline C# mixed directly into the script
- the `Script` block with `NodeJS`, `Python`, `IronPython`, or `Jint`

Choose the lightest tool that solves the problem cleanly.

## When to use which option

Prefer inline C# when:

- no suitable block exists
- a small helper method is cleaner than many blocks
- you need regex, LINQ, parsing, or .NET APIs directly
- you want the fewest runtime dependencies

Prefer a `Script` block interpreter when:

- you need a library or language ecosystem feature that is easier outside .NET
- the logic is already naturally expressed in Python or JavaScript
- the runtime dependency is acceptable for the target OB2 installation

Rules that still apply:

- in the main script, you can use `input`, `data`, and `globals`
- in the startup script, only `globals` exists
- interop is normal in OB2; mixing blocks, LoliCode statements, and C# is expected

## Inline C# and custom usings

If the C# uses types from namespaces that are not already imported, add them through the config's custom usings.

Example:

```csharp
string match = new Regex(@"^\d{4}").Match(input.DATA).ToString();
```

This requires:

- `System.Text.RegularExpressions` in custom usings

Practical workflow:

1. inspect or update custom usings through config settings / LoliCode tools
2. write the mixed script
3. call `convert_lolicode_to_csharp` if the emitted structure or variable scope is unclear
4. call `debug_config` to validate runtime behavior

Custom usings matter for C# compilation. They do not install packages and they do not affect Python or NodeJS imports.

## Script block interop

The standard non-C# interop surface is the `Script` block.

Typical shape:

```loli
BLOCK:Script
INTERPRETER:Python
INPUT x,y
BEGIN SCRIPT
result = x + y
END SCRIPT
OUTPUT Int @result
ENDBLOCK
```

Agent implications:

- `INPUT` explicitly copies selected OB2 variables into the foreign runtime
- `OUTPUT` marshals values back into OB2 variables
- keep the interface narrow instead of passing many values blindly
- if you need complex nested structures, serialize them to JSON strings yourself

## NodeJS

Use `INTERPRETER:NodeJS` when JavaScript or the Node package ecosystem is the right tool.

Important facts:

- NodeJS must be installed on the target system
- third-party packages must be installed in OB2's `Scripts` directory
- Node dependencies are resolved from `Scripts/node_modules`
- if the target installation is unknown, do not assume NodeJS or npm packages are available

Example with an external package:

```loli
BLOCK:Script
INTERPRETER:NodeJS
BEGIN SCRIPT
import axios from 'axios';
const response = await axios.get('https://api.example.com');
const responseSource = response.data;
END SCRIPT
OUTPUT String @responseSource
ENDBLOCK
```

For agent-authored configs, document required runtimes and packages in the config readme whenever they are not standard OB2 capabilities.

## Python

Use `INTERPRETER:Python` for real CPython interop. This is the preferred Python option for new configs.

Supported direct input/output types:

- `String`
- `Int`
- `Float`
- `Bool`
- `ListOfStrings`
- `DictionaryOfStrings`
- `ByteArray`

If you need richer structures, serialize to `String` yourself, usually as JSON.

### Runtime resolution

OB2 resolves Python like this:

1. if `Scripts/.venv` exists, OB2 uses that virtual environment
2. otherwise, OB2 downloads and uses a Python `3.14` redistributable automatically

Consequences:

- `.venv` is the source of truth for Python version and installed packages
- OB2 does not install third-party packages for you
- the first run without `.venv` can take longer because Python may need to be downloaded

If you need external modules, create `Scripts/.venv` and install them there.

### Concurrency and runtime behavior

The embedded CPython runtime is shared by the whole OB2 process.

That means:

- once one Python environment is initialized, you cannot switch to a different Python version or `.venv` without restarting OB2
- synchronous blocking code can become a bottleneck with many bots
- async I/O code is usually a better fit than blocking I/O

Prefer async libraries such as `httpx` or `aiohttp` for network-heavy Python scripts.

### Logging and errors

Anything written to Python stdout or stderr is captured and sent to the bot log when the script completes or fails.

If the Python script throws an exception, the error is propagated back to the bot log as well.

## Legacy interpreters

`IronPython` and `Jint` still exist, but they are legacy choices:

- `IronPython` is not CPython and uses Python 2 semantics
- `IronPython` is planned for deprecation
- `Jint` is planned for deprecation

Prefer `Python` for new Python-based configs and `NodeJS` for new JavaScript-based configs.

## Agent guidance

- Prefer built-in blocks or inline C# first when the requirement is simple and already covered by .NET APIs.
- Reach for Python or NodeJS when the language ecosystem is the real advantage, not just out of habit.
- Do not assume runtimes, virtual environments, or third-party packages exist on the target installation unless the environment is known.
- Keep `INPUT` and `OUTPUT` explicit so the interop boundary is easy to debug.
- Use `convert_lolicode_to_csharp` when mixed C# behavior or generated structure is unclear.
- Use `debug_config` to validate type marshalling, logs, exceptions, and runtime prerequisites.
