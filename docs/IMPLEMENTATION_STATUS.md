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
- Runtime-generated Neon Arena floor grid, luminous walls/pylons, four bot palettes, multicolor bobbing food and friendly particle/audio feedback.
- Swedish-first HUD copy, visible safe-area-aware steering/turbo controls, plus persistent sound and reduced-effects controls available from menu and pause.
- EditMode tests for movement, steering, arena bounds, food collection, bot targeting and spawn lifecycle.

## Still required for MVP

- Complete physical-device profiling and release-candidate validation on the declared reference Android device.

The automated Unity/Android gate and API 35 x86_64 emulator smoke tests pass on the current equipped host. Emulator evidence does not replace the physical-device performance and thermal checks required for release approval.
