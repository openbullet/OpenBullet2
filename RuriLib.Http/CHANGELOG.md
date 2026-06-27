# Changelog

## 2.0.1 - 2026-06-27

- Added random profile selection support for curl-impersonate.
- Improved curl-impersonate debug logging by recording the actual sent headers.
- Fixed header values being split on commas in curl-impersonate requests.
- Fixed curl-impersonate native loading on macOS arm64 (by meinname).
- Fixed zstd response decoding (by GekySan).
- Ignored malformed `Set-Cookie` headers safely.
- Added HTTPS proxy support through `RuriLib.Proxies`.

## 2.0.0 - 2026-06-06

Changes since `1.0.1`, released on 2022-03-09:

- Retargeted the package to .NET 10.
- Added curl-impersonate HTTP support, including browser profiles, custom profiles, native library loading, and packaged runtime assets.
- Added zstd response decoding support.
- Improved HTTP stream content decoding and ignored unsupported encoding values safely.
- Improved cookie parsing, including more reliable comma-separated cookie handling.
- Fixed plain HTTP requests sent over HTTP proxies.
- Added an option path for ignoring certificate validation.
- Improved Windows certificate authority handling for curl-impersonate.
- Added regression tests for content encodings, cookie parsing, HTTP proxy behavior, and curl-impersonate behavior.
