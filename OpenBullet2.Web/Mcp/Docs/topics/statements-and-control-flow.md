# Statements And Control Flow

LoliCode also has native statements outside blocks.

Common statements:

- `LOG`, `CLOG`: write to the debugger log
- `IF / ELSE IF / ELSE / END`: branching
- `TRY / CATCH / FINALLY / END`: exception handling
- `WHILE`, `REPEAT`, `FOREACH`: loops
- `JUMP`: goto-style flow control
- `SET VAR`, `SET CAP`, `MARK`, `UNMARK`: variable and capture operations
- `SET USEPROXY`, `SET PROXY`: proxy control
- `LOCK`: synchronous critical section for shared state
- `ACQUIRELOCK` / `RELEASELOCK`: async-safe critical section for shared state
- `TAKEONE`, `TAKE`: consume config resources

Capture guidance:

- Prefer `SET CAP` or block outputs with `=> CAP @var` when the value should be captured directly.
- Use `MARK` mainly for an already existing variable that must be added to capture later.
- Avoid the pattern “create inner-scope variable, then expect it to be captured at script end”.

Scope guidance:

- Variables created only inside inner scopes such as `IF`, `REPEAT`, `FOREACH`, and `WHILE` are not reliable final capture variables.
- If a value needs to survive to the end of the script, define the variable beforehand in outer scope and assign to it from inside the inner scope.

Important concurrency rule:

- Use `LOCK globals` for synchronous shared-state changes.
- Use `ACQUIRELOCK globals` with `TRY / FINALLY / RELEASELOCK` when async work is involved.

These statements are often cleaner than forcing everything into blocks, especially around flow control and shared state.
