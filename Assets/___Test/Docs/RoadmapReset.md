# Building Auto Generation Roadmap Reset

Historical roadmap: retained for implementation history. The active plan is
`Assets/___Test/Docs/BuildingGeneralizationPlan.md` as of 2026-07-10.

Last reset: 2026-07-08

## Project Goal

Build a maintainable procedural building generation pipeline for the
`Assets/___Test/TestScene.unity` prototype first, then expand it into city
generation.

The long-term target is:

- Configure tiles, profiles, and presets.
- Generate valid house structures automatically.
- Generate floors, rooms, walls, doors, stairs, ceilings, colliders, triggers,
  and visibility metadata.
- Validate generated buildings before they are used.
- Reuse the generated building data later for city placement, road connection,
  and district rules.

`MainScene/House 01` remains the first visual and structural reference, but the
new generator must not depend on MainScene objects at runtime.

## Current State Summary

The project currently has a working first-pass House 01-like generator.

Completed or functionally started:

- Test workspace structure under `Assets/___Test`.
- House 01 reference documentation.
- Core generated data models.
- Tile palette and building profile assets.
- TestScene root hierarchy and scene binding.
- Numeric floor ground generation.
- Outer wall visual generation.
- Room layout generation.
- Room visual generation.
- Room door opening generation.
- Floor and room collider generation.
- Two-floor generation.
- Stair visual generation.
- Upper-floor stair hole generation.
- Stair walkable, entry, exit, and rail tilemap generation.
- Stair fine collider grid.
- Stair rail rotation and coordinate correction.
- Stair zone metadata for future floor transition logic.
- Multiple building profiles for House 01-like, small one-floor, and larger
  two-floor generation tests.

The code currently compiles with zero errors and zero warnings through:

```text
dotnet build Assembly-CSharp.csproj
```

## Current Known Problems

These issues should be resolved before moving into city-scale generation.

- The generator still targets one House 01-like structure rather than a fully
  generic building grammar.
- Stair generation now reads House 01-like module values from
  `GeneratedStairModulePreset` data stored on the building profile.
- Stair module, room layout, door placement, and entrance placement data are
  now split into reusable ScriptableObject preset assets and referenced by
  building profiles.
- Room placement is deterministic and varied enough for testing. Basic
  reachability validation is implemented, but useful room adjacency and natural
  circulation still need later refinement.
- Door placement is basic. Generated room doors are now checked for public-floor
  reachability. Door width and room corner visuals have been corrected against
  the saved Room 01 reference, but door placement still needs gameplay testing.
- External building entrance data is modeled for the House 01-like generator.
  Road alignment and entrance visual tile rules are still pending.
- Ceiling generation is implemented for the House 01-like profile.
- Visibility groups and player-driven transparency are connected, but floor,
  room, ceiling, and upper-floor alpha priorities need a focused regression
  pass before more generation rules are added.
- Generated data validation is implemented for basic structural and internal
  connectivity checks.
- Unit gameplay integration exists for TestScene, but generated colliders,
  triggers, floors, stairs, and visibility groups still need a profile matrix
  regression pass before the building generator can be considered
  gameplay-ready.
- Multiple building profiles now exist, but their architectural grammar is still
  profile-size variation rather than a full city-scale building family system.
- City placement data such as footprint, entrance direction, bounds, and road
  connection point is not finalized.

## Reset Stage Plan

Use the stages below from this point forward. Finish one stage, verify its
checklist, then proceed to the next.

## Reset Stage 16 - Ceiling Generation

Status: Complete

Goal:

- Generate a ceiling tilemap for the whole building footprint.
- Keep ceiling visual data separate from floor and room visual data.
- Prepare the ceiling object for later visibility control.

Work:

- Added `GeneratedCeilingRenderer`.
- Render `Ceiling/Ceiling Grid/Visual Tilemap` from the building footprint.
- Use the palette ceiling tile and centralized sorting profile.
- Keep generation inside `Assets/___Test`.
- Connected ceiling rendering to `Generate Building With Colliders` and
  `Generate Building With Stairs`.
