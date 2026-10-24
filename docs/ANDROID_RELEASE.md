# Android release validation

## Automated local gate

Use Unity `6000.0.23f1` with Android Build Support, OpenJDK, the Android SDK/NDK modules and `python3` installed.

```bash
UNITY_PATH=/path/to/Unity ./scripts/validate-unity.sh
```

The script verifies Unity `6000.0.23f1`, including the dot-delimited internal build metadata that Unity may append to `-version`, while rejecting other editor releases. It then runs all EditMode tests, validates the generated XML contains passing tests, and invokes `Wigglebites.Editor.AndroidBuild.BuildValidation` for a local validation APK. Outputs are restricted to the ignored `Builds/` and `Logs/` folders; `WIGGLEBITES_BUILD_PATH` may select another `.apk` path below `Builds/`.

Run the version-matching regression test without launching Unity:

```bash
./scripts/tests/unity-version-test.sh
```

For an Intel Android emulator, build the separate x86_64 development APK. This artifact is only for emulator QA and does not change the ARM64 validation or release configuration:

```bash
/path/to/Unity -batchmode -quit -projectPath "$PWD" \
  -buildTarget Android \
  -executeMethod Wigglebites.Editor.AndroidBuild.BuildEmulatorApk \
  -logFile Logs/android-emulator-build.log
```

The default output is `Builds/Android/Wigglebites-emulator.apk`. It uses x86_64 for the local debug emulator; ARM64 validation and release builds are unchanged. As with other local builds, `WIGGLEBITES_BUILD_PATH` may override it with another `.apk` path below `Builds/`.

`Wigglebites.Editor.AndroidBuild.BuildRelease` is the separate release entry point. It fails closed unless Unity has a custom keystore, alias and passwords. A clean batch job can inject them with `WIGGLEBITES_KEYSTORE_PATH`, `WIGGLEBITES_KEYSTORE_PASSWORD`, `WIGGLEBITES_KEY_ALIAS` and `WIGGLEBITES_KEY_PASSWORD`; all four must be present together. The values are restored after the build and are never logged. Set `WIGGLEBITES_BUILD_AAB=1` for an Android App Bundle and use a `.aab` path below `Builds/`. Signing files and credentials must remain outside the repository and must never be written to build logs.

## Device acceptance checklist

Declare the device model, Android version, chipset, RAM, display resolution/density and build commit in the test record.

1. Install a clean build and confirm the menu appears within five seconds after launch.
2. Play a full three-minute round with 1 player, 19 bots and 350 food objects.
3. Confirm drag steering, hold-to-boost, pause/resume, death results and replay.
4. Verify every combat outcome follows the 1.10 mass threshold: body bites create candy and leave the head-side snake alive, head bites eliminate, and no double elimination appears.
5. Check top/bottom cutouts, gesture navigation, 56dp controls and Swedish text at the device's native density.
6. Toggle sound and reduced effects, restart the app and confirm both settings and best mass persist.
7. Interrupt the app during play and after results, then resume/relaunch and confirm no corrupt save or frozen state.
8. Capture average FPS, 1% low frame time, peak managed/native memory and thermal state for a full match. Acceptance target: stable 60 FPS without visible hitching or unbounded memory growth.
9. Repeat the full match with reduced effects and compare frame/memory evidence.
10. Record APK/AAB size, test results, build logs and any device-specific deviations before release approval.

## Current validation status

Unity `6000.0.23f1`, its Android tooling and an active Unity Personal entitlement were verified on the current host on 2026-10-22. A validation build completed successfully and produced an ARM64 APK for `com.wigglebites.snakearena` targeting Android 15 / API 35 with a minimum of API 26. The generated manifest requests no Android permissions.

The last recorded automated baseline passed 41 EditMode tests and produced the validation APK successfully before the segmented-combat and visual refresh. The expanded suite and APK must be rerun with Unity `6000.0.23f1` before this change is treated as validated. An earlier API 35 x86_64 emulator smoke test verified menu launch, match start, visible steering direction, active turbo feedback, pause and resume. The generated APKs are local evidence only, use non-release signing and have not completed the physical-device acceptance checklist. Do not label the commit as a release candidate until the current checklist passes with the intended release artifact.
