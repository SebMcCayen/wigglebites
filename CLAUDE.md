# CLAUDE.md — Wigglebites

Play Store listing name: "Wigglebites – Snake Party for Kids"
Package name: `com.wigglebites.game` (adjust to your domain once registered)

Instructions for Claude Code. Read this whole file before writing any code.

## What we're building

A multiplayer snake battle game for young kids (ages ~5–10) on Android.
Core loop: join a 3-minute round, eat candy and other snakes, grow, place top 3, earn coins, buy fun cosmetics, play again.

## Non-negotiable rules for you (Claude Code)

1. **Kotlin + Jetpack Compose only.** No XML layouts, no Java.
2. **Material 3 Expressive** for all menus/UI. Airy, playful, modern 2026 feel: large rounded corners (28dp+), generous whitespace, big touch targets (min 56dp — kids have small clumsy fingers), springy motion, bold friendly typography.
3. **Always use the latest stable versions** via the Gradle version catalog (`gradle.toml` / `libs.versions.toml`). Never pin old versions. Before adding a dependency, check its latest stable release. No alpha/beta deps in main.
4. **Kids-first compliance (this is law, not preference):**
   - Must comply with Google Play **Families Policy**, COPPA and GDPR-K.
   - No open text chat. No free-text usernames shown to strangers (use generated fun names like "Wiggly Waffle").
   - No loot boxes / gambling-style random paid rewards.
   - No third-party ad SDKs unless certified for families (prefer: none at all).
   - Data collection: absolute minimum. No precise location, no contacts.
5. **Authoritative server.** The client never decides who ate whom — the server does. Clients send input, server sends state. This prevents cheating and desync.
6. Every PR/commit must build (`./gradlew assembleDebug`) and pass tests before you consider a task done.
7. Small commits, conventional commit messages (`feat:`, `fix:`, `refactor:`).

## Tech stack

| Layer | Choice | Why |
|---|---|---|
| Language | Kotlin (latest stable, K2 compiler) | Standard |
| UI | Jetpack Compose + Material 3 Expressive (latest Compose BOM) | Modern menus |
| Game rendering | Compose `Canvas` with a fixed-timestep game loop (60 Hz render, 20 Hz simulation tick from server) | Enough for a 2D snake game; skip game engines |
| DI | Koin (lightweight) | Simpler than Hilt for this size |
| Networking | Ktor client + WebSockets, kotlinx.serialization (protobuf or CBOR for game state, JSON for lobby/meta) | Low-latency realtime |
| Game server | Ktor server (Kotlin, JVM), Dockerized | Same language client+server = shared models module |
| Auth & meta backend | Firebase Auth (anonymous sign-in for kids) + Firestore for profiles, coins, inventory, leaderboards | Cheap, fast to ship |
| Matchmaking | Firestore queue + Cloud Function assigns players to a game-server room | Simple v1 |
| Local storage | DataStore (Preferences) | Settings, cached inventory |
| Animations | Compose animation APIs + Lottie (lottie-compose) for celebration/menu effects | Kid-friendly juice |
| Audio | Android `SoundPool` for SFX, `MediaPlayer` for music | Standard |
| Testing | JUnit5, Turbine, Compose UI tests, server: Ktor test host | |
| CI | GitHub Actions: build + test on every push | |

## Repo structure (monorepo)

```
/app            Android app (Compose)
/server         Ktor authoritative game server
/shared         Kotlin Multiplatform module: game rules, physics, scoring math,
                network protocol models — used by BOTH app and server
/docs           Design docs, this spec
.github/workflows/ci.yml
```

Sharing the simulation code in `/shared` is critical: the client runs the same sim for prediction, the server runs it for truth.

## Game design spec

### Round flow
1. **Lobby (30 s countdown).** Shows joined players as bouncing snake avatars. If fewer than 8 players when timer hits 0, fill remaining slots with bots (bots get silly names + a small robot badge — kids should be able to tell).
2. **Round: 3 minutes.** Big friendly countdown clock. Last 10 seconds: screen edge pulses + "hurry up" music.
3. **Results screen.** Podium animation for places 1–3 with confetti (Lottie), scores counting up with a tick sound, then coin reward reveal.

### Map themes (pick per round, random)
10 themes, each = background art, candy sprite set, obstacle style, music track:
Jungle, City Street, Forest, Sky/Clouds, Ocean, Candy Land, Space, Desert, Snow/Ice, Volcano.
Implement themes as data (a `MapTheme` object with asset refs) — adding a theme must never require code changes.

