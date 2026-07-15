# Architecture Rule Audit

Stage: 26A
Date: 2026-07-09
Status: Complete

## Purpose

Audit the current generated building system before city generation work begins.

The main question:

- Is the generator already generic enough for city-scale building placement?
- Or is it still too tightly shaped around the current House01-like reference?

Conclusion:

- The system is not locked to one TestScene result, because profiles, palettes,
  stair module data, runtime metadata, validation, and instance data exist.
- It is still a House01-style generator, because several generation, scene, and
  runtime binding paths assume rectangular buildings, two named floors, one
  main entrance, and one stair from Floor 01 to Floor 02.
- City generation should wait until Stages 26B through 26F remove the highest
  risk assumptions.

## Rule Classification

### Generic Rules

These are safe to keep as code-level rules for now:

- Generated data is built before Tilemaps are painted.
- Visual Tilemaps and Collider/Trigger Tilemaps are separated.
- Tilemap + CompositeCollider2D is preferred over per-tile GameObjects.
- Generated colliders/triggers receive `GeneratedRuntimeIdentity`.
- Generated floor/room/stair/ceiling visibility targets use
  `GeneratedVisibilityGroup`.
- Generated layouts must pass validation before city use.
- Seed reproducibility is required.
- City-facing placement data should come from generated data/instance data, not
  from scene hierarchy traversal.

### Profile Data

These are already controlled by `GeneratedBuildingProfile`:

- `buildingId`.
- `displayName`.
- `tilePalette`.
- `footprintOrigin`.
- `footprintSize`.
- `floorCount`.
- `floorVisualOffsetStep`.
- `minRoomsPerFloor`.
- `maxRoomsPerFloor`.
- `minRoomSize`.
- `maxRoomSize`.
- `roomPaddingFromOuterWall`.
- `minimumRoomGap`.
- `defaultEntranceSide`.
- `defaultEntranceFloorIndex`.
- `generateStairs`.
- `defaultStairVisualBounds`.
- `defaultUpperFloorHoleBounds`.
- `gridProfile`.
- `sortingProfile`.

Ceiling alignment is tile-set metadata, not a building-shape rule. It is stored
once as `GeneratedBuildingTilePalette.ceilingCellOffset` so every building that
uses the same wall/ceiling tiles receives the same alignment.

Existing profile assets:

- `House01LikeProfile.asset`.
- `SmallHouseProfile.asset`.
- `LargeHouseProfile.asset`.

### Tile Palette Data

These are already controlled by `GeneratedBuildingTilePalette`:

- Ground tile.
- Ceiling tile.
- Collision tile.
- Outer wall primary/secondary directional tile sets.
- Room wall primary/secondary directional tile sets.
- Door directional tile set.
- Stair visual tile.
- Default prop tile.

### Stair Module Data

These are already data-backed through `GeneratedStairModulePreset`, but the
preset is currently embedded inside the building profile instead of being a
standalone reusable asset:

- Stair visual row offsets.
- Walkable fine-grid min/max offsets.
- Entry trigger row range.
- Exit trigger row range.
- Exit trigger X offset.
- Rail rotation.
- Rail reference positions.
- Rail length.
- Fine grid scale.
- Fallback visual bounds.
- Fallback upper-floor hole bounds.
- Max reasonable stair area.

## House01-Like Assumptions Found

### Floor Binding Assumptions

Problem:

- `GeneratedBuildingSceneRoot` stores fixed `floor01Root` and `floor02Root`.
- `GeneratedGroundGenerator` stores fixed Floor 01/Floor 02 ground and wall
  target fields.
- Several renderers and binders call `buildingData.GetFloor(1)` and
  `buildingData.GetFloor(2)` directly.

Impact:

- 1-floor and 2-floor profiles can work, but 3-floor or dynamic-floor profiles
  require code changes.
- Generated scene binding is not fully data-driven.

Files:

- `Assets/___Test/Scripts/Scene/GeneratedBuildingSceneRoot.cs`.
- `Assets/___Test/Scripts/Generation/GeneratedGroundGenerator.cs`.
- `Assets/___Test/Scripts/Rendering/GeneratedColliderRenderer.cs`.
- `Assets/___Test/Scripts/Scene/GeneratedVisibilityBinder.cs`.
- `Assets/___Test/Scripts/Scene/GeneratedRuntimeBinder.cs`.

Next stage:

- Stage 26B.

### Stair Assumptions

Problem:

- `GeneratedStairPlanner` creates one stair between Floor 01 and Floor 02.
- Stair id is fixed as `stair_01_to_02`.
- `GeneratedStairRenderer` paints stairs from lower Floor 01 data.
- Runtime validation still has checks that assume a lower/upper pair for the
  current stair module.

Impact:

- One stair module works for the House01-like two-floor case.
- Multiple stairs, stairs between Floor 02 and Floor 03, different stair
  directions, or different stair module shapes are not yet general.

