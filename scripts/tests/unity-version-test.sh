#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
source "$project_dir/scripts/lib/unity-version.sh"

required_version="6000.0.23f1"

for reported_version in \
  "6000.0.23f1" \
  "6000.0.23f1.7976.6000.0/staging.4246"; do
  if ! wigglebites_unity_version_matches "$required_version" "$reported_version"; then
    echo "Expected accepted Unity version: $reported_version" >&2
    exit 1
  fi
done

for reported_version in \
  "" \
  "6000.0.23f10" \
  "6000.0.23f1beta" \
  "6000.0.24f1"; do
  if wigglebites_unity_version_matches "$required_version" "$reported_version"; then
    echo "Expected rejected Unity version: ${reported_version:-empty}" >&2
    exit 1
  fi
done

echo "Unity version matching tests passed."
