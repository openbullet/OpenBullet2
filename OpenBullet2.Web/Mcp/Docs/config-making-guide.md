# OpenBullet 2 Config Making Guide

Use this guide first, then request deeper topics only when needed.

## What a config actually contains

An OB2 config is not just a script. It usually includes:

- metadata: name, author, category
- settings: allowed wordlist types, custom inputs, proxy defaults, resources, data rules, script usings
- main script: the per-bot logic
- optional startup script: shared initialization that runs before the main script
- optional readme: operator-facing usage notes

## Core scripting model

- The main script runs once per bot.
- The startup script runs before bots start and is meant for shared state.
- In multi-run jobs, many bots can execute the main script in parallel, so cross-bot state must go through `globals` and proper locking.
- LoliCode is the normal authoring surface, but it compiles to C# and can contain inline C# directly.
- Blocks, LoliCode statements, and C# can live in the same script.
- LoliCode and C# can go hand-in-hand inside the same config when that produces the cleanest result.
- If no suitable block exists, write the missing logic in C# instead of forcing the wrong block.

## Runtime variable rules you must not violate

- In the main script, the important runtime objects are `input`, `data`, and `globals`.
- In the startup script, only `globals` is available.
- `input.*` values come from wordlist slices and custom inputs, and they are strings.
- `data` contains per-bot runtime state such as source, cookies, headers, response code, proxy, logger, captures, and config settings.
- `globals` is shared across bots and is the right place for startup-produced shared state.

## Wordlists and environment are core

Most real configs depend on the currently configured wordlist types in `Environment.ini`.

Each wordlist type defines:

- `Name`: the wordlist type name
- `Regex`: how a raw line is validated
- `Verify`: whether the regex is enforced before execution
- `Separator`: how the line is split
- `Slices`: which `input.*` variable names are produced after splitting

Example:

```ini
[WORDLIST TYPE]
Name=Credentials
Regex=^.*:.*$
Verify=True
Separator=:
Slices=USERNAME,PASSWORD
```

Then a line like `john:secret` can produce:

- `input.USERNAME = "john"`
- `input.PASSWORD = "secret"`

Agent implications:

- do not assume a slice exists unless the selected wordlist type defines it
- align config settings such as allowed wordlist types with the actual environment
- use custom inputs only for extra values that do not naturally belong in the wordlist line
- changing `Environment.ini` is outside normal config-making flow and requires an OB2 restart

## Recommended config-making flow

1. If you need a new config, call `create_config`.
2. Call `get_environment` to inspect the configured wordlist types and other environment-level data.
3. Call `get_config_making_guide` first, then `get_config_making_topic` for the deeper topics you actually need.
4. Call `list_blocks` to discover candidate blocks by category.
5. Before using any block, call `get_block_details` for its exact parameter names, parameter types, enum options, extra info, and return type.
6. Call `get_config_settings` and `update_config_settings` to align allowed wordlist types, custom inputs, proxy behavior, resources, data rules, and script usings with the script you are writing.
7. Call `get_config_lolicode` and `update_config_lolicode` to read and edit the main script, startup script, and custom usings.
8. If you need to understand what will actually run, call `convert_lolicode_to_csharp` and inspect the emitted C#.
9. Call `debug_config` with test data, and a test proxy when relevant, to validate the real runtime behavior.
10. Finish with `get_config_metadata` / `update_config_metadata` and `get_config_readme` / `update_config_readme` when the config needs to be usable by humans.

## When to prefer blocks vs C#

Prefer blocks when:

- a standard block already models the operation well
- the block gives you built-in UI semantics, safer parameter typing, or keycheck integration
- the operation is common OB2 behavior such as requests, parsing, keycheck, constants, random values, or script interpreters

Prefer inline C# when:

- you need glue logic between blocks
- you need small helper methods, LINQ, regex, custom parsing, or branching that would be awkward in pure blocks
- no existing block exposes the exact behavior you need
- you need to work with .NET types directly

Use both together when that is the cleanest result. In OB2, that is normal, not a fallback.

## Validation rules for agents

- Never invent block settings or enum values. Read them from `get_block_details`.
- Do not assume startup can read `input` or `data`; it cannot.
- Do not assume a wordlist slice exists unless it is defined by the selected wordlist type in `Environment.ini`.
- Do not invent custom statuses. If you need non-standard statuses, inspect `get_environment` first and use only statuses that actually exist there.
- Prefer `CAP` when a value should be captured directly, instead of `VAR` followed by a later `MARK`, unless you are intentionally marking an already existing variable.
- Variables created only inside inner scopes such as `IF`, `REPEAT`, `FOREACH`, `WHILE`, and similar blocks are not guaranteed to persist at the end of the script, so they may not appear in the final capture output.
- If a value must be captured after inner-scope logic, define the variable beforehand in outer scope and then assign to it inside the scope.
- If your script uses extra namespaces, make sure the custom usings are present in config settings.
- If your script needs shared auth tokens, cookies, counters, or resources, prefer startup plus `globals`.
- Choose end statuses intentionally: `FAIL`, `NONE`, `RETRY`, `BAN`, `ERROR`, and `SUCCESS` have different effects on retries, hits, and proxies.
- If your script depends on proxy behavior, align the config proxy settings and test with `debug_config`.

## Troubleshooting loop

- If the script shape is unclear, inspect the current script with `get_config_lolicode`.
- If compilation behavior is unclear, inspect emitted C# with `convert_lolicode_to_csharp`.
- If the config runs but behaves incorrectly, use `debug_config` and read the plain log, variables, and final error.
- If the data format is wrong, revisit `get_environment`, allowed wordlist types, custom inputs, and data rules.

## Deep-dive topics

Request them through `get_config_making_topic` using one of the topic ids returned alongside this guide.
