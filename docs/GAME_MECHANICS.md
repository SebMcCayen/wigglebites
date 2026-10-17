# Mechanics and balancing (initial test values)

## Core rules
- Snake has `mass`, derived `level`, head position, heading, speed and a body path.
- Start mass 10; 350 active food objects; food yields 1–3 mass.
- Elimination: if attacker head contacts opponent head/body and `attacker.mass >= defender.mass * 1.10`, defender is eliminated. For mutual head contact, resolve once from the same simulation snapshot; if neither qualifies, both are separated/deflected with a brief cooldown.
- On elimination, part of defender mass is converted into collectable food; proposed fraction 50%, adjustable. No infinite collision loops; eliminated snakes removed before next simulation step.
- Rocks and solid crystal bases block movement and cause sliding/deflection, not instant death. Decorative foliage is non-solid. Arena boundary is solid.
- Boost costs 2 mass/sec while held and above minimum mass.

## Level thresholds (prototype)
- 1: 0–19; 2: 20–49; 3: 50–89; 4: 90–139; 5: 140–199; 6+: 200 onward, using configurable continuing thresholds.
- Level is UI feedback; collision eligibility uses actual mass only.

## Arena
- 100x100 world units, top-down plane with visual depth.
- Spawn snakes and food in free, navigable locations; keep safe spawn radius from larger snakes and solid obstacles.
- AI bots respawn with brief grace period in safe locations; grace must be visually communicated and must not allow attacking.

## AI
- 40% Collector; 30% Hunter; 20% Survivor; 10% Aggressive.
- States: Collect, Hunt, Flee, Wander, Recover. Use local perception and simple steering/obstacle avoidance.
- AI decisions 5–10 Hz; movement 30 Hz. Stagger updates.
- Avoid unfair omniscience: perception radius, reaction delay, variable accuracy.

## Open tuning questions
- Exact snake body segment spacing, growth curve, camera zoom curve, turn rate, collision radius, obstacle density and bot respawn delay must be tuned in playtests.
- All values are provisional; expose them as ScriptableObjects.
