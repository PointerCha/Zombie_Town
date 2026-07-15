# House 01 Reference For Building Auto Generation

Source scene: `Assets/Scenes/MainScene.unity`

This document records the actual `World/House 01` structure that the new
`___Test/TestScene` building generator should target. It is a reference only;
no generation code should depend on this file at runtime.

## Generation Target

The generator should produce a complete multi-floor house using configured
tiles and numeric layout data. The first target shape is `House 01` from
`MainScene`.

Core goals:

- Create Ground by numeric `width x height` data.
- Split visual walls into `Upper Wall` and `Under Wall`.
- Treat building outer walls and room walls as separate rule sets.
- Support non-fixed, randomized room placement.
- Generate stair modules with visual tiles, walkable area, rails, and endpoints.

## House 01 Hierarchy

```text
House 01
├─ Floor 01
│  └─ Floor 01 Grid
│     ├─ Floor Visual Tilemap
│     │  ├─ Ground
│     │  ├─ Upper Wall
│     │  └─ Under Wall
│     ├─ Room 01 Visual Tilemap
│     │  ├─ Ground
│     │  ├─ Upper Wall
│     │  └─ Under Wall
│     ├─ Room 02 Visual Tilemap
│     │  ├─ Ground
│     │  ├─ Upper Wall
│     │  └─ Under Wall
│     ├─ Floor Collider Tilemap
│     │  ├─ Grid 1p0x0p5
│     │  │  └─ Ground
│     │  └─ Grid 0p3x0p15
│     │     ├─ Upper Wall
│     │     └─ Under Wall
│     ├─ Room 01 Collider Tilemap
│     │  ├─ Grid 1p0x0p5
│     │  │  └─ Ground
│     │  └─ Grid 0p3x0p15
│     │     └─ Under Wall
│     └─ Room 02 Collider Tilemap
│        ├─ Grid 1p0x0p5
│        │  └─ Ground
│        └─ Grid 0p3x0p15
│           ├─ Upper Wall
│           └─ Under Wall
│
├─ Floor 02
│  └─ Floor 02 Grid
│     └─ Same visual/collider layer pattern as Floor 01
│
├─ Stair
│  └─ Stair 01to02
│     ├─ 1p0x0p5 Grid
│     │  └─ Visual Tilemap
│     └─ 0p2x0p1 Grid
│        ├─ Walkable Trigger Area Tilemap
│        ├─ Left Rail Collider Tilemap
│        ├─ Right Rail Collider Tilemap
│        ├─ Floor Entry Trigger Tilemap
│        └─ Floor Exit Trigger Tilemap
│
├─ Ceiling
│  └─ Ceiling Grid
│     └─ Visual Tilemap
│
└─ Back Occlusion Zone
   └─ Back Occlusion Zone Grid
      └─ Collider Tilemap
```

## Numeric Tilemap Bounds

These values are read from the actual serialized Tilemap data.

### Outer Floor

| Layer | Origin | Size | Tile refs |
| --- | --- | --- | ---: |
| Floor 01 Ground Visual | `(-26, -9, 0)` | `(60, 42, 1)` | 2520 |
| Floor 01 Ground Collider | `(-26, -9, 0)` | `(60, 42, 1)` | 2520 |
| Floor 01 Upper Wall Visual | `(-26, -9, 0)` | `(60, 42, 1)` | 101 |
| Floor 01 Under Wall Visual | `(-26, -9, 0)` | `(60, 42, 1)` | 96 |
| Floor 02 Ground Visual | `(-23, -6, 0)` | `(60, 42, 1)` | 2485 |
| Floor 02 Ground Collider | `(-23, -6, 0)` | `(60, 42, 1)` | 2520 |
| Floor 02 Upper Wall Visual | `(-23, -6, 0)` | `(60, 42, 1)` | 101 |
| Floor 02 Under Wall Visual | `(-23, -6, 0)` | `(60, 41, 1)` | 100 |

Important implication:

- The first generator target should use `60 x 42` as the default building
  footprint.
- Floor 02 has fewer visual ground tiles than its collider grid because the
  stair opening removes part of the visible floor.

### Rooms

| Layer | Origin | Size | Tile refs |
| --- | --- | --- | ---: |
| Floor 01 Room 01 Ground Visual | `(0, -9, 0)` | `(34, 42, 1)` | 306 |
| Floor 01 Room 01 Ground Collider | `(0, 0, 0)` | `(34, 33, 1)` | 306 |
| Floor 01 Room 02 Ground Visual | `(0, -8, 0)` | `(34, 22, 1)` | 396 |
| Floor 01 Room 02 Ground Collider | `(0, -8, 0)` | `(34, 22, 1)` | 396 |
| Floor 02 Room 01 Ground Visual | `(0, 0, 0)` | `(37, 36, 1)` | 256 |
| Floor 02 Room 01 Ground Collider | `(0, 0, 0)` | `(37, 36, 1)` | 256 |
| Floor 02 Room 02 Ground Visual | `(0, -5, 0)` | `(37, 41, 1)` | 368 |
| Floor 02 Room 02 Ground Collider | `(0, -6, 0)` | `(37, 24, 1)` | 384 |

Important implication:

