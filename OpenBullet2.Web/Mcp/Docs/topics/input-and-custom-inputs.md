# Input And Custom Inputs

The `input` variable contains:

- the current data line, sliced according to the selected wordlist type
- any custom inputs defined in config settings

Wordlist slices:

- names come from the `Slices` field of the chosen wordlist type
- examples: `input.DATA`, `input.USERNAME`, `input.PASSWORD`
- these values are always strings

Custom inputs:

- are configured in Config Settings > Data > Inputs
- let the operator provide extra values such as API keys, domains, tenant ids, or static secrets
- are also exposed under `input.*`
- are also strings

Examples:

- `input.apiKey`
- `input.baseUrl`
- `input.USERNAME`

Runtime notes:

- in a multi-run job, the operator can override custom input values per run
- in the debugger, the defaults from config settings are used

If a script needs a value under `input.*`, make sure either the wordlist type or the config custom inputs actually define it.
