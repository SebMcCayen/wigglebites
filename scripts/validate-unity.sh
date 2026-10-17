#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_bin="${UNITY_PATH:-}"
if [[ -z "$unity_bin" ]]; then
  default_unity="/Applications/Unity/Hub/Editor/6000.0.23f1/Unity.app/Contents/MacOS/Unity"
  if [[ -x "$default_unity" ]]; then
    unity_bin="$default_unity"
  elif command -v Unity >/dev/null 2>&1; then
    unity_bin="$(command -v Unity)"
  else
    echo "Unity 6000.0.23f1 was not found. Set UNITY_PATH to the Unity executable." >&2
    exit 2
  fi
fi

mkdir -p "$project_dir/Builds/Validation" "$project_dir/Logs"
"$unity_bin" -batchmode -nographics -quit \
  -projectPath "$project_dir" \
  -runTests -testPlatform EditMode \
  -testResults "$project_dir/Builds/Validation/editmode-results.xml" \
  -logFile "$project_dir/Logs/editmode-tests.log"

if [[ "${WIGGLEBITES_BUILD_AAB:-0}" == "1" ]]; then
  build_path="Builds/Android/Wigglebites.aab"
else
  build_path="Builds/Android/Wigglebites.apk"
fi

WIGGLEBITES_BUILD_PATH="$build_path" \
"$unity_bin" -batchmode -nographics -quit \
  -projectPath "$project_dir" \
  -executeMethod Wigglebites.Editor.AndroidBuild.BuildRelease \
  -logFile "$project_dir/Logs/android-build.log"

echo "Validation complete: EditMode tests and Android APK build succeeded."
