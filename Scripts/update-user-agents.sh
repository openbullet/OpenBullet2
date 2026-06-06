#!/usr/bin/env bash

set -euo pipefail

usage() {
  cat <<'EOF'
Usage: Scripts/update-user-agents.sh [version]

Downloads the published `user-agents` npm package, extracts
`package/dist/user-agents.json`, validates it, and copies it to:

  - OpenBullet2.Web/user-agents.json
  - OpenBullet2.Native/user-agents.json

Arguments:
  version   Optional npm version or tag. Defaults to `latest`.

Examples:
  Scripts/update-user-agents.sh
  Scripts/update-user-agents.sh 2.1.41
EOF
}

if [[ "${1:-}" == "-h" || "${1:-}" == "--help" ]]; then
  usage
  exit 0
fi

require_command() {
  local command_name="$1"
  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Missing required command: $command_name" >&2
    exit 1
  fi
}

require_command npm
require_command node
require_command tar
require_command mktemp
require_command cp
require_command cmp
require_command sha256sum

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/.." && pwd)"

PACKAGE_NAME="user-agents"
REQUESTED_VERSION="${1:-latest}"
PACKAGE_SPEC="${PACKAGE_NAME}@${REQUESTED_VERSION}"

TARGET_FILES=(
  "$REPO_ROOT/OpenBullet2.Web/user-agents.json"
  "$REPO_ROOT/OpenBullet2.Native/user-agents.json"
)

for target in "${TARGET_FILES[@]}"; do
  if [[ ! -f "$target" ]]; then
    echo "Missing target file: $target" >&2
    exit 1
  fi
done

TEMP_DIR="$(mktemp -d)"
cleanup() {
  rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

echo "Packing ${PACKAGE_SPEC} from npm..."
TARBALL_NAME="$(cd "$TEMP_DIR" && npm pack "$PACKAGE_SPEC" --silent)"
TARBALL_PATH="$TEMP_DIR/$TARBALL_NAME"
PACKAGE_JSON_PATH="$TEMP_DIR/package.json"
SOURCE_JSON_PATH="$TEMP_DIR/user-agents.json"

tar -xOf "$TARBALL_PATH" package/package.json > "$PACKAGE_JSON_PATH"
tar -xOf "$TARBALL_PATH" package/dist/user-agents.json > "$SOURCE_JSON_PATH"

RESOLVED_VERSION="$(
  node -e "const fs=require('fs'); console.log(JSON.parse(fs.readFileSync(process.argv[1], 'utf8')).version);" \
    "$PACKAGE_JSON_PATH"
)"

node - "$SOURCE_JSON_PATH" <<'EOF'
const fs = require('fs');

const filePath = process.argv[2];
const content = fs.readFileSync(filePath, 'utf8');
const data = JSON.parse(content);

if (!Array.isArray(data) || data.length === 0) {
  throw new Error('Expected a non-empty JSON array.');
}

const invalidEntries = data.filter(
  (entry) =>
    !entry ||
    typeof entry.userAgent !== 'string' ||
    entry.userAgent.trim() === '' ||
    typeof entry.platform !== 'string' ||
    entry.platform.trim() === '' ||
    typeof entry.weight !== 'number',
);

if (invalidEntries.length > 0) {
  throw new Error(`Found ${invalidEntries.length} invalid user-agent entries.`);
}
EOF

SHA256="$(sha256sum "$SOURCE_JSON_PATH" | awk '{print $1}')"

echo "Imported ${PACKAGE_NAME}@${RESOLVED_VERSION}"
echo "SHA256: ${SHA256}"

CHANGED=0
for target in "${TARGET_FILES[@]}"; do
  relative_target="${target#"$REPO_ROOT"/}"
  if cmp -s "$SOURCE_JSON_PATH" "$target"; then
    echo "Unchanged: ${relative_target}"
    continue
  fi

  cp "$SOURCE_JSON_PATH" "$target"
  echo "Updated:   ${relative_target}"
  CHANGED=1
done

if [[ "$CHANGED" -eq 0 ]]; then
  echo "No updates were necessary."
fi
