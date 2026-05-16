# Runtime Outcomes And Execution Model

This topic explains the runtime semantics that matter when authoring a config, not how to operate jobs in the UI.

## How a config actually executes

- the startup script runs once before bots start
- the main script runs once per bot
- in multi-run jobs, many bots can execute the main script in parallel
- in the debugger, there is only one bot and `data.BOTNUM` is `0`

Practical consequences:

- `input` and `data` are per-bot state
- `globals` is shared across bots
- a script that works in the debugger can still be wrong in a multi-run job if it mutates shared state unsafely

## Debugger vs multi-run from a config-making perspective

Debugger behavior:

- uses one bot only, so shared-state races do not show up naturally
- uses the config's default custom input values
- can use an optional test proxy

Multi-run behavior:

- many bots may hit the same logic at the same time
- operator-provided custom inputs can override config defaults
- proxies, retries, bans, and hit outputs affect how statuses behave operationally

Use `debug_config` to validate script logic, but do not treat a debugger pass as proof that shared-state logic is safe under concurrency.

## Shared state and locking

Use `globals` only for values that truly need to be shared across bots, such as:

- startup-produced auth tokens
- counters
- caches
- shared resources

If multiple bots can mutate the same shared value:

- use `LOCK globals` for synchronous critical sections
- use `ACQUIRELOCK globals` with `TRY / FINALLY / RELEASELOCK` for async critical sections

If a value does not need to be shared, keep it in per-bot state instead of `globals`.

## End statuses you should reason about

The final bot status is not just cosmetic. It affects retries, proxy handling, hits, and operator expectations.

Common statuses:

- `SUCCESS`: positive checked outcome
- `FAIL`: negative checked outcome
- `NONE`: ambiguous or manual-review outcome, commonly used as "to check"
- `RETRY`: retry the same data because the outcome looks transient
- `BAN`: retry the same data with a different proxy because the current proxy looks bad for this target
- `ERROR`: runtime failure path; treat it as a real failure signal, not normal business logic
- `CUSTOM` or environment-defined custom statuses: named outcome categories that count as hits

## Hits vs non-hits

By default, these statuses produce hits:

- `SUCCESS`
- `NONE`
- `CUSTOM`
- custom statuses defined in `Environment.ini`

Important implications:

- `FAIL` is normally not a hit
- `NONE` is a hit-type outcome even though it usually means "review later"
- do not invent custom statuses unless they already exist in the current environment

If you need custom statuses, inspect `get_environment` first and use only statuses that are actually defined there.

## How to choose statuses intentionally

Prefer:

- `SUCCESS` when the target was checked successfully and you found the positive condition you wanted
- `FAIL` when the target was checked successfully and the data is simply not valid for the goal
- `NONE` when the result is ambiguous and should be reviewed or retried manually later
- `RETRY` when the issue looks transient but not proxy-specific
- `BAN` when the response suggests the proxy is the problem, such as rate limits, captive portals, WAF blocks, or region denial

Avoid:

- using `ERROR` as a normal branch for expected outcomes
- using `BAN` as a catch-all for every unknown response

Overusing `BAN` is one of the easiest ways to create ban loops.

## Proxy-related status semantics

When proxies are in use:

- `BAN` should mean "the data may still be good, but this proxy is not"
- `RETRY` should mean "try again without blaming the proxy yet"
- `ERROR` often behaves operationally like a failure that can keep the data from finishing cleanly

Proxy outcomes are also shaped by config proxy settings, especially:

- ban statuses
- ban loop evasion
- allowed proxy types
- maximum uses per proxy

If your config uses `BAN` keychains, align them with the proxy settings instead of treating them in isolation.

## Ban loops and how config authors cause them

Typical causes:

- a broad `BAN` keychain that catches responses that are not actually proxy failures
- enabling "ban if no match" without understanding all likely response shapes
- classifying site logic bugs or parsing mistakes as proxy issues

What to do instead:

- make `BAN` conditions specific
- use `NONE` for ambiguous cases you have not modeled yet
- use `FAIL` for clean negative checks
- validate with `debug_config`, including a test proxy when proxy behavior matters

## Agent rules

- Do not invent custom statuses. Read the environment first.
- Do not assume debugger behavior proves multi-run correctness.
- Use `globals` only for true shared state, and lock shared mutations.
- Choose `FAIL`, `NONE`, `RETRY`, `BAN`, and `SUCCESS` deliberately because they have different operational meanings.
- If a config depends on proxy-sensitive classification, test the proxy branches explicitly instead of assuming `ERROR` or `BAN` is acceptable.
