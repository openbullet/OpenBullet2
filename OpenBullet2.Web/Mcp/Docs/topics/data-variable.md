# Data Variable

`data` contains runtime state for the current bot.

Very useful properties:

- `data.STATUS`: current bot status
- `data.RAWSOURCE`: last HTTP response body as `byte[]`
- `data.SOURCE`: last HTTP response body as `string`
- `data.ERROR`: last caught safe-mode error
- `data.ADDRESS`: last absolute response address
- `data.RESPONSECODE`: last HTTP status code
- `data.COOKIES`: cookies seen so far
- `data.HEADERS`: headers of the last response
- `data.Objects`: disposable shared objects for the current bot
- `data.MarkedForCapture`: variables marked for capture
- `data.BOTNUM`: bot number, `0` in debugger

Useful nested state:

- `data.Line.Data`: the whole unsplit data line
- `data.Line.Retries`: retry count
- `data.Proxy`: current proxy, or `null` if proxies are off
- `data.Logger.Enabled`: whether logging is enabled

Useful methods:

- `data.MarkForCapture(string varName)`
- `data.Logger.Log(...)`
- `data.Logger.LogObject(...)`
- `data.Logger.Clear()`

Other runtime objects exposed through `data`:

- `data.ConfigSettings`
- `data.Providers`
- `data.Random`
- `data.CancellationToken`
- `data.AsyncLocker`
- `data.Stepper`
- `data.ExecutionInfo`

Use `data` for per-bot state. Do not try to use it inside the startup script.
