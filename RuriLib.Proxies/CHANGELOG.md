# Changelog

## 2.0.1 - 2026-06-27

- Added HTTPS proxy support.
- Added prefixed stream and proxy connection helpers used by HTTPS proxy tunneling.
- Improved proxy connection metadata handling.

## 2.0.0 - 2026-06-06

Changes since `1.0.2`, released on 2022-03-09:

- Retargeted the package to .NET 10.
- Improved SOCKS proxy client reliability.
- Added cancellation token propagation during DNS resolution.
- Preferred IPv4 addresses over IPv6 addresses when resolving hosts.
- Fixed plain HTTP requests over HTTP proxies.
- Added `BadProxyException` and improved bad proxy classification for non-working proxies.
- Refactored proxy settings, clients, and tests with nullable reference types enabled.
- Added regression tests for proxy clients and host resolution behavior.
