#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/../../.." && pwd)"
native_dir="$repo_root/RuriLib.Http/runtimes/osx-arm64/native"
source_file="$script_dir/curl_impersonate_shim.c"
output_file="$script_dir/osx-arm64/libcurl-impersonate-shim.dylib"

if [[ ! -f "$native_dir/libcurl-impersonate.dylib" ]]; then
  echo "Missing $native_dir/libcurl-impersonate.dylib" >&2
  echo "Run: dotnet run --project Tools/CurlImpersonate.NativeAssets -- --rid osx-arm64" >&2
  exit 1
fi

mkdir -p "$(dirname "$output_file")"

clang \
  -dynamiclib \
  -arch arm64 \
  -mmacosx-version-min=11.0 \
  "$source_file" \
  "$native_dir/libcurl-impersonate.dylib" \
  -Wl,-rpath,@loader_path \
  -install_name @rpath/libcurl-impersonate-shim.dylib \
  -o "$output_file"

install_name_tool \
  -change /Users/runner/work/_temp/install/lib/libcurl-impersonate.4.dylib \
  @loader_path/libcurl-impersonate.dylib \
  "$output_file"

shasum -a 256 "$output_file"
