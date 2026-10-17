# Neon Arena — visual implementation brief

## Visual identity
A playful premium mobile arcade aesthetic: rounded glossy snake bodies, huge friendly eyes, saturated candy colors, deep navy-violet arena floor, luminous cyan/magenta highlights, layered environmental props and soft bloom. **Not** a gritty sci-fi game. Use `design/neon-arena-approved.png` as primary concept reference; reference art is directional, not pixel-perfect UI specification.

## Camera and scene
- Portrait 9:16 reference; angled orthographic top-down view (roughly 50–65 degrees from horizontal), camera tracks player with damping.
- Zoom adjusts gradually with snake size while preserving enough forward visibility; clamp so HUD stays legible.
- Hexagonal or subtly tiled floor in navy/slate; beveled tiles, selective emissive seams, ambient occlusion and soft contact shadows.
- Arena edge: low futuristic walls with neon trims, but keep gameplay collision shape simple and visible.
- Foreground and background prop layers provide parallax-like depth; avoid occluding playable snakes.

## Obstacles and set dressing
- Rocks: rounded faceted stone clusters, low/medium height, solid, readable collision footprints.
- Crystals: translucent stylized blue/purple clusters with emissive edges; solid bases, glow and slow sparkle particles.
- Shrubs: soft saturated green tufts, non-solid and subtly animated on proximity.
- Small neon pylons and floor decals add variation without gameplay ambiguity.
- Boost pad: circular electric-blue floor plate, speed boost when crossed (later-phase feature, not MVP).
- Teleport pad: swirling purple ring (future feature, concept only).
- Ensure no hard obstacle traps or visually hidden collision shapes.

## Snakes
- Rounded, segmented, flexible bodies with glossy materials and a visible directional head.
- Oversized animated eyes, eyebrows/eyelids, smile and expressive anticipation/celebration poses.
- MVP: blue player, orange-red, lime-green, violet and ice-blue opponents; distinct silhouettes/patterns in addition to colors.
- Suggested later skins: Classic, Fire, Toxic, Galaxy, Ice.
- Body length/thickness respond smoothly to mass; keep hitboxes predictable and avoid covering large screen areas.
- Player snake: bright cyan-blue edge lighting and subtle crown/marker only when appropriate; rivals retain readable outlines.

## VFX and feedback
- Food: small emissive orbs in cyan, pink, gold and green, with gentle bobbing and light sparkle.
- Pickup: tiny pop, scale pulse, short inward particle burst and pleasant gulp sound.
- Growth/level up: 0.5–0.8s golden ring, small crown burst, restrained camera punch and UI counter animation.
- Boost: cyan speed trail, animated body streaks and subtle field distortion (optional), no screen-obscuring particles.
- Elimination: friendly colorful cube/orb burst and a brief squash/pop, not gore; drop collectable food.
- Victory: golden spotlight, trophy/crown and modest confetti.
- Use URP bloom sparingly; cap particle counts; quality tiers and reduced-effects setting.

## UI
- Semi-transparent dark navy rounded panels with luminous trim; bright yellow primary action buttons.
- Timer centered top, leaderboard compact, mass and level readable, joystick bottom left, boost bottom right.
- Touch targets >=56dp equivalent where practical, scalable for device density and safe areas.
- Strong contrast, readable typography, icon+text rather than color-only semantics.
- Swedish-first strings in design; structure Unity localization-ready.

## Color direction (approximate, tune in-engine)
- Deep navy #101B39; floor slate #25385C; electric cyan #23DFFF; cobalt #167CFF; violet #9A48FF; candy pink #FF4EDB; sunny gold #FFD33D; lime #7CEB4D.

## Art handoff
- Concept images are mood boards; do not slice them into final production sprites without reviewing licensing, resolution and consistency.
- Create original production assets and materials from the references.

## Current runtime implementation
- Deep navy floor with an optional cyan emissive grid, luminous arena walls and corner pylons.
- Cyan player plus orange, lime, violet and ice-blue bot palettes; cyan, pink, gold and lime food.
- Food bobbing/rotation, pickup/elimination/results particle bursts and short generated feedback tones.
- Reduced-effects mode removes decorative grid/pylons, food motion and particles while retaining gameplay readability; sound is independently switchable.
- Swedish-first HUD labels are kept in the presentation layer so a future localization table can replace them without touching simulation code.
