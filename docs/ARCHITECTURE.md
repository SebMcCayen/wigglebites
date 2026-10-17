# Unity 6 technical architecture

## Stack
- Unity 6 + C#, Android build, portrait orientation.
- 2.5D: gameplay on a 2D plane with 3D meshes, orthographic angled camera, URP lighting/materials.
- No networking, backend or Firebase in MVP.

## Layers
1. **Presentation:** Unity UI, input, camera, meshes, animations, particles, sound.
2. **Application:** match lifecycle, pause, score, spawn orchestration, save/load.
3. **Simulation:** snake movement/body trails, food, growth, collisions, spatial queries, AI.
4. **Persistence:** local versioned JSON under `Application.persistentDataPath` for best scores/settings.

## Implementation guidance
- Fixed simulation tick 30 Hz, rendering target 60 FPS with interpolation.
- Use path samples at fixed spatial intervals for body segment positions rather than a Rigidbody per segment.
- Use circle approximations and spatial hash for broad-phase collisions; deterministic ordering and one resolution per pair/tick.
- Pool food, particles, and frequently respawned visual objects.
- Use ScriptableObjects for game balance, bot profiles, environment palettes, snake skins and effects presets.
- Prefer minimal allocations in hot loops; profile on Android.
- Separate plain C# simulation code from MonoBehaviours to support EditMode tests.

## Suggested layout
```
Assets/
  Art/{Materials,Sprites,VFX}/
  Audio/
  Prefabs/{Snakes,Food,Obstacles,UI}/
  Scenes/{Boot,MainMenu,Arena}.unity
  Scripts/{Core,Simulation,AI,Rendering,UI,Persistence}/
  Config/
  Tests/{EditMode,PlayMode}/
```

## Core modules
GameManager, MatchTimer, SnakeSimulation, SnakeView, SnakeBodyPath, InputController, CollisionSystem, GrowthSystem, AISystem, FoodManager, SpawnSystem, CameraController, SaveManager.

## Tests
- Combat eligibility at 1.10 threshold, ties and simultaneous head collisions.
- Growth, food pickup, boost floor, body following, obstacle collision.
- AI state transitions and safe spawning.
- 180-second match termination and ranking; save/load versioning.
- Android profiling with 20 snakes and 350 food objects.