- Use `GeneratedBuildingTilePalette.ceilingCellOffset` so ceiling alignment is
  defined once per tile set instead of being duplicated by every building
  profile. The House 01 palette uses `(3, 3)`, matching the corrected TestScene
  ceiling reference.
- Runtime validation compares the generated ceiling bounds with the expected
  footprint and reports an error when alignment drifts.

Confirm:

- `Generate Building With Stairs` or a new full-generation command creates a
  ceiling tilemap.
- Ceiling covers the building footprint naturally.
- Ceiling does not overwrite floor, room, or stair tilemaps.
- Code compiles without errors.

## Reset Stage 17 - Visibility Metadata Structure

Status: Complete

Goal:

- Generate visibility-ready metadata for floors, rooms, ceilings, and stair
  areas without implementing full player behavior yet.

Work:

- Added `GeneratedVisibilityGroupKind`.
- Added `GeneratedVisibilityGroup`.
- Added `GeneratedVisibilityBinder`.
- Bind floor outer walls, room walls, ceiling, and stair visual targets to
  generated visibility groups.
- Keep generated visibility metadata separate from production
  `Assets/__Scripts`.

Confirm:

- Floor, room, and ceiling visibility groups can be inspected in TestScene.
- Each group references the correct TilemapRenderer targets.
- No runtime player dependency is required yet.
- Code compiles without errors.

## Reset Stage 18 - Building Validation System

Status: Complete

Goal:

- Detect invalid generated buildings before relying on visual inspection.

Work:

- Added validation under `Assets/___Test/Scripts/Validation`.
- Check room overlap.
- Check room bounds inside floor footprint.
- Check door openings are not blocked by room wall data.
- Check stair has visual, walkable, entry, exit, and rail data.
- Check Floor 01 and Floor 02 stair linkage.
- Check required palette/profile references.
- Add a `GeneratedBuildingDataView > Validate Building Data` context menu.
- Automatically validate after full collider/stair generation paths.

Confirm:

- Validation logs clear success/failure results.
- Broken generation data can be reported with actionable messages.
- Valid current House 01-like generation passes validation.
- Code compiles without errors.

## Reset Stage 19 - Connectivity Validation

Status: Complete

Goal:

- Ensure generated buildings are actually traversable.

Work:

- Added grid-based reachability checks on generated logical data.
- Confirm rooms with doors are reachable from the floor public area.
- Confirm stairs are referenced by both source and destination floors.
- Confirm upper stair floor has stair hole cells.
- External entrance checks remain for a later stage.

Confirm:

- The current generated building passes reachability checks.
- If a door is removed or blocked, validation catches it.
- If a stair trigger is missing, validation catches it.
- Code compiles without errors.

## Reset Stage 20 - Seed And Regeneration Workflow

Status: Complete

Goal:

- Make deterministic generation convenient and testable.

Work:

- Added deterministic building fingerprints and hashes.
- Confirm same seed produces the same generated data.
- Confirm different seed can produce valid variation.
- Added `GeneratedGroundGenerator > Validate Seed Reproducibility`.
- Keep debug logs readable.

Confirm:

- Same seed creates identical room/stair/door data.
- Different seeds change room layout while preserving validity.
- Full generation + validation can be run in a predictable order.
- Code compiles without errors.

## Reset Stage 21 - Preset Data Extraction

Status: Complete

Goal:

- Move hard-coded House 01 rules toward reusable preset data.

Work:

- Added `GeneratedStairModulePreset`.
- Move stair visual mask rows, walkable mask offsets, entry/exit trigger rows,
  rail reference coordinates, rail rotation, fine grid scale, and fallback
  bounds into profile-owned stair module data.
- Keep room count, size, padding, floor count, and ceiling offset in
  `GeneratedBuildingProfile`.
- Keep tile references in `GeneratedBuildingTilePalette`.

Confirm:

