# Wordlists And Environment

This topic is the extended reference for wordlist behavior beyond the core guide.

## Validation order

Wordlist handling happens in this order:

1. the raw data line is checked against the selected wordlist type
2. if validation passes, the line is split using the configured separator
3. the resulting slices become `input.*` variables
4. config-level data rules, if any, are applied after slicing

This means:

- environment-level regex validates the raw line shape
- config-level data rules validate the already parsed slices

## Example

Wordlist type:

```ini
[WORDLIST TYPE]
Name=KeywordsCodes
Regex=^[a-z]{4,8}:[0-9]{6}$
Verify=True
Separator=:
Slices=KEYWORD,CODE
```

Then:

- `rainbow:456723` is valid
- `rainbow:456723` becomes `input.KEYWORD` and `input.CODE`
- invalid lines are discarded before the config runs

## Default behavior for new configs

The first wordlist type in `Environment.ini` becomes the default one suggested to newly created configs. That affects the initial allowed wordlist type of new configs.

## Restart rule

Changes to `Environment.ini` require restarting OpenBullet 2 before they take effect.

## Data rules vs environment regex

Prefer `Environment.ini` when:

- the data shape is globally true for that wordlist type
- every config using that type should agree on the same raw-line format

Prefer config data rules when:

- a config needs stricter constraints than the base wordlist type
- the extra checks apply to parsed slices, not just the raw line
- the rule is specific to one target or one config family