Files:

- `Assets/___Test/Scripts/Generation/GeneratedStairPlanner.cs`.
- `Assets/___Test/Scripts/Rendering/GeneratedStairRenderer.cs`.
- `Assets/___Test/Scripts/Data/GeneratedStairModulePreset.cs`.
- `Assets/___Test/Scripts/Validation/GeneratedBuildingValidator.cs`.

Next stages:

- Stage 26B for floor loop support.
- Stage 26C for reusable stair preset assets.

### Room Layout Assumptions

Problem:

- `GeneratedRoomPlanner` uses simple X-axis sector splitting.
- Room shapes are rectangular.
- Room doors are generated on one side with a fixed 2-cell opening around the
  center.
- Room type is currently set to `GeneratedRoomType.Custom` for generated rooms.

Impact:

- The generator has seed variation but produces a narrow family of layouts.
- It is good enough for validation, not yet enough for varied houses or city
  building families.

Files:

- `Assets/___Test/Scripts/Generation/GeneratedRoomPlanner.cs`.
- `Assets/___Test/Scripts/Data/GeneratedRoomData.cs`.
- `Assets/___Test/Scripts/Data/GeneratedDoorData.cs`.

Next stages:

- Stage 26C for room/door strategy presets.
- Stage 26D for room layout strategy upgrade.
- Stage 26E for door placement rules.

### Entrance Assumptions

Problem:

- The generator creates one main entrance.
- The id is fixed as `main_entrance_01`.
- Entrance width is currently hard-coded as 2 cells.
- Entrance position is centered on the chosen side.
- `GeneratedBuildingInstanceData.PrimaryRoadConnectionCell` assumes a single
  primary entrance.

Impact:

- This is enough to connect one house to one road.
- It is not enough for corner buildings, shops with multiple entrances,
  warehouses, apartment blocks, or building-specific road frontage rules.

Files:

- `Assets/___Test/Scripts/Generation/GeneratedGroundGenerator.cs`.
- `Assets/___Test/Scripts/Data/GeneratedEntranceData.cs`.
- `Assets/___Test/Scripts/Data/GeneratedBuildingInstanceData.cs`.

Next stage:

- Stage 26E.

### Footprint Assumptions

Problem:

- Generated buildings use one rectangular `RectInt` footprint.
- Outer walls are generated from rectangle bounds.
- Ceiling footprint is generated from rectangular footprint plus offset.

Impact:

- Rectangular houses are fine.
- L-shaped, U-shaped, complex stores, attached buildings, and irregular ruins
  are not supported yet.

Files:

- `Assets/___Test/Scripts/Data/GeneratedBuildingProfile.cs`.
- `Assets/___Test/Scripts/Generation/GeneratedGroundGenerator.cs`.
- `Assets/___Test/Scripts/Rendering/GeneratedCeilingRenderer.cs`.

Next stage:

- Not required before first city prototype.
- Defer until after Stage 27 unless irregular buildings become a near-term
  requirement.

### Scene Hierarchy Assumptions

Problem:

- Many systems still find children by generated names such as
  `Floor 01 Visual Tilemaps`, `Floor 02 Room Tilemaps`, and
  `Room 01 Visual Tilemap`.

Impact:

- The generated naming is stable enough for the current prototype.
- It is brittle if city generation starts instantiating many generated houses or
  if floors become dynamically counted.

Files:

- `Assets/___Test/Scripts/Generation/GeneratedGroundGenerator.cs`.
- `Assets/___Test/Scripts/Rendering/GeneratedColliderRenderer.cs`.
- `Assets/___Test/Scripts/Scene/GeneratedVisibilityBinder.cs`.
- `Assets/___Test/Scripts/Scene/GeneratedRuntimeBinder.cs`.

Next stage:

- Stage 26B.

## Priority Extraction List

Do first:

1. Replace fixed Floor 01/Floor 02 scene references with floor-index based
   generated floor roots.
2. Make all floor rendering/collider/visibility/runtime binding loops consume
   `buildingData.Floors`.
3. Make stair rendering consume all stairs from all floors without assuming
   Floor 01 is the only lower floor.
4. Preserve current House01-like, SmallHouse, and LargeHouse behavior after the
   floor binding change.

Do next:

1. Convert `GeneratedStairModulePreset` into a reusable ScriptableObject asset
   or add a ScriptableObject wrapper while preserving existing profile data.
2. Add room layout strategy data.
3. Add door placement strategy data.
4. Add entrance placement strategy data.

Defer:

1. Irregular building footprints.
2. Multiple external entrances.
3. Multiple stair modules in one building.
4. Multi-building city road placement.

## Stage 26A Result

Stage 26A is complete.

The correct next step is Stage 26B:

- Dynamic Floor Scene Binding.

Stage 26B should make the generator loop over generated floors and stop relying
on fixed Floor01/Floor02 fields where possible.
