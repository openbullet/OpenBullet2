# Data Rules

Data rules are config-level checks applied after a wordlist line has already been validated and sliced.

Use them when:

- the environment wordlist type is too broad
- a specific config needs extra constraints on one or more slices
- you want to discard bad data before hitting the target

Execution order:

1. the wordlist type regex validates the raw line
2. the line is split into slices
3. the config data rules validate the individual slices

Example:

- wordlist type exposes `KEYWORD` and `CODE`
- config data rules can require `KEYWORD` to start with `abc`
- config data rules can reject `CODE` values matching an unwanted pattern

Effects:

- lines failing data rules are marked `INVALID` in multi-run jobs
- this reduces unnecessary requests and speeds up runs

Data rules belong in config settings, not in `Environment.ini`.