- House 01-like generation still works.
- Stair generation no longer relies on unexplained House 01 constants inside
  planner code.
- Profile/preset values are inspectable in Unity.
- Code compiles without errors.

## Reset Stage 22 - External Entrance Data

Status: Complete

Goal:

- Prepare each generated building for city placement.

Work:

- Added `GeneratedEntranceKind`.
- Added `GeneratedEntranceData`.
- Generate one main entrance from the profile default entrance side/floor.
- Store entrance side, floor index, and cell on `GeneratedBuildingData`.
- Cut the matching outer wall cell so the entrance is not blocked by generated
  wall data.
- Add entrance data to deterministic fingerprints and summaries.
- Validate entrance floor ownership, bounds, wall opening, and public-floor
  reachability.

Confirm:

- Generated building data exposes entrance position and direction.
- The entrance is reachable from inside the building.
- The entrance data can later be aligned to a road.
- Code compiles without errors.

## Reset Stage 23 - Multiple Building Presets

Status: Complete

Goal:

- Prove the generator is not locked to one House 01 shape.

Work:

- Added `Assets/___Test/Data/Profiles/SmallHouseProfile.asset`.
- Added `Assets/___Test/Data/Profiles/LargeHouseProfile.asset`.
- Small profile varies footprint, room count, room size range, and disables
  stairs for a one-floor building.
- Large profile varies footprint, room count, room size range, ceiling offset,
  and keeps two-floor stair generation enabled.
- Added generated tilemap cleanup support so switching from a two-floor profile
  to a one-floor profile does not leave stale Floor 02 or extra room tilemaps.

Confirm:

- House 01-like, small house, and larger house presets can generate.
- Each preset passes validation.
- No preset requires hand-editing generated scene objects.
- Code compiles without errors.

## Reset Stage 24A - Generated Building Runtime Readiness

Status: Complete

Goal:

- Confirm generated buildings expose all runtime information needed by Unit
  gameplay without parsing object names.

Work:

- Audit generated Floor, Room, Stair, Entrance, Ceiling, Collider, Trigger, and
  Visibility objects.
- Ensure runtime identifiers are stored as components or generated data, not
  inferred from hierarchy strings.
- Confirm generated collider and trigger layers/tags are explicit enough for
  Unit systems.
- Identify which generated objects must be active/inactive per floor.
- Added `GeneratedRuntimeIdentity`.
- Added `GeneratedRuntimeBinder`.
- Bind runtime identities to generated building, floor, room, ceiling, stair,
  floor collider, room collider, and trigger objects.
- Added `GeneratedBuildingSceneRoot > Log Runtime Identity Summary`.

Confirm:

- A Unit system can discover the generated building, current floor data,
  visibility groups, stair zones, and trigger areas through stable components
  or data.
- No required gameplay behavior depends on manually edited scene objects.
- Code compiles without errors.

## Reset Stage 24B - Unit Integration Test

Status: Complete

Goal:

- Place a Unit in `TestScene` and verify generated houses are playable.

Work:

- Add or reuse a test Unit controller in `Assets/___Test` only.
- Test left-click selection and right-click movement if needed for the test
  scene.
- Verify the Unit can approach the main entrance, enter the building, move
  through room doors, and interact with generated floor triggers.
- Keep the test Unit isolated from production `Assets/__Scripts` unless
  explicitly requested.
