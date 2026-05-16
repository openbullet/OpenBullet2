# Globals Variable

`globals` is shared state across all bots of a run.

Use it for:

- startup-produced auth tokens or cookies
- shared counters
- shared resources or caches
- state that multiple bots need to read

Under the hood:

- `globals` is a dynamic object backed by `ExpandoObject`
- you can add properties at runtime

Example:

```csharp
globals.Token = "abc";
int count = globals.Count;
```

Reserved predefined properties you can read:

- `globals.JobId`
- `globals.Resources`
- `globals.OwnerId`

Concurrency rules:

- if multiple bots mutate the same global state, protect it
- use `LOCK globals` for synchronous sections
- use `ACQUIRELOCK globals` / `RELEASELOCK globals` for async sections

`globals` is the only runtime variable available inside the startup script.
