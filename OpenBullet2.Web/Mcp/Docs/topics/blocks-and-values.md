# Blocks And Values

Blocks are the smallest standard work units in an OB2 config.

Syntax:

```loli
BLOCK:Id
[LABEL:Custom label]
[DISABLED]
[SAFE]
  [settingName = settingValue]
  [=> VAR/CAP @outputVariable]
ENDBLOCK
```

Important parts:

- `BLOCK:Id`: the block identifier
- `LABEL`: optional custom log/UI label
- `DISABLED`: block is skipped
- `SAFE`: supported block exceptions are caught and stored in `data.ERROR`
- `settingName = settingValue`: block settings
- `=> VAR` or `=> CAP`: capture the return value when the block has one

Setting value forms:

- fixed: `"hello"`, `123`, `true`, `GET`, `["a", "b"]`
- interpolated: `$"Hello <input.USERNAME>"`
- variable: `@token`

Fixed types commonly exposed by blocks:

- bool
- int
- float
- string
- enum
- byte array
- list of strings
- dictionary of strings

Do not guess setting names, value types, or enum members. Always call `get_block_details` before writing a block.
