# Game Design Specification — Snake Arena MVP 0.1

## Pitch
A playful, fast, offline Android portrait-mode arena game for children. Steer a cute snake, collect glowing food, grow larger, and eat smaller rival snakes. Finish the 180-second round with the greatest mass.

## Session
- One human-controlled snake and 19 locally simulated AI opponents.
- Fixed 180-second round. At time expiry, highest mass wins; ties resolved by eliminations, then deterministic stable snake ID.
- If the player is eliminated, show results and a fast retry option. AI replacements can respawn after a short delay to maintain activity; the player does not auto-respawn in MVP.
- Main menu -> arena -> pause -> results -> replay. No lobby, internet or login.

## Controls
- Left-side virtual joystick controls heading; snake advances automatically.
- Right-side hold-to-boost button: base speed 5 units/s, boost speed 8 units/s, costs 2 mass/s, cannot reduce mass below a configurable safe minimum (proposed 10).
- Soft steering interpolation and capped turn rate; no instant 180-degree reversal.

## HUD
- Large timer centered top; compact mass/level and leaderboard (top five) near top corners.
- Bottom-left joystick, bottom-right boost button; preserve safe areas and avoid obscuring the snake.
- Results show rank, final mass, eliminations, best mass and replay.

## Kid-friendly requirements
- Expressive, non-threatening snakes with oversized eyes; no gore, blood or scary death screens.
- Short, clear feedback, oversized tap targets, icon-supported UI, pause and audio controls.
- Never require internet to play. No account, chat, ads or purchases in MVP.
- Prefer accessible effects; avoid rapid flashing, allow reduced effects and sound toggles.

## Not in MVP
Online multiplayer, backend, monetization, daily quests, skins shop, portals, complex power-ups and multiple arenas. They may be visually explored but not implemented without explicit approval.
