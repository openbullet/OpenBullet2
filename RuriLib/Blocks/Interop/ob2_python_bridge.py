import asyncio
import contextvars
import sys
from typing import Any

if sys.platform == "win32":
    try:
        asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())
    except Exception:
        pass

# Cache compiled wrappers by script hash plus signature so repeated executions
# skip the dynamic exec/compile step.
_CACHE: dict[str, Any] = {}
_LOGS: dict[str, tuple[str, str]] = {}
_STDOUT_BUFFER: contextvars.ContextVar[list[str] | None] = contextvars.ContextVar(
    "ob2_stdout_buffer", default=None
)
_STDERR_BUFFER: contextvars.ContextVar[list[str] | None] = contextvars.ContextVar(
    "ob2_stderr_buffer", default=None
)


class _BufferedStream:
    def __init__(self, buffer: contextvars.ContextVar[list[str] | None], fallback: Any) -> None:
        self._buffer = buffer
        self._fallback = fallback
        self.encoding = getattr(fallback, "encoding", "utf-8")

    def write(self, value: Any) -> int:
        text = "" if value is None else str(value)
        chunks = self._buffer.get()

        if chunks is None:
            return self._fallback.write(text)

        chunks.append(text)
        return len(text)

    def flush(self) -> None:
        if self._buffer.get() is None:
            self._fallback.flush()

    def isatty(self) -> bool:
        return False

    def writable(self) -> bool:
        return True


def _ensure_stream_hooks() -> None:
    if not isinstance(sys.stdout, _BufferedStream):
        sys.stdout = _BufferedStream(_STDOUT_BUFFER, sys.stdout)

    if not isinstance(sys.stderr, _BufferedStream):
        sys.stderr = _BufferedStream(_STDERR_BUFFER, sys.stderr)


def _indent(script_source: str) -> str:
    lines = script_source.splitlines()
    if not lines:
        return "    pass"

    return "\n".join(("    " + line) if line else "" for line in lines)


def _make_return_block(output_names: list[str]) -> str:
    if not output_names:
        return "    return {}"

    lines = ["    return {"]
    for name in output_names:
        lines.append(f"        {name!r}: {name},")
    lines.append("    }")
    return "\n".join(lines)


def _normalize(value: Any) -> Any:
    if value is None or isinstance(value, (str, int, float, bool)):
        return value

    if isinstance(value, (bytes, bytearray, memoryview)):
        return bytes(value)

    if isinstance(value, dict):
        return {str(k): _normalize(v) for k, v in value.items()}

    if isinstance(value, (list, tuple, set)):
        return [_normalize(v) for v in value]

    raise TypeError(
        f"Unsupported Python output type {type(value).__name__}. "
        "Serialize custom objects to strings if you need to pass them back to OpenBullet."
    )


async def run(
    invocation_id: str,
    script_hash: str,
    script_source: str,
    inputs: dict[str, Any],
    output_names: list[str],
) -> dict[str, Any]:
    _ensure_stream_hooks()

    stdout_chunks: list[str] = []
    stderr_chunks: list[str] = []
    stdout_token = _STDOUT_BUFFER.set(stdout_chunks)
    stderr_token = _STDERR_BUFFER.set(stderr_chunks)
    input_names = [str(name) for name in inputs.keys()]
    output_names = [str(name) for name in output_names]

    try:
        # The wrapper shape depends on both the script body and the declared IO shape.
        cache_key = f"{script_hash}|{','.join(input_names)}|{','.join(output_names)}"
        fn = _CACHE.get(cache_key)

        if fn is None:
            args = ", ".join(input_names)
            wrapper = "\n".join(
                [
                    f"async def __ob2_entry__({args}):",
                    _indent(script_source),
                    _make_return_block(output_names),
                ]
            )

            namespace: dict[str, Any] = {}
            exec(compile(wrapper, f"<ob2:{script_hash}>", "exec"), namespace, namespace)
            fn = namespace["__ob2_entry__"]
            _CACHE[cache_key] = fn

        kwargs = dict(inputs)
        # CSnakes awaits this coroutine from .NET, so asyncio cancellation can reach user code.
        result = await fn(**kwargs)
        return _normalize(result)
    finally:
        _LOGS[invocation_id] = ("".join(stdout_chunks), "".join(stderr_chunks))
        _STDOUT_BUFFER.reset(stdout_token)
        _STDERR_BUFFER.reset(stderr_token)


def take_logs(invocation_id: str) -> tuple[str, str]:
    return _LOGS.pop(invocation_id, ("", ""))