- Added `GeneratedTestUnitController`.
- Added `GeneratedTestUnitGizmos`.
- Added `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- The generated test Unit uses Rigidbody2D movement, collision-aware movement
  casts, left-click selection, right-click movement, and trigger logging for
  `GeneratedRuntimeIdentity` trigger objects.
- The generated test Unit now creates a runtime fallback SpriteRenderer visual
  and logs input/movement-blocking events for debugging.
- Main entrance generation now cuts a 2-cell outer wall opening so regenerating
  the building does not recreate a blocking Under Wall collider at the entrance.
- Floor-specific collision filtering moved to Stage 24C.

Confirm:

- Unit movement works against generated floor and wall colliders.
- Door openings are physically passable.
- Ground triggers detect the Unit only when expected.
- Code compiles without errors.

## Reset Stage 24C - Floor Isolation

Status: Implementation Complete - Play Mode Confirmation Required

Goal:

- Ensure a Unit only collides with and triggers objects on its current floor.

Work:

- Define how generated floor index is stored on collider and trigger objects.
- Add Unit current-floor tracking for the test scene.
- Ensure Floor 01 Units do not collide with or trigger Floor 02 walls, rooms,
  room grounds, or stair exit zones.
- Ensure Floor 02 Units do not collide with or trigger Floor 01 room/wall
  colliders except where intentionally connected through stairs.
- Added `GeneratedFloorIsolationAgent`.
- `GeneratedTestUnitController` delegates floor filtering to
  `GeneratedFloorIsolationAgent`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` adds the floor
  isolation agent and initializes the test Unit to Floor 01.
- The agent uses `GeneratedRuntimeIdentity.FloorIndex` and
  `Physics2D.IgnoreCollision` to ignore generated collider/trigger objects from
  other floors.

Confirm:

- Moving under/near another floor does not cause collision, trigger, or
  visibility side effects from that floor.
- Floor isolation works for House01-like and LargeHouse profiles.
- Manually switching the test Unit floor index and refreshing isolation swaps
  which floor colliders are active.
- Code compiles without errors.

## Reset Stage 24D - Visibility Runtime Binding

Status: Implementation Complete - Play Mode Confirmation Required

Goal:

- Connect generated visibility metadata to actual player-driven transparency.

Work:

- Use generated `GeneratedVisibilityGroup` targets for room, floor, ceiling,
  and stair visibility.
- Fade room walls, floor walls, and ceilings based on Unit occupancy and
  current floor.
- Avoid per-frame renderer scans; cache target renderers from generated
  visibility groups.
- Keep alpha transitions centralized and reusable.
- Added `GeneratedVisibilityController`.
- Added `GeneratedVisibilityAgent`.
- `GeneratedVisibilityBinder` attaches/rebuilds the controller on
  `Generated House Root`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` adds the visibility
  agent to the test Unit.
- Floor ground trigger occupancy fades the current floor's outer walls and
  ceiling.
- Room ground trigger occupancy fades that room's wall tilemaps.
- Floor ground trigger occupancy hides floor visual tilemaps above the Unit's
  current floor.
- Stair floor changes refresh upper-floor visual hiding immediately.
- Stair floor changes keep the ceiling hidden until normal floor/room
  visibility registration takes over.
- Ceiling visibility respects the Unit's current-floor context, preventing thin
  stair endpoint/ground trigger exits from restoring the ceiling too early.
- Current-floor changes register the destination floor visibility group, so
  Floor 02 outer walls fade immediately after stair transition.
- Visibility trigger handling respects `GeneratedFloorIsolationAgent`.

Confirm:

- Entering a generated room hides/fades that room's blocking walls.
- Entering a generated building/floor hides/fades the correct ceiling or upper
  blocking visuals.
- Floors above the Unit's current floor do not visually block lower-floor
  testing.
- Other floors do not react to the Unit.
- Exiting the floor/room restores the corresponding visuals.
- Code compiles without errors.

## Reset Stage 24E - Stair Runtime Transition

Status: Implementation Complete - Play Mode Confirmation Required

Goal:

- Make generated stair zones perform reliable floor transitions.

Work:

- Use `GeneratedStairZone` metadata for walkable, entry, exit, and rail zones.
- Decide the exact transition rule for generated stairs.
- Update Unit current floor when entering the correct stair transition zone.
- Preserve stair rail collision so the Unit cannot leave the stair sides.
- Added `GeneratedStairTransitionAgent`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` adds the stair
  transition agent to the test Unit.
- Transition zones only switch floors when the Unit's current floor matches the
  zone's source floor.
