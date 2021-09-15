### LoliCode specific statements

##### LOG
Prints text to the debugger log.
Example:
```
LOG "hello"
```
---
##### CLOG
Prints colored text to the debugger log.
A full list of colors is available [here](https://www.colorhexa.com/color-names) (remove dashes and spaces and apply PascalCase).
Example:
```
CLOG YellowGreen "hello"
```
---
##### JUMP
Jumps to a specified point in the code. Remember to watch out for endless loops!
Example:
```
...
#HERE
...
JUMP #HERE
```
---
##### REPEAT
Repeats something N times.
Example:
```
REPEAT 5
LOG "hello"
END
```
---
##### FOREACH
Iterates on a list variable.
Example:
```
BLOCK:ConstantList
  value = ["one", "two", "three"]
  => VAR @list
ENDBLOCK

FOREACH elem IN list
LOG elem
END
```
---
##### WHILE
Executes something while a condition is true.
Example:
```
WHILE INTKEY 1 LessThan 2
...
END
```
---
##### IF / ELSE / ELSE IF
Executes something, or something else.
Example:
```
IF INTKEY 5 LessThan 1
LOG "nope"
ELSE IF INTKEY 5 LessThan 3
LOG "nope again"
ELSE
LOG "yep"
END
```
---
##### TRY / CATCH
Executes something. If it fails, executes something else.
Example:
```
TRY
// request to an unreliable URL
CATCH
// fallback request to a reliable URL
END
```
---
##### LOCK
Very useful if you want to execute synchronous operations on global variables.
It makes sure that only 1 bot can enter a given piece of code at a time, so that multiple bots do not edit the same global variable at the same time.
Often used in conjunction with TRY/CATCH.
Example:
```
LOCK globals
TRY
// Try to increase globals.Count by 1 if it exists
globals.Count++;
CATCH
// Create globals.Count if it doesn't exist
globals.Count = 1;
END
END
```
---
##### ACQUIRELOCK / RELEASELOCK
Very useful if you want to execute asynchronous operations on global variables.
It makes sure that only 1 bot can enter a given piece of code at a time, so that multiple bots do not edit the same global variable at the same time.
You MUST use this in conjunction with TRY/CATCH/FINALLY.
Example:
```
ACQUIRELOCK globals
TRY
// Do some async operation here
CATCH
throw; // Rethrow any exception
FINALLY
RELEASELOCK globals
END
```
---
##### SET VAR/CAP
Sets a string variable, and optionally also marks it for capture. Introduced for consistency with OB1.
Example:
```
SET VAR @myString "variable"
LOG myString

SET CAP @myCapture "capture"
LOG myCapture
```
---
##### SET USEPROXY
Sets whether to use the currently set proxy or not.
Example:
```
SET USEPROXY TRUE
SET USEPROXY FALSE
```
---
##### SET PROXY
Sets a given proxy. The available types are: HTTP, SOCKS4, SOCKS4A, SOCKS5.
Example:
```
SET PROXY "127.0.0.1" 9050 SOCKS5
SET PROXY "127.0.0.1" 9050 SOCKS5 "username" "password"
```
---
##### MARK
Adds a variable to the capture.
Example:
```
MARK @myVar
```
---
##### UNMARK
Removes a variable from the capture.
Example:
```
UNMARK @myVar
```
---
##### TAKEONE
Takes a single item from a resource. You can configure resources in Config Settings > Data > Resources.
You need to provide the name of the resource and the name of the variable that will be created (of type `string`).
Example:
```
TAKEONE FROM "resourceName" => @myString
LOG myString
```
---
##### TAKE
Takes a multiple items from a resource. You can configure resources in Config Settings > Data > Resources.
You need to provide the name of the resource and the name of the variable that will be created (of type `List<string>`).
Example:
```
TAKE 5 FROM "resourceName" => @myList
```
