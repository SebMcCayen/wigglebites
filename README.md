# Wigglebites — Snake Arena (Unity MVP specification)

This folder is a documentation overlay for the `SebMcCayen/wigglebites` repository. It defines the approved new direction for a child-friendly, offline Android snake arena game. **It is not a complete Unity project or executable game.**

Start with `AGENTS.md`; read all `docs/` and visual references under `design/`. `CLAUDE.md` is retained only as a compatibility redirect to the current instructions.

See `docs/DEVELOPMENT_PLAN.md` for implementation milestones.

## Open the current vertical slice

1. Install Unity `6000.0.23f1` with Android Build Support.
2. Open the repository root in Unity Hub.
3. Allow Package Manager and asset imports to finish.
4. Open `Assets/Wigglebites/Scenes/Arena.unity` and enter Play mode.

Drag on the left side of an Android screen to steer. Arrow keys or WASD provide editor controls. The current build contains a steerable player snake, one collector bot, food pickup and mass growth using runtime-generated placeholder visuals.

Run EditMode tests from **Window > General > Test Runner**. See `docs/IMPLEMENTATION_STATUS.md` for implemented and remaining MVP scope.
