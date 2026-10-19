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

`Wigglebites.Editor.AndroidBuild.BuildRelease` is the separate release entry point. It fails closed unless Unity has a custom keystore, alias and passwords. A clean batch job can inject them with `WIGGLEBITES_KEYSTORE_PATH`, `WIGGLEBITES_KEYSTORE_PASSWORD`, `WIGGLEBITES_KEY_ALIAS` and `WIGGLEBITES_KEY_PASSWORD`; all four must be present together. The values are restored after the build and are never logged. Set `WIGGLEBITES_BUILD_AAB=1` for an Android App Bundle and use a `.aab` path below `Builds/`. Signing files and credentials must remain outside the repository and must never be written to build logs.

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

The repository-side static gates and Unity version-matching regression test pass. Unity and the Android tooling are installed on the current host, but Unity is not yet licensed, so no Unity build or Android device evidence has been produced. Do not label a commit as a release candidate until the automated gate and checklist above are completed after license activation.
