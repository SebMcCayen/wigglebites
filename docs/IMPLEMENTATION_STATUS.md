# Implementation status

## Current vertical slice

- Unity 6 project structure and Android portrait player settings.
- Deterministic 30 Hz simulation separated from MonoBehaviour presentation.
- Touch-drag and keyboard steering with capped turn rate.
- Player snake, one local collector bot, arena boundary, food pickup and mass growth.
- Runtime-generated placeholder arena and snake visuals.
- EditMode tests for movement, steering, arena bounds, food collection and bot targeting.

## Still required for MVP

- Boost and mass drain.
- Full 19-bot roster, four personalities and staggered AI decisions.
- Size-based snake combat, elimination drops, safe spawning and respawns.
- Match timer, leaderboard, pause/results/replay flows and local persistence.
- Approved Neon Arena art direction, UI, VFX, audio and reduced-effects support.
- Android build, device profiling and release-candidate validation.

Unity editor, Android SDK and emulator validation must be run on a host with those tools installed.
