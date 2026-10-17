# Android release validation

## Automated local gate

Use Unity `6000.0.23f1` with Android Build Support, OpenJDK and the Android SDK/NDK modules installed.

```bash
UNITY_PATH=/path/to/Unity ./scripts/validate-unity.sh
```

The script verifies the exact Unity version, runs all EditMode tests, validates the generated XML contains passing tests, and then invokes `Wigglebites.Editor.AndroidBuild.BuildValidation` for a local validation APK. Outputs are restricted to the ignored `Builds/` and `Logs/` folders; `WIGGLEBITES_BUILD_PATH` may select another `.apk` path below `Builds/`.

`Wigglebites.Editor.AndroidBuild.BuildRelease` is the separate release entry point. It fails closed unless Unity has a custom keystore, alias and passwords supplied by the secure build environment. Set `WIGGLEBITES_BUILD_AAB=1` for an Android App Bundle and use a `.aab` path below `Builds/`. Signing files and credentials must remain outside the repository and must never be written to build logs.

## Device acceptance checklist

Declare the device model, Android version, chipset, RAM, display resolution/density and build commit in the test record.

1. Install a clean build and confirm the menu appears within five seconds after launch.
2. Play a full three-minute round with 1 player, 19 bots and 350 food objects.
3. Confirm drag steering, hold-to-boost, pause/resume, death results and replay.
4. Verify every combat outcome follows the 1.10 mass threshold and no double elimination appears.
5. Check top/bottom cutouts, gesture navigation, 56dp controls and Swedish text at the device's native density.
6. Toggle sound and reduced effects, restart the app and confirm both settings and best mass persist.
7. Interrupt the app during play and after results, then resume/relaunch and confirm no corrupt save or frozen state.
8. Capture average FPS, 1% low frame time, peak managed/native memory and thermal state for a full match. Acceptance target: stable 60 FPS without visible hitching or unbounded memory growth.
9. Repeat the full match with reduced effects and compare frame/memory evidence.
10. Record APK/AAB size, test results, build logs and any device-specific deviations before release approval.

## Current validation status

The repository-side static gates pass, but no Unity build or Android device evidence has been produced on the current host because Unity, Android SDK/adb and .NET are unavailable. Do not label a commit as a release candidate until the automated gate and checklist above are completed on an equipped machine.
