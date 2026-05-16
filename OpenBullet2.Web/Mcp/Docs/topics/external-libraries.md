# External Libraries

OB2 can use external C# libraries from the plugin loading system, but an agent should treat that as read-only environment state unless it has an explicit tool to manage plugins.

How external libraries become available in OB2:

- place the library `.dll` and its dependencies in `UserData/Plugins`
- restart OB2
- import the library namespace through custom usings

Notes:

- if OB2 already depends on a library, do not add another copy unless you know a duplicate is safe
- external libraries are useful when built-in blocks and plain .NET APIs are still not enough

Agent rules:

- do not assume you can add external libraries yourself
- only rely on an external library if you already know it is present in the target OB2 installation
- if the needed library is not already available, prefer built-in blocks or plain C# with existing framework types
- if an unavailable external library is truly required, report the limitation instead of pretending it can be installed

If a config depends on a non-standard library, document that dependency in the config readme.
