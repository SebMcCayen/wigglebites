# Implementation status

## Current vertical slice

- Unity 6 project structure and Android portrait player settings.
- Deterministic 30 Hz simulation separated from MonoBehaviour presentation.
- Touch-drag and keyboard steering with capped turn rate.
- Hold-to-boost controls with configurable speed, mass drain and safe mass floor.
- Player snake, 19 local bots across four personalities, arena boundary, 350 food objects and mass growth.
- Deterministic 1.10x mass combat, friendly elimination and configurable food drops.
- Safe bot spawns, delayed respawns, visible non-combat grace periods and staggered 5–10 Hz AI decisions.
- Bounded 500-item food-state/visual pool with inactive-item reuse for elimination drops.
- 180-second match timer, deterministic ranking, player-death results, pause/resume and replay.
- Versioned local JSON persistence for best mass plus a functional runtime HUD/menu/results flow.
- Allocation-conscious spatial hash broad phase for food pickup and snake-body combat queries, with per-tick diagnostics.
- Runtime-generated placeholder arena and snake visuals.
- EditMode tests for movement, steering, arena bounds, food collection, bot targeting and spawn lifecycle.

## Still required for MVP

- Approved Neon Arena art direction, UI, VFX, audio and reduced-effects support.
- Android build, device profiling and release-candidate validation.

Unity editor, Android SDK and emulator validation must be run on a host with those tools installed.
