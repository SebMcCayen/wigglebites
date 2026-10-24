# Mechanics and balancing (initial test values)

## Core rules
- Snake has `mass`, derived `level`, head position, heading, speed and a body path.
- Start mass 10; 350 active food objects; food yields 1–3 mass.
- Combat eligibility: an attacker must have `attacker.mass >= defender.mass * 1.10`. Ineligible contacts deflect with a brief cooldown.
- Head bite: if an eligible attacker head contacts defender segment 0, the defender is eliminated. Mutual head contacts resolve once from the same simulation snapshot.
- Body bite: if an eligible attacker head contacts any later segment, that segment and every segment behind it are severed into collectible candy. The defender remains alive with the head-side portion, and its mass/length are reduced proportionally.
- A body bite briefly deflects the attacker and cools down the survivor, preventing one overlapping contact from consuming several cuts in consecutive ticks. If several bites target one snake in the same snapshot, the closest-to-head bite wins with stable snake ID as the tie-breaker.
- On head elimination, the configured fraction of defender mass is converted into collectable candy. No infinite collision loops; eliminated snakes are removed before the next simulation step.
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
