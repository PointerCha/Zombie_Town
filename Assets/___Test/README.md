# Building Generation Test Workspace

This folder is isolated for the new building auto-generation work.

Main production scripts under `Assets/__Scripts` should not be modified for this
prototype unless explicitly requested. The current target scene is:

- `Assets/___Test/TestScene.unity`

Primary reference:

- `Assets/___Test/House01Reference.md`

Active roadmap:

- `Assets/___Test/Docs/BuildingGeneralizationPlan.md`

Progress and earlier roadmap history:

- `Assets/___Test/Docs/StageProgress.md`
- `Assets/___Test/Docs/RoadmapReset.md`

## Folder Layout

```text
Assets/___Test
├─ Data
│  ├─ Palettes
│  ├─ Profiles
│  └─ Presets
├─ Debug
├─ Docs
├─ Generated
├─ Scripts
│  ├─ Data
│  ├─ Editor
│  ├─ Generation
│  ├─ Rendering
│  ├─ Scene
│  └─ Validation
├─ House01Reference.md
└─ TestScene.unity
```

## Responsibility Split

- `Scripts/Data`: serializable runtime data models such as floor, room, wall,
  stair, and generated building data.
- `Scripts/Generation`: layout generation and rule selection.
- `Scripts/Rendering`: converting generated data into Tilemaps.
- `Scripts/Scene`: creating scene hierarchy and Unity components.
- `Scripts/Validation`: checking generated layouts before rendering.
- `Scripts/Editor`: editor-only tools and buttons.
- `Data/Palettes`: ScriptableObjects that reference TileBase assets.
- `Data/Profiles`: generation profiles such as House01-like dimensions and
  sorting orders.
- `Data/Presets`: reusable high-level building presets.
- `Generated`: optional generated assets or captured test outputs.
- `Debug`: debug notes, captures, or temporary inspection files.
- `Docs`: planning and implementation notes.

## Current Rule

Each step should be completed and confirmed before moving to the next stage.
Do not mix later-stage generator logic into earlier-stage scaffolding.

## Current Priority

The generator is a functional building prototype, not yet a city-ready runtime
system. The active sequence is:

1. Stage 26H - Building-family matrix reconfirmed. Complete (`4/4` passed).
2. Stage 27A - Automated regression foundation complete; the suite now reports
   `28/28` after the Stage 27C-A additions.
3. Stage 27B - Responsibility split implementation complete.
4. Stage 27C-A - Unique building-instance ownership and per-building runtime
   registry complete.
5. Stage 27C-B - Registry-based Unit floor/visibility implementation complete.
6. Stage 27C-C - Multi-building, multi-Unit Play Mode isolation complete
   (`1/1` passed; run with `F6`).
7. Stage 27D-A/27D-B - Batch Tilemap rendering, guarantee regeneration cleanup,
   classify data lifetime, and measure CPU/memory budgets.
8. Stage 27E-A/27E-B - Run the automated gameplay matrix and final visual
   building-stability gate.
9. Stage 28A-28C - Expand building grammar, authoring/catalog support, and
   generation/save lifetime policy.
10. Stage 29A-29D - City contracts, placement, roads/districts, rendering,
   streaming, and city validation.
11. Stage 30 - Navigation, NPC/zombie systems, persistence, and promotion from
   the test workspace.

## Current Building Profiles

- `Data/Profiles/House01LikeProfile.asset`: two-floor House 01-like reference
  profile.
- `Data/Profiles/SmallHouseProfile.asset`: one-floor small house profile with
  no stair generation.
- `Data/Profiles/LargeHouseProfile.asset`: larger two-floor profile with more
  rooms and the House 01-like stair module.
- `Data/Profiles/WideHouseProfile.asset`: one-floor wide variant used by the
  building-family validation matrix.

To test a profile, assign it to `Building Generator > GeneratedBuildingSceneRoot`
in `TestScene`, then run `GeneratedGroundGenerator > Generate Building With
Stairs`. The same rendering and validation pipeline should be used for every
profile.

Use `Docs/BuildingGeneralizationPlan.md` for completion criteria. City rendering
should not start until generated buildings pass the Stage 27 stability gate and
city-facing contracts are defined.
