### Syntax of blocks in LoliCode
Optional elements are enclosed in square brackets.
```
BLOCK:Id
[LABEL:Custom label]
[DISABLED]
  [settingName = settingValue]
  [=> VAR/CAP @outputVariable]
ENDBLOCK
```
- `Id`: the unique identifier of the block
- `settingName`: the unique name of a given setting of the block
- `settingValue`: see below
- `=> VAR/CAP @outputVariable`: if the block returns a value, you can define if it needs to be
a normal variable (`VAR`) or also marked as capture (`CAP`)
---
##### Setting values
Setting values can have 3 types:
- Fixed (e.g. `"hello"` or `123`)
- Interpolated (e.g. `$"My name is <name>"`)
- Variable (e.g. `@name`)
---
##### Fixed value types
- Bool (`true` or `false`)
- Int (e.g. `123`)
- Float (e.g. `0.42`)
- String (e.g. `"hello"`)
- Byte Array (as base64 e.g. `plvB6Yer`)
- List of Strings (e.g. `["one", "two", "three"]`)
- Dictionary of Strings (e.g. `{("one", "first"), ("two", "second"), ("three", "third")}`)
---
##### Interpolated value types
- String (e.g. `$"This is my <name>"`)
- List of Strings (e.g. `$["one", "<secondNumber>", "three"]`)
- Dictionary of Strings (e.g. `${("one", "first"), ("<secondNumber>", "second"), ("three", "third")}`)
---
##### Final notes
- There is automatic type casting, so you can use a variable of type `Int` in a setting of type `String`