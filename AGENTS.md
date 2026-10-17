# Codex instructions — Wigglebites / Snake Arena

## Source of truth
This file and `docs/` describe the NEW approved direction. The former Kotlin/multiplayer instructions are obsolete; `CLAUDE.md` is retained only as a compatibility redirect to this file. Read all docs and visual references before implementation.

## Approved decisions
- Android portrait game built in Unity 6, C#, 2.5D top-down perspective.
- Offline-only MVP: 1 human + 19 local AI snakes, 180-second matches.
- Size/mass-based combat: attacker must have >= 1.10x defender mass; head touching another snake's head/body triggers elimination when eligible.
- Friendly Neon Arena visual direction, bright glowing food, crystals, rocks, foliage, colorful snakes and VFX.
- No server, Firebase, WebSockets, multiplayer, ads, store, or real-money purchases in MVP.
- No Kotlin/Jetpack Compose app architecture for this version.

## Engineering rules
- Keep deterministic, fixed-timestep gameplay simulation separate from Unity visuals and UI.
- Use configurable ScriptableObjects for tuning; avoid hard-coded balance values.
- Use object pooling for food and VFX; use spatial partitioning for nearby collision candidates.
- Build small, testable components and Unity EditMode tests for core gameplay rules.
- Target 60 FPS rendering and 30 Hz simulation on a defined reference Android device; profile before optimizing.
- Design touch targets for children; do not rely solely on text or color to communicate danger.
- Keep features and effects accessible, avoid excessive flashes, include sound toggles.
- Keep changes small, run applicable tests, and report what was and wasn't verified.
- Do not ask GitHub Copilot to review code or PRs.
- Do not silently reinterpret open questions. Document them and ask before major architecture changes.

## Workflow
1. Read `docs/GAME_DESIGN.md`, `docs/ARCHITECTURE.md`, `docs/VISUAL_DESIGN.md`, `docs/GAME_MECHANICS.md`, `docs/DEVELOPMENT_PLAN.md` and `design/`.
2. Inspect existing repository. Report conflicting legacy artifacts before removing or replacing anything.
3. Propose an incremental plan and implement the first playable vertical slice: arena, steerable snake, food, one bot.
4. Add the remaining AI, combat, effects, UI, persistence and Android packaging incrementally.
