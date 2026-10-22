#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "$project_dir/scripts/lib/unity-version.sh"

reject_linked_path_components() {
  local candidate="$1"
  local current="$candidate"
  while [[ "$current" != "$project_dir" && "$current" != "/" ]]; do
    if [[ -L "$current" ]]; then
      echo "Validation path must not contain symbolic links: $current" >&2
      exit 2
    fi
    current="$(dirname "$current")"
  done
  if [[ "$current" != "$project_dir" ]]; then
    echo "Validation path escaped the project directory: $candidate" >&2
    exit 2
  fi
}

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

unity_version_output="$("$unity_bin" -version 2>/dev/null | tr -d '\r')"
unity_version_line="${unity_version_output%%$'\n'*}"
unity_version="${unity_version_line%%[[:space:]]*}"
if ! wigglebites_unity_version_matches "6000.0.23f1" "$unity_version"; then
  echo "Expected Unity 6000.0.23f1, found: ${unity_version:-unknown}" >&2
  exit 2
fi

reject_linked_path_components "$project_dir/Builds/Validation"
reject_linked_path_components "$project_dir/Logs"
mkdir -p "$project_dir/Builds/Validation" "$project_dir/Logs"
test_results="$project_dir/Builds/Validation/editmode-results.xml"
editmode_log="$project_dir/Logs/editmode-tests.log"
android_build_log="$project_dir/Logs/android-build.log"
for output_file in "$test_results" "$editmode_log" "$android_build_log"; do
  if [[ -L "$output_file" ]]; then
    echo "Validation output must not be a symbolic link: $output_file" >&2
    exit 2
  fi
done
rm -f "$test_results"
"$unity_bin" -batchmode -nographics \
  -projectPath "$project_dir" \
  -runTests -testPlatform EditMode \
  -testResults "$test_results" \
  -logFile "$editmode_log"

python3 - "$test_results" <<'PY'
import pathlib
import sys
import xml.etree.ElementTree as ET

path = pathlib.Path(sys.argv[1])
if not path.is_file():
    raise SystemExit("Unity did not produce an EditMode result file")
root = ET.parse(path).getroot()
cases = root.findall(".//test-case")
if not cases:
    raise SystemExit("Unity reported zero EditMode tests")
passed = sum(1 for case in cases if case.get("result", "").lower() == "passed")
non_passing = len(cases) - passed
root_result = root.get("result", "").lower()
if passed == 0 or non_passing or root.get("failed", "0") not in {"0", None} or root_result != "passed":
    raise SystemExit(
        f"Unity EditMode tests did not fully pass: {passed} passed, {non_passing} failed/skipped/inconclusive"
    )
print(f"Unity EditMode tests passed: {passed} test cases")
PY

build_path="${WIGGLEBITES_BUILD_PATH:-Builds/Android/Wigglebites.apk}"

WIGGLEBITES_BUILD_PATH="$build_path" \
"$unity_bin" -batchmode -nographics -quit \
  -projectPath "$project_dir" \
  -buildTarget Android \
  -executeMethod Wigglebites.Editor.AndroidBuild.BuildValidation \
  -logFile "$android_build_log"

echo "Validation complete: EditMode tests and Android validation APK succeeded at $build_path."