- A stair locks after one transition until the Unit leaves its Walkable trigger
  area, preventing unwanted toggles at the opposite endpoint.
- Stair runtime transition now follows the MainScene endpoint model: touching
  the entry endpoint begins a transition, touching the target endpoint completes
  it, and touching the original endpoint returns to the source floor.
- Thin Walkable/Entry trigger exits no longer complete or revert the transition.

Confirm:

- Floor 01 to Floor 02 transition works.
- Floor 02 to Floor 01 transition works.
- Re-entering or crossing stair edges does not cause unwanted floor toggles.
- Current floor isolation updates immediately after transition.
- Code compiles without errors.

## Reset Stage 24E-1 - Test Camera Follow Support

Status: Complete

Goal:

- Make TestScene camera follow the generated test Unit for playability
  verification.

Work:

- Added `GeneratedTestCameraController`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` binds `Main Camera`
  to the generated test Unit.
- Camera follows with SmoothDamp and snaps on first target binding.

Confirm:

- Main Camera follows the generated test Unit smoothly and quickly.
- The camera support remains isolated under `Assets/___Test`.
- Code compiles without errors.

## Reset Stage 24F - Playability Validation

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Extend validation from structural correctness to gameplay readiness.

Work:

- Validate entrance-to-room and entrance-to-stair reachability.
- Validate door width and clearance.
- Validate stair entry/exit zones connect to valid floor walkable cells.
- Validate generated floor isolation metadata exists on required colliders and
  triggers.
- Add clear logs for gameplay-blocking generation failures.
- Added gameplay-readiness checks to `GeneratedBuildingValidator`.
- Added `GeneratedRuntimeSceneValidator`.
- Added `GeneratedBuildingSceneRoot > Validate Runtime Scene`.
- Full generation now logs both generated data validation and runtime scene
  validation.
- Checks include entrance/door width, entrance-based floor reachability, stair
  access, stair trigger adjacency, runtime identities, trigger states,
  `CompositeCollider2D.Geometry Type = Polygons`, visibility groups, and stair
  zone metadata.

Confirm:

- Generated houses fail validation if a Unit cannot reasonably traverse them.
- House01-like, SmallHouse, and LargeHouse profiles pass gameplay validation.
- Runtime scene validation passes after `Generate Building With Stairs`.
- Code compiles without errors.

## Reset Stage 25 - Generated Building Instance Data

Status: Complete

Goal:

- Store city-ready metadata for generated houses.
- Keep city placement data independent from visual Tilemap objects.
- Prepare city generation to place houses by footprint and connect roads to
  entrances.

Work:

- Added `GeneratedBuildingInstanceData`.
- Added `GeneratedBuildingInstanceDataView`.
- Generation now rebuilds/logs instance data after structural/runtime
  validation.
- `GeneratedBuildingDataView > Rebuild Instance Data` can rebuild instance
  metadata from the current generated data.
- Instance data includes instance id, building id, profile id, display name,
  seed, deterministic hash, footprint, occupied bounds, floor count, room
  count, stair count, entrance count, stair flag, primary entrance metadata,
  and primary road connection cell.

Confirm:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Console logs `Generated Building Instance Data`.
- `Building Generator` has `GeneratedBuildingInstanceDataView`.
- Footprint and primary road connection cell are available for future city
  placement.
- Code compiles without errors.

## Reset Stage 26 - City Generation Preparation

Status: Replaced By Building Generalization Stages

Goal:

- City generation remains the long-term target, but it should not start until
  the current House01-like generator is generalized enough to support multiple
  reliable building families.

Work:

- Follow `Assets/___Test/Docs/BuildingGeneralizationPlan.md`.
- Complete Stages 26A through 26H first.
- Do not implement full city generation until building validation and presets
  are reliable across multiple profiles.

Confirm:

- Stage 26H confirms multiple building profiles are generated, validated, and
  playable.
- Only then start Stage 27 city generation preparation.

## Reset Stage 26A - Architecture Rule Audit

Status: Complete

Goal:

- Separate generic building rules from House01 reference rules.
- Identify all hard-coded assumptions that prevent reusable building families.

Work:

- Audit `GeneratedBuildingProfile`, `GeneratedGroundGenerator`,
  `GeneratedRoomPlanner`, `GeneratedStairPlanner`, renderers, and validators.
- Classify each rule as generic, profile data, preset data, or House01-only
  reference rule.
- Document what must become data before city generation.

Confirm:

- A clear extraction list exists.
- No city generation work starts yet.

Result:

- Completed in `Assets/___Test/Docs/ArchitectureRuleAudit.md`.
- Next stage is 26B, because fixed Floor01/Floor02 binding is the highest
  priority blocker.

## Reset Stage 26B - Dynamic Floor Scene Binding

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Remove Floor01/Floor02-only assumptions from scene binding and generation.

Work:

- Replace fixed floor fields with floor-index based generated roots.
- Update renderers, colliders, visibility, runtime binding, and validation to
  loop through generated floors.
- Added floor-index based root helpers to `GeneratedBuildingSceneRoot`.
- Updated floor painting in `GeneratedGroundGenerator` to loop over
  `buildingData.Floors`.
- Updated collider, visibility, and runtime binding paths to resolve floor roots
  by floor index.
- Updated stair planning and validation to operate on adjacent floor pairs
  rather than only Floor 01 and Floor 02.
- Corrected Small/Large ceiling offsets after Unity visual confirmation.
- Adjusted upper-floor exterior corner generation to avoid an overlapping
  Floor 02 corner tile.

Confirm:

- 1-floor and 2-floor profiles use the same flow.
- Future 3-floor support is not blocked by fixed scene references.
- Code compiles without errors.

## Reset Stage 26C - Reusable Preset Assets

Status: Complete

Goal:

- Convert reusable architectural modules into standalone preset assets.

Work:

- Added `GeneratedStairModulePresetAsset`.
- Added `GeneratedRoomLayoutPresetAsset`.
- Added `GeneratedDoorPlacementPresetAsset`.
- Added `GeneratedEntrancePlacementPresetAsset`.
- Added reusable preset assets under `Assets/___Test/Data/Presets`.
- Updated House01-like, SmallHouse, and LargeHouse profiles to reference the
  shared door, entrance, and stair presets plus their matching room layout
  presets.
- Kept profile-local values as fallback data so existing assets remain
  compatible if a preset reference is intentionally removed.

Confirm:

- Profiles can share or swap presets without generator code changes.
- Code compiles without errors.

## Reset Stage 26D - Room Layout Strategy Upgrade

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Improve room variation beyond simple X-axis sector splitting.

Work:

- Added `GeneratedRoomLayoutStrategy`.
- Room layout presets now expose strategy, placement attempts per room, and
  stair clearance from rooms.
- The default preset strategy is Random Packed placement rather than simple
  X-axis sector splitting.
- Sector Split remains available as a compatibility/fallback strategy.
- Random Packed placement uses seeded candidates, scores better-spaced rooms,
  avoids generated floor holes, stair masks, and entrance clearance corridors,
  and checks basic door exterior clearance before accepting a room.
- Room collider tilemap roots are cleared before regeneration so stale room
  colliders cannot remain without matching visual rooms.
- Validation now enforces generated room count against profile min/max.

Confirm:

- Same seed is reproducible.
- Different seeds create meaningful variation.
- Rooms remain reachable.
- Code compiles without errors.

## Reset Stage 26E - Runtime Visibility And Floor Flow Stabilization

Status: Complete

Goal:

- Stabilize the current generated building runtime flow before adding more
  generation rules.

Work:

- Verify Floor 01 entry, Floor 01 room fade, stair transition to Floor 02,
  Floor 02 outer wall fade, Floor 02 room fade, return to Floor 01, and ceiling
  behavior.
- Confirm upper-floor hiding, floor wall fade, room wall fade, and ceiling fade
  do not overwrite each other.
- Add targeted runtime validation/debug support only where it prevents repeated
  regressions.
- Runtime scene validation now checks visibility target ownership, floor/room
  visibility group coverage, and required Test Unit floor/visibility/stair
  agents.

Confirm:

- House01-like and LargeHouse pass Play Mode floor/visibility checks.
- SmallHouse still works without stair transition logic.
- Code compiles without errors.

## Reset Stage 26F - Generation Pipeline Maintainability Refactor

Status: Complete

Goal:

- Keep the generator maintainable before adding more building-family rules.

Work:

- Split generation-run support out of `GeneratedGroundGenerator` into
  `GeneratedGenerationRunSupport`.
- Split main entrance generation out of `GeneratedGroundGenerator` into
  `GeneratedEntrancePlanner`.
- Split gameplay-readiness validation out of `GeneratedBuildingValidator` into
  `GeneratedBuildingGameplayValidator`.
- Keep `GeneratedRoomPlanner` unchanged in this pass to avoid changing room
  generation behavior during a maintainability-only stage.
- Keep generation data separate from rendering and runtime components.
- Avoid adding per-frame hierarchy scans or avoidable runtime allocations.

Confirm:

- Existing generated output remains stable unless intentionally changed.
- House01-like, SmallHouse, and LargeHouse still pass validation.
- Code compiles without errors.

## Reset Stage 26G - Door And Entrance Strategy Upgrade

Status: Complete

Goal:

- Make doors and entrances data-driven enough for future road connection.

Work:

- Added configurable room door side selection rules.
- Added configurable entrance position strategy, side padding, and road
  connection offset.
- Store generated entrance road connection metadata on `GeneratedEntranceData`.
- Keep `GeneratedBuildingInstanceData.PrimaryRoadConnectionCell` sourced from
  generated entrance data.
- Validate entrance road connection cell alignment and outside-floor placement.
- Use generated entrance data for Test Unit spawn when available.

Confirm:

- Different profiles can use different entrance sides and widths.
- `GeneratedBuildingInstanceData` still exposes valid city placement data.
- Code compiles without errors.

## Reset Stage 26H - Building Family Validation Matrix

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Prove the generator is not only correct for the current TestScene House01-like
  building.

Work:

- Added `WideHouseProfile` as a new one-floor wide profile variant.
- Added `GeneratedGroundGenerator > Validate Building Family Matrix`.
- Matrix validation regenerates each profile, then checks structural
  validation, runtime scene validation, seed reproducibility, and instance
  city-placement data.
- The matrix currently covers House01-like, SmallHouse, LargeHouse, and
  WideHouse.

Confirm:

- All selected profiles pass.
- Generated hierarchy cleanup works across profile switches.
- Unit movement, floor isolation, stair transition, and visibility work where
  applicable.
- Code compiles without errors.

## Reset Stage 27 - City Generation Preparation

Status: Blocked Until Stage 26H

Goal:

- Prepare city placement only after building families are sufficiently generic.

Work:

- Define building spacing rules.
- Define road connection assumptions.
- Define district/building type selection data.
- Define how a future city generator consumes profiles and instance data.

Confirm:

- A future `CityGenerator` can consume building profile candidates and generated
  instance data without inspecting Tilemap children.

## Operating Rules From This Point

- Keep prototype work inside `Assets/___Test` unless explicitly requested.
- Keep generation data separate from rendering.
- Keep visual tilemaps separate from collider/trigger tilemaps.
- Prefer Tilemap + CompositeCollider2D over per-tile GameObjects.
- Generated Tilemap collider/trigger objects should use
  `CompositeCollider2D.Geometry Type = Polygons` to keep isometric trigger
  regions stable.
- Avoid adding player-specific behavior before generated building data is
  stable.
- Every stage should end with a clear Unity-side confirmation checklist and
  `dotnet build Assembly-CSharp.csproj`.
- If a stage creates runtime data that later systems will consume, expose it in
  logs or Inspector-visible components.