### Core mechanics
- All snakes start small (length 10 segments).
- **Candy** spawns at random free positions, ~1 candy per 2 s per active player, capped. Eating candy: +1 segment.
- **Eating another snake:** if your head touches another snake's body at segment *k*, that snake loses everything from segment *k* to its tail. The lost segments convert into **snake pieces** dropped on the map.
- **Snake pieces** are worth more than candy: +3 segments each.
- Head-to-head collision: the shorter snake loses its tail half (dropped as pieces); equal length = both bounce back stunned 1 s. Nobody ever "dies" — kids hate dying. You just shrink and keep playing.
- Speed boost: hold-to-boost, costs 1 segment per second, minimum length 5 (can't boost yourself to nothing).

### Bots
- Server-side, run the same shared sim.
- 3 difficulty tiers; matchmaker picks tiers so a bot roughly matches the average human's skill.
- Behavior: seek nearest candy/pieces, avoid bigger heads, occasionally chase smaller snakes. Add randomness so they feel alive, and make them slightly bad on purpose — kids should win often.

## Scoring & progression math (implement exactly this in /shared)

**In-round score** (determines placement):
```
score = 10 × candy_eaten + 30 × pieces_eaten + 50 × bites_landed + max_length_reached
```

**Coins earned per round:**
```
placement_base: 1st=100, 2nd=60, 3rd=40, 4th=25, 5th–8th=15, others=10
streak_multiplier = 1 + 0.10 × min(win_streak, 5)        // caps at 1.5×
first_win_of_day_bonus = +50                              // resets daily, drives daily return
coins = round(placement_base × streak_multiplier) + floor(score / 50) + first_win_bonus
```
Why this shape: everyone always earns something (no rage-quits), streaks reward coming back, the daily bonus creates a habit loop, and skill (score) matters but placement matters more. This is "play more" psychology **without** loot boxes or pay-to-win — which would violate Families Policy anyway.

**XP & levels (cosmetic prestige only):**
```
xp = score / 10, rounded
level_threshold(n) = 100 × n^1.5   // gentle curve, level-ups feel frequent early
```
Level-ups unlock free starter cosmetics so new kids get rewards fast.

**Bots never earn the top spot artificially** — if a bot would place 1st, that's fine, but tune bot difficulty so humans win ~60% of podium places.

## In-game store (coins only, no real money in v1)

All items are cosmetic or short-lived fun — nothing pay-to-win-ish:
- **Skins:** patterns/colors (stripes, dots, rainbow, glow, per-theme skins like "lava snake").
- **Hats:** crown, propeller cap, pirate hat, party hat, viking helmet.
- **Trails:** sparkles, bubbles, rainbow, footprints (yes, footprints on a snake — kids will laugh).
- **Emotes:** tap-to-play faces/sounds (laugh, "wow", raspberry) visible to others.
- **Victory dances:** podium animations for the results screen.
- **Consumable boosters (single-round, cheap):** Turbo Start (+20% speed first 20 s), Candy Magnet (30 s pull radius), Bubble Shield (survive one bite), Ghost (3 s pass-through, once per round), Big Appetite (candy worth ×2 for 60 s).
- **Snake sounds:** custom eat/boost sound packs (burp pack, cartoon pack, robot pack).

Pricing: skins 200–800, hats 150–500, trails 300, emotes 100, dances 400, boosters 25–50. Tune so an average kid affords something small every 2–3 rounds.

## Menus & juice (kid-focused UX)

- Home screen: your snake idles on screen, follows your finger, blinks, yawns if you're idle.
- Buttons wobble/squash on press with spring physics; soft pop sounds on every tap.
- Eating: satisfying "gulp" + brief chubby-bulge animation traveling down the snake.
- Biting a snake: comic "CHOMP!" burst; being bitten: dizzy stars (funny, not scary).
- Results: confetti, drumroll, count-up numbers.
- Store: items previewed live on YOUR snake before buying.
- Text: minimal words, big icons — many players can't read yet. Every action must be understandable without reading.

## Milestones (work in this order, one PR each)

1. **M0 Scaffold:** repo structure, version catalog, CI, empty Compose app boots, Ktor server boots, shared module wired.
2. **M1 Offline core:** local single-player sim on Canvas — movement, candy, growth, bite/cut mechanic, 3-min timer, results screen. All rules in `/shared` with unit tests for the scoring math.
3. **M2 Server-authoritative:** move sim to server, WebSocket protocol, client prediction + interpolation, 2 phones on one LAN can play together.
4. **M3 Lobby & bots:** 30 s lobby, bot fill, matchmaking via Firebase.
5. **M4 Progression:** coins/XP math, Firestore profiles, anonymous auth.
6. **M5 Store & cosmetics:** inventory, equip system, first 10 items.
7. **M6 Themes & juice:** 3 themes shipped (Jungle, Candy Land, Space), Lottie effects, sounds. Remaining 7 themes are content drops.
8. **M7 Polish & compliance pass:** Families Policy checklist, privacy policy, Play Console pre-launch report.

Ask before making architecture decisions not covered here. When versions or APIs are uncertain, look them up — don't guess from memory.
