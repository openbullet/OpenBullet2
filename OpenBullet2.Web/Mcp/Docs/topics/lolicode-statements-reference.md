# LoliCode Statements Reference

This topic is the syntax reference for the supported native LoliCode statements. Use it when you need exact statement syntax instead of the higher-level overview.

## LOG

Purpose: prints text to the debugger log.

Syntax:

```loli
LOG "hello"
LOG $"Hello <input.USERNAME>"
```

## CLOG

Purpose: prints colored text to the debugger log.

Syntax:

```loli
CLOG YellowGreen "hello"
CLOG SkyBlue $"Token: <token>"
```

Notes:

- color names should use PascalCase
- this is mainly useful for human-readable debug output

## JUMP

Purpose: jumps to a label.

Syntax:

```loli
#HERE
LOG "loop"
JUMP #HERE
```

Notes:

- use carefully, since it can create endless loops

## REPEAT

Purpose: repeats a block of code a fixed number of times.

Syntax:

```loli
REPEAT 5
LOG "hello"
END
```

## FOREACH

Purpose: iterates over a list variable.

Syntax:

```loli
FOREACH elem IN list
LOG elem
END
```

Example:

```loli
BLOCK:ConstantList
  value = ["one", "two", "three"]
  => VAR @list
ENDBLOCK

FOREACH elem IN list
LOG elem
END
```

## WHILE

Purpose: runs while a condition is true.

Syntax:

```loli
WHILE INTKEY 1 LessThan 2
LOG "looping"
END
```

## IF / ELSE IF / ELSE

Purpose: conditional branching.

Syntax:

```loli
IF INTKEY 5 LessThan 1
LOG "nope"
ELSE IF INTKEY 5 LessThan 3
LOG "nope again"
ELSE
LOG "yep"
END
```

Notes:

- the whole conditional chain closes with a single `END`

## TRY / CATCH / FINALLY

Purpose: exception handling.

Basic syntax:

```loli
TRY
// code that may fail
CATCH
// fallback path
END
```

With finally:

```loli
TRY
// code that may fail
CATCH
// error handling
FINALLY
// cleanup
END
```

Notes:

- `FINALLY` is especially important with async lock management

## LOCK

Purpose: synchronous critical section for shared state.

Syntax:

```loli
LOCK globals
TRY
globals.Count++;
CATCH
globals.Count = 1;
END
END
```

Notes:

- use when multiple bots may edit the same shared data synchronously
- commonly paired with `TRY / CATCH`

## ACQUIRELOCK / RELEASELOCK

Purpose: async-safe critical section for shared state.

Syntax:

```loli
ACQUIRELOCK globals
TRY
// async operations
CATCH
throw;
FINALLY
RELEASELOCK globals
END
```

Rules:

- always pair `ACQUIRELOCK` with `RELEASELOCK`
- the safe pattern is `TRY / CATCH / FINALLY`
- put `RELEASELOCK` in `FINALLY`

## SET VAR

Purpose: sets a string variable.

Syntax:

```loli
SET VAR @myString "variable"
LOG myString
```

## SET CAP

Purpose: sets a string variable and marks it for capture.

Syntax:

```loli
SET CAP @myCapture "capture"
LOG myCapture
```

Notes:

- these statements were introduced for OB1-style consistency

## SET USEPROXY

Purpose: enables or disables use of the current proxy.

Syntax:

```loli
SET USEPROXY TRUE
SET USEPROXY FALSE
```

## SET PROXY

Purpose: explicitly sets a proxy.

Syntax without auth:

```loli
SET PROXY "127.0.0.1" 9050 SOCKS5
```

Syntax with auth:

```loli
SET PROXY "127.0.0.1" 9050 SOCKS5 "username" "password"
```

Supported proxy types:

- `HTTP`
- `SOCKS4`
- `SOCKS4A`
- `SOCKS5`

## MARK

Purpose: adds an existing variable to capture.

Syntax:

```loli
MARK @myVar
```

## UNMARK

Purpose: removes a variable from capture.

Syntax:

```loli
UNMARK @myVar
```

## TAKEONE

Purpose: takes one item from a named config resource into a string variable.

Syntax:

```loli
TAKEONE FROM "resourceName" => @myString
```

Example:

```loli
TAKEONE FROM "resourceName" => @myString
LOG myString
```

## TAKE

Purpose: takes multiple items from a named config resource into a `List<string>` variable.

Syntax:

```loli
TAKE 5 FROM "resourceName" => @myList
```

Notes:

- resources are configured in Config Settings > Data > Resources

## Practical guidance

- Prefer native statements for flow control, logging, captures, and resource access.
- Prefer blocks for standard operations with structured parameters and outputs.
- Mix statements, blocks, and C# when that makes the script simpler.
