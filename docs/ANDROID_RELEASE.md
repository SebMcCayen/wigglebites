# Android release validation

## Automated local gate

Use Unity `6000.0.23f1` with Android Build Support, OpenJDK and the Android SDK/NDK modules installed.

```bash
UNITY_PATH=/path/to/Unity ./scripts/validate-unity.sh
```

The script runs all EditMode tests and then invokes `Wigglebites.Editor.AndroidBuild.BuildRelease`. Outputs are written only below the ignored `Builds/` and `Logs/` folders. Set `WIGGLEBITES_BUILD_AAB=1` for an Android App Bundle or `WIGGLEBITES_BUILD_PATH` for another project-relative `.apk`/`.aab` output. Release signing credentials must stay outside the repository and be configured through Unity or the secure build environment.

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
