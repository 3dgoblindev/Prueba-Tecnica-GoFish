# GoFish — Technical Test Prototype

A Unity 2D fishing game prototype based on [Go Fish! by Kwalee](https://play.google.com/store/apps/details?id=com.kwalee.gofish1), built in two weeks as part of a Junior Game Developer technical test.

**Unity version:** 2022.3.62f2  
**Render pipeline:** URP 2D  
**Input:** Mouse (simulating touch)

---

## Project Structure

```
Assets/
├── Animations/        # Animator controllers and animation clips
├── Audio/
│   ├── Music/         # Background music
│   └── SFX/           # Sound effect clips
├── Prefabs/
│   ├── Fish/          # One prefab per fish species
│   └── Particles/     # Catch and splash VFX prefabs
├── ScriptableObjects/ # FishData assets (one per species)
├── Scripts/           # All gameplay C# scripts (see below)
├── Scenes/
│   └── MainScene.unity
└── Settings/          # URP renderer and pipeline assets
```

---

## Architecture

The project is built around a **decoupled event-driven architecture**. Systems communicate through `static C# events` rather than direct references, which keeps each system independently testable and avoids tight coupling between unrelated managers.

### Event Flow (main game loop)

```
PlayerController ──OnCastCompleted──► HookController
                                      FishSpawner
                                      WallGenerator
                                      FishingLineController
                                      CameraController
                                      StoreManager (hide)

HookController ──OnReturnToSurface──► PlayerController (trigger recoil anim)
                                      FishSpawner (despawn fish)
                                      WallGenerator (despawn walls)
                                      FishingLineController (hide line)
                                      CameraController (switch target)
                                      StoreManager (show)

HookController ──OnCatchReady──► CatchRewardPresenter (animate fish reward)
HookController ──OnDepthChanged──► DepthLabel (update UI)
HookController ──OnCatchCountChanged──► CatchLabel (update UI)
SavesManager ──OnCoinsChanged──► CoinsLabel (update UI)
                                 StoreManager (refresh button states)
StoreManager ──OnPurchaseSuccess/Error──► StoreSFX (play audio feedback)
```

---

## Core Systems

### PlayerController
Handles all player input and the cast animation state machine. Uses `GetMouseButtonDown/Up` to detect a charge-and-release mechanic with a subtle camera zoom-in on hold. Fires `OnCastCompleted` via an **Animation Event** (called at the peak of the throw animation), which acts as the global signal that kicks off all other systems. Also handles a freeze-frame effect on cast for juice.

### HookController
The central state machine of the game. Three states: `Idle → Descending → Ascending`. Manages the hook's movement physics (via `Rigidbody2D`), horizontal player control during descent, depth clamping, fish collection on ascent via `OnTriggerEnter2D`, and the reward pipeline. Exposes `RefreshStatsFromSave()` so the store can apply purchased upgrades mid-session without restarting. Also handles hook tilt/sway as a visual feedback of horizontal velocity.

### FishSpawner
Manages fish lifecycle using a **Dictionary-keyed object pool** (`Dictionary<FishData, Queue<FishController>>`). On each cast, spawns a batch of fish scaled to the current max depth, filtering eligible species by their `minDepth`/`maxDepth` range defined in their `FishData` ScriptableObject. Fish are recycled back to the pool on surface return rather than destroyed.

### FishController
Handles individual fish behaviour: horizontal swimming with direction flipping on boundary triggers, a turn cooldown to prevent rapid oscillation, and the catch sequence (disabling physics, attaching to hook, playing VFX + audio). Uses `Rigidbody2D` for movement to allow future physics interactions.

### FishData (ScriptableObject)
Data container per fish species. Defines name, prefab reference, price, rarity, swim speed, spawn depth range, catch particles, and catch sound. Adding a new fish species requires only creating a new `FishData` asset and assigning its prefab, no code changes needed.

### CatchRewardPresenter
Drives the post-catch presentation sequence: each caught fish animates from the hook to center screen (scale up, straighten rotation), holds briefly, then flies toward the player (scale to zero). Runs fish sequentially with configurable stagger and escalating speed and pitch per fish for a satisfying rhythm. Spawns `CoinFlyEffect` per fish for the coin UI animation.

### WallGenerator
Pools and spawns stone objects along both side walls during each dive. Wall depth adjusts dynamically from the save data, so depth upgrades are reflected immediately on the next cast.

### AudioManager
Singleton with a small `AudioSource` pool (index 0 reserved for looping music, indices 1–N for SFX). Overloaded `PlaySFX` methods accept single clips or arrays (picks randomly) and support fixed or randomized pitch range. Handles all audio requests game-wide without any scene dependency.

### SavesManager
Persistent data layer using `JsonUtility` serialization to `Application.persistentDataPath`. Stores `coins`, `maxDepth`, and `maxCatch`. Fires `OnCoinsChanged` so all UI updates reactively without polling. Includes editor-only debug shortcuts (C = add 1000 coins, X = wipe save).

### StoreManager
Manages the upgrade shop UI, shown between casts. Calculates upgrade costs dynamically based on current stats (depth cost scales with current depth, capacity cost scales with current capacity). Fires `OnPurchaseSuccess/Error` events consumed by `StoreSFX` for audio feedback, keeping UI and audio fully decoupled.

### MiniTweenFeel
Lightweight tween utility (no external dependencies). Supports position, rotation and scale animation with `OneWay` or `PingPong` modes and five ease curves: Linear, EaseIn, EaseOut, EaseInOut, and PunchElastic. Used on the player cast and catch feedback moments.

### CameraController
Follows a target transform on the Y axis using `Vector3.SmoothDamp`. Switches target from the player (boat) to the hook on cast, and back on surface return. Runs in `LateUpdate` to avoid jitter after physics resolves.

### InfinitePan
Scrolls a material's UV offset over time to produce a looping water background. Instantiates its own material copy on `Start` and destroys it on `OnDestroy` to avoid modifying the shared asset.

---

## Production Log

Total tracked time: **~15 hours** across 9 working sessions over approximately 10 days.

| Session | Date | Area | Hours | Notes |
|---|---|---|---|---|
| Planning | May 28 AM | Prod | 1.0 h | Game breakdown, task list, asset review |
| Core loop | May 28 PM | Code | 2.0 h | PlayerController, HookController, basic cast/ascent cycle |
| Fish behaviour | May 29 AM | Code | 1.3 h | FishController, FishSpawner, pooling, depth-based spawn |
| Fish + loop polish | May 29 PM | Code | 1.5 h | Boundary turning, catch trigger, state cleanup |
| Loop + UI | Jun 1 PM | Code / UI | 1.5 h | StoreManager, DepthLabel, CoinsLabel, CatchLabel |
| Hook polish | Jun 1 PM | Code | 0.75 h | Hook tilt/sway, freeze frame, camera switch |
| Fish presentation | Jun 1–2 | Code / UI | 1.6 h | CatchRewardPresenter, CoinFlyEffect, stagger sequence |
| Walls + VFX | Jun 4 | Code / VFX | 1.0 h | WallGenerator pooling, particle prefabs, Splash effect |
| Audio + bugfix | Jun 5 PM | Audio / Code | 1.4 h | AudioManager pool, MusicManager, SFX wiring, minor fixes |
| Final polish | Jun 5–6 | VFX | 3 h | InfinitePan water, particle tuning, Hook freeze frame, minor fixes|

**Breakdown by discipline:**

| Area | Hours |
|---|---|
| Code | ~10.0 h |
| UI | ~1.0 h |
| VFX / Polish | ~2 h |
| Audio | ~0.5 h |
| Production / Planning | ~1 h |

The majority of time went into the core game loop (cast → descend → ascend → reward) and making the fish pooling and presentation feel solid. Audio and visual polish were compressed into the final sessions.

---

## Controls

| Input | Action |
|---|---|
| Click / hold | Charge cast |
| Release | Throw hook |
| Click + drag (during descent/ascent) | Steer hook horizontally |

> Designed for mouse input simulating touch. All interactions use `Input.GetMouseButton`.

---

## Known Limitations

- Fish capacity has no upper bound. The store allows unlimited capacity upgrades
- Fishing line renders as a straight segment (no parabolic curve)
- No first-run tutorial or onboarding
