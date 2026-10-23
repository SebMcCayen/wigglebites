# Wigglebites — Snake Arena (Unity MVP specification)

This folder is a documentation overlay for the `SebMcCayen/wigglebites` repository. It defines the approved new direction for a child-friendly, offline Android snake arena game. **It is not a complete Unity project or executable game.**

Start with `AGENTS.md`; read all `docs/` and visual references under `design/`. `CLAUDE.md` is retained only as a compatibility redirect to the current instructions.

See `docs/DEVELOPMENT_PLAN.md` for implementation milestones.

## Open the current vertical slice

1. Install Unity `6000.0.23f1` with Android Build Support.
2. Open the repository root in Unity Hub.
3. Allow Package Manager and asset imports to finish.
4. Open `Assets/Wigglebites/Scenes/Arena.unity` and enter Play mode.

Choose **SPELA NU**, then drag on the left side of an Android screen to steer and hold the right side to boost. Arrow keys or WASD steer in the editor; hold Space to boost. The current build contains a 180-second offline match, live top-five standings, pause/results/replay flows, local best-mass and settings persistence, 19 personality-driven bots, safe respawns, 350 spatially indexed candy objects, head-only eliminations, body-and-tail severing, and a runtime-generated Neon Arena presentation. Sound and reduced-effects controls are available from the menu and pause screen.

Run EditMode tests from **Window > General > Test Runner**. See `docs/IMPLEMENTATION_STATUS.md` for implemented and remaining MVP scope.

For a command-line EditMode + Android build gate and the required device acceptance record, see `docs/ANDROID_RELEASE.md`.
