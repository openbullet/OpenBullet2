import asyncio
from typing import Any

# Cache compiled wrappers by script hash plus signature so repeated executions
# skip the dynamic exec/compile step.
_CACHE: dict[str, Any] = {}


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
    script_hash: str,
    script_source: str,
    inputs: dict[str, Any],
    output_names: list[str],
) -> dict[str, Any]:
    input_names = [str(name) for name in inputs.keys()]
    output_names = [str(name) for name in output_names]
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