- Room visual bounds and room collider bounds are not always identical.
- Room wall generation cannot blindly reuse outer-floor wall logic.
- Rooms should be generated from room data, then rendered separately to
  `Room Visual Tilemap` and `Room Collider Tilemap`.

### Stair

| Layer | Origin | Size | Tile refs |
| --- | --- | --- | ---: |
| Stair Visual | `(-13, 0, 0)` | `(13, 31, 1)` | 12 |
| Walkable Trigger Area | `(-65, 0, 0)` | `(65, 163, 1)` | 786 |
| Left Rail Collider | `(0, 0, 0)` | `(86, 223, 1)` | 27 |
| Right Rail Collider | `(0, 0, 0)` | `(73, 196, 1)` | 27 |
| Floor Entry Trigger | `(-66, 0, 0)` | `(66, 147, 1)` | 22 |
| Floor Exit Trigger | `(-33, 0, 0)` | `(33, 163, 1)` | 21 |

Important implication:

- Stairs must be a module, not just a visual tile.
- A stair module requires visual tiles, walkable trigger area, rail colliders,
  floor entry trigger, and floor exit trigger.
- Stair colliders use a finer grid than normal floor colliders.

## Grid Standards

| Purpose | Cell size | Cell layout |
| --- | --- | --- |
| Visual floor, room, ceiling, stair visual | `(1, 0.5, 1)` | Isometric |
| Floor/room wall colliders | `(0.3, 0.15, 1)` | Isometric |
| Stair walkable/rail/endpoint colliders | `(0.2, 0.1, 1)` | Isometric |

Generation rule:

- Use `1 x 0.5` grids for visual tile placement and broad floor triggers.
- Use smaller grids for wall and stair collision precision.
- Do not generate per-tile GameObjects; use Tilemaps and CompositeCollider2D.

## Sorting Order Reference

Observed sorting pattern:

| Layer | Sorting order examples |
| --- | --- |
| Floor 01 Ground | `0` |
| Floor 01 Room 01 Ground | `1` |
| Floor 01 Outer Upper Wall | `5` |
| Floor 01 Room 01 Upper Wall | `10` |
| Floor 01 Room 01 Under Wall | `15` |
| Floor 01 Room 02 Ground | `20` |
| Floor 01 Room 02 Upper Wall | `25` |
| Floor 01 Room 02 Under Wall | `30` |
| Floor 02 Ground | `35` |
| Floor 02 Outer Upper Wall | `40` |
| Floor 02 Room 01 Ground | `45` |
| Floor 02 Room 01 Upper Wall | `50` |
| Floor 02 Room 01 Under Wall | `55` |
| Floor 02 Room 02 Ground | `60` |
| Floor 02 Room 02 Upper/Under Wall | `65` |
| Outer Under Wall | `100` |
| Ceiling | `105` |

Generation rule:

- Sorting order should be centralized in one profile or constants class.
- Avoid hard-coded magic numbers in the renderer.

## Tile Sets Observed

### Outer Building Walls

- `Assets/Tile/02_Wall/01/_Tile/Wall E1_E.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E1_N.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E1_S.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E1_W.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E2_E.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E2_N.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E2_S.asset`
- `Assets/Tile/02_Wall/01/_Tile/Wall E2_W.asset`

### Room Walls

- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A1_E.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A1_N.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A1_S.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A1_W.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A2_E.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A2_N.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A2_S.asset`
- `Assets/Tile/03_Room Wall/01/_Tile/Wallpaper A2_W.asset`

### Stair

- `Assets/Tile/05_Stair/02/_Tile/Stairs A1_S.asset`

## Collider/Trigger Rules

Observed pattern:

- Floor Ground collider is used as a floor occupancy/visibility trigger.
- Room Ground collider is used as a room occupancy/visibility trigger.
- Wall colliders are not triggers.
- Stair walkable area is a trigger.
- Stair rails are non-trigger colliders.
- Stair entry/exit endpoints are triggers.
- Back occlusion zone is a trigger.

Generation rule:

- Visual tilemaps and gameplay tilemaps must remain separate.
- Generated colliders should use `TilemapCollider2D + Rigidbody2D(Static) +
  CompositeCollider2D`.
- Trigger/non-trigger state must be configured by layer purpose, not manually
  per generated object.

## First Implementation Target

The first generator milestone should create one `House 01`-like building in
`Assets/___Test/TestScene.unity` with:

1. `60 x 42` outer ground footprint.
2. Two floors.
3. Outer `Upper Wall` and `Under Wall`.
4. At least two generated rooms per floor.
5. Room walls using room-wall tiles, not outer-wall tiles.
6. A stair module connecting Floor 01 and Floor 02.
7. Separate visual, collider, trigger, ceiling, and visibility-ready hierarchy.

## Confirmation Checklist For Step 1

Confirm that these reference assumptions are acceptable before Step 2:

- Default outer building footprint starts at `60 x 42`.
- The new generator should imitate the House 01 hierarchy, not reuse MainScene
  objects directly.
- Outer walls and room walls must use separate tile palettes/rules.
- Rooms may be randomized, but generated results must remain valid and
  reachable.
- Stair generation should be isolated as its own module.
- Test work should continue inside `Assets/___Test`.
