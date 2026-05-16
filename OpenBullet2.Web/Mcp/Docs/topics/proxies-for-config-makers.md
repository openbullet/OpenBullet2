# Proxies For Config Makers

Proxy behavior matters at both the script level and the config settings level.

Config-level concerns:

- whether proxies are suggested by default
- maximum uses per proxy
- ban loop evasion
- which end statuses ban a proxy
- allowed proxy types

Script-level concerns:

- `data.UseProxy`: whether the bot is currently using a proxy
- `data.Proxy`: current proxy information, or `null`
- `SET USEPROXY` and `SET PROXY`: explicit proxy control in script
- keycheck logic can produce `BAN` to mark bad proxies

Debugger `testProxy` syntax:

- `127.0.0.1:8000`
- `127.0.0.1:8000:username:password`
- `(socks5)127.0.0.1:8000`
- `(http)127.0.0.1:8000:username:password`

Important rule:

- if the proxy string does not include a type prefix, the debugger's separate proxy type option is used as the default type
- if the proxy string includes a prefix like `(http)` or `(socks5)`, that explicit type is used instead

Useful `data.Proxy` fields:

- `Type`
- `Host`
- `Port`
- `Username`
- `Password`
- `WorkingStatus`
- `Country`
- `Ping`
- `LastUsed`
- `ProxyStatus`

Important safety rule:

- `data.Proxy` can be `null`, so null-check it before using proxy properties in C#

If a config depends on proxies, align both the proxy settings and the keycheck behavior, then validate with `debug_config` using a test proxy.
