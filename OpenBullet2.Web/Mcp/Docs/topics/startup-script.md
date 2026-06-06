# Startup Script

The startup script runs before the main script and is meant for shared initialization.

Use it for:

- fetching shared auth cookies or tokens once
- preparing shared resources
- initializing `globals`

Hard rule:

- only `globals` is available in startup
- `input` is not available there
- `data` is not available there

That means startup logic must not depend on per-bot input lines or per-bot response state.

Typical pattern:

1. do shared setup in startup
2. save results to `globals`
3. read those values from the main script

You can use both regular LoliCode and inline C# in startup, just like in the main script.

If a piece of logic needs `input.*` or `data.*`, it belongs in the main script, not in startup.
