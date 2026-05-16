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

Important concurrency rule:

- Use `LOCK globals` for synchronous shared-state changes.
- Use `ACQUIRELOCK globals` with `TRY / FINALLY / RELEASELOCK` when async work is involved.

These statements are often cleaner than forcing everything into blocks, especially around flow control and shared state.
