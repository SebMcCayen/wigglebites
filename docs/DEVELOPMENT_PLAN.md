# Implementation plan and MVP acceptance criteria

## Milestones (estimated 19–30 effective development days for a Unity-experienced developer)
1. **Core vertical slice (3–5d):** Unity project setup, arena, touch steering, single snake, camera and one AI.
2. **Gameplay (3–5d):** food, mass, growth, collision rules, obstacles, respawns and tests.
3. **AI (4–6d):** 19 bots, four personalities, safe spawning, tuning and profiling.
4. **Visual polish (5–8d):** 2.5D Neon Arena meshes/materials, UI, VFX, audio, reduced-effects option.
5. **Android release candidate (4–6d):** local saves, 180s match, results, input polish, Android build and performance testing.

## Definition of done
- Start match from menu quickly (target <=5s after menu appears on reference device).
- 1 player + 19 bots + 350 food objects work without noticeable hitching.
- All combat obeys mass threshold; no inconsistent double eliminations.
- Offline play, pause/resume, safe areas and touch controls work.
- Best mass and settings persist across app restarts.
- Stable 60 FPS target on a declared Android reference device; actual measured FPS documented.
- All simulation unit tests pass; Android build succeeds.

## Explicitly deferred
Multiplayer, online leaderboard, Firebase, accounts, shop, ads, portals, advanced power-ups, multiple map themes and cosmetic progression.

## First Codex task
Inspect the repo and read the compatibility redirect in `CLAUDE.md`; propose the minimal Unity 6 project scaffold and implement only the first playable vertical slice after user approval. Do not assume Unity editor/Android SDK are installed; report tool limitations honestly.
