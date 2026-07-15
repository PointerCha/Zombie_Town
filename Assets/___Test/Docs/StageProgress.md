# Stage Progress

## Stage 1 - House 01 Reference

Status: Complete

Output:

- `Assets/___Test/House01Reference.md`

Confirmation received:

- Default footprint can start from the `House 01` reference.
- The generator should imitate the structure without reusing MainScene objects.
- Outer walls, room walls, and stairs should be separated into their own rules.

## Stage 2 - Test Workspace Structure

Status: Complete

Goal:

- Create the isolated folder layout for the new generator.
- Keep future code, data, debug files, and scene helpers organized.
- Avoid touching production scripts under `Assets/__Scripts`.

Completion checklist:

- `Data/Palettes`, `Data/Profiles`, and `Data/Presets` exist.
- Runtime script folders are separated by responsibility.
- Documentation and debug folders exist.
- `TestScene.unity` remains the target scene.

## Stage 3 - Core Generation Data Models

Status: Complete

Goal:

- Create the logical data model used before Tilemap rendering.
- Represent building, floor, room, wall, door, stair, and cell-mask data.
- Keep the data serializable for Inspector visibility.
- Use runtime lookup caches where repeated cell checks are expected.

Output:

- `Assets/___Test/Scripts/Data/BuildingGenerationTypes.cs`
- `Assets/___Test/Scripts/Data/GeneratedCellMask.cs`
- `Assets/___Test/Scripts/Data/GeneratedWallData.cs`
- `Assets/___Test/Scripts/Data/GeneratedDoorData.cs`
- `Assets/___Test/Scripts/Data/GeneratedRoomData.cs`
- `Assets/___Test/Scripts/Data/GeneratedStairData.cs`
- `Assets/___Test/Scripts/Data/GeneratedFloorData.cs`
- `Assets/___Test/Scripts/Data/GeneratedBuildingData.cs`
- `Assets/___Test/Scripts/Data/GeneratedBuildingDataView.cs`

Completion checklist:

- Data compiles without errors.
- Data is independent from Tilemap rendering.
- Ground footprint can be represented numerically.
- Floor holes, rooms, walls, doors, and stairs can be represented.
- A scene object can inspect/log `GeneratedBuildingData` through
  `GeneratedBuildingDataView`.

## Stage 4 - Tile Palette And Generation Profile

Status: Complete

Goal:

- Create ScriptableObject assets that separate tile references from generation
  rules.
- Store House 01-like tile references in a reusable palette.
- Store House 01-like footprint, grid, sorting, room, and stair settings in a
  reusable profile.

Output:

- `Assets/___Test/Scripts/Data/DirectionalTileSet.cs`
- `Assets/___Test/Scripts/Data/GeneratedBuildingTilePalette.cs`
- `Assets/___Test/Scripts/Data/GeneratedBuildingGridProfile.cs`
- `Assets/___Test/Scripts/Data/GeneratedBuildingSortingProfile.cs`
- `Assets/___Test/Scripts/Data/GeneratedBuildingProfile.cs`
- `Assets/___Test/Data/Palettes/House01TilePalette.asset`
- `Assets/___Test/Data/Profiles/House01LikeProfile.asset`

Completion checklist:

- Palette contains House 01 ground, ceiling, collision, wall, room-wall, door,
  stair, and default prop tile references.
- Profile references the palette.
- Profile uses `60 x 42` footprint and `2` floors.
- Profile stores grid sizes and sorting order rules from the House 01 reference.
- Code compiles without errors.

## Stage 5 - Test Scene Root Hierarchy

Status: Complete

Goal:

- Place the root objects required for the building generation pipeline in
  `Assets/___Test/TestScene.unity`.
- Bind the scene to `House01LikeProfile`.
- Mirror the major House 01 hierarchy shape without generating tiles yet.

Output:

- `Assets/___Test/Scripts/Scene/GeneratedBuildingSceneRoot.cs`
- `Assets/___Test/TestScene.unity`

Scene hierarchy:

```text
Building Generator
└─ Generated House Root
   ├─ Floors
   │  ├─ Floor 01
   │  │  ├─ Floor 01 Visual Tilemaps
   │  │  │  ├─ Ground
   │  │  │  ├─ Upper Wall
   │  │  │  └─ Under Wall
   │  │  ├─ Floor 01 Room Tilemaps
   │  │  │  ├─ Room 01 Visual Tilemap
   │  │  │  └─ Room 02 Visual Tilemap
   │  │  └─ Floor 01 Collider Tilemaps
   │  │     ├─ Floor Collider Tilemap
   │  │     ├─ Room 01 Collider Tilemap
   │  │     └─ Room 02 Collider Tilemap
   │  └─ Floor 02
   │     └─ Same placeholder layout as Floor 01
   ├─ Stair
   │  ├─ Stair Visual Tilemaps
   │  └─ Stair Collider And Trigger Tilemaps
   ├─ Ceiling
   └─ Back Occlusion Zone
```

Completion checklist:

- `Building Generator` exists in `TestScene`.
- `GeneratedBuildingSceneRoot` references `House01LikeProfile`.
- Major House 01-like hierarchy roots exist.
- No Tilemap rendering or collider components are generated yet.
- Scene transform references are valid.
- Code compiles without errors.

## Stage 6 - Ground Generation

Status: Complete

Goal:

- Generate numeric floor ground masks from `House01LikeProfile`.
- Paint Floor 01 and Floor 02 `Ground` tilemaps from the profile palette.
- Keep generation manual through an Inspector context menu for safe iteration.

Output:

- `Assets/___Test/Scripts/Rendering/GeneratedTilemapUtility.cs`
- `Assets/___Test/Scripts/Generation/GeneratedGroundGenerator.cs`
- `GeneratedBuildingDataView` attached to `Building Generator`
- `GeneratedGroundGenerator` attached to `Building Generator`

Completion checklist:

- `GeneratedGroundGenerator > Generate Ground Only` fills Floor 01 and Floor 02
  ground masks using a `60 x 42` footprint.
- Floor ground data is visible through `GeneratedBuildingDataView`.
- Visual tilemap parents receive `Grid` components when generation runs.
- Ground objects receive `Tilemap` and `TilemapRenderer` components when
  generation runs.
- Code compiles without errors.
- Scene transform references are valid.

## Stage 7 - Outer Wall Generation

Status: Complete

Goal:

- Generate outer wall data from each floor footprint.
- Split outer walls into `Upper Wall` and `Under Wall` visual tilemaps.
- Keep wall generation layered separately from ground generation.

Output:

- `GeneratedGroundGenerator > Generate Building Shell`
- `GeneratedTilemapUtility.PaintWall`
- Floor 01 and Floor 02 wall target bindings in `TestScene`

Generation rule:

- House 01 Floor 01 uses `OuterWallPrimary` (`Wall E1`) for regular outer wall
  lines.
- House 01 Floor 01 uses `OuterWallSecondary` (`Wall E2`) only for outer corner
  correction tiles.
- `Upper Wall` renders the top edge with `Wall E1_W`, the right edge with
  `Wall E1_N`, and three corner correction tiles.
- `Under Wall` renders the bottom edge with `Wall E1_E`, the left edge with
  `Wall E1_S`, and one corner correction tile.
- Door and stair-hole-specific exceptions are intentionally left for later
  refinement stages.

Completion checklist:

- `Generate Ground Only` still works as the Stage 6 isolated check.
- `Generate Building Shell` fills Ground plus Floor 01/Floor 02 outer walls.
- `GeneratedBuildingDataView` shows outer wall data for each floor.
- Code compiles without errors.
- Scene transform references are valid.

## Stage 8 - Room Layout And Visual Generation

Status: Complete

Goal:

- Generate room layout data inside each floor footprint.
- Keep room placement deterministic from the building seed.
- Prevent room overlap by dividing the usable floor area into room sectors.
- Render room ground and room walls separately under each room visual group.

Output:

- `Assets/___Test/Scripts/Generation/GeneratedRoomPlanner.cs`
- `GeneratedGroundGenerator > Generate Building With Rooms`
- Runtime-created room visual child layers:
  `Ground`, `Upper Wall`, and `Under Wall`

Generation rule:

- Room count is read from `House01LikeProfile`.
- Room bounds are randomized within each sector but remain reproducible from
  the seed.
- Room floor uses the profile ground tile for now.
- Room walls use `RoomWallPrimary`.
- Floor visual separation is controlled by `House01LikeProfile.floorVisualOffsetStep`
  instead of manual per-floor scene edits.
- Door openings, room corner correction tiles, room colliders, and visibility
  triggers are intentionally left for later stages.

Completion checklist:

- `Generate Building With Rooms` fills building ground, outer walls, and room
  visuals.
- Each generated room group contains `Ground`, `Upper Wall`, and `Under Wall`
  child tilemaps.
- `GeneratedBuildingDataView` shows generated rooms and room wall data.
- Code compiles without errors.

## Stage 9 - Room Door Opening And Visual Generation

Status: Complete

Goal:

- Add one default doorway to each generated room.
- Remove the matching wall cell from room wall data so later collider
  generation does not create a blocked doorway.
- Render door tiles after room wall tiles.

Output:

- `GeneratedWallData.RemoveCell`
- `GeneratedRoomPlanner` default room door generation
- `GeneratedTilemapUtility.PaintDoor`
- Door rendering in `GeneratedGroundGenerator`

Generation rule:

- Each generated room currently receives one default door on the front/bottom
  room wall.
- The door uses the profile `DoorTiles` directional tile set.
- Door cells are cut from the matching room wall data before rendering.
- Door colliders, door interaction logic, and smarter multi-side door placement
  are intentionally left for later stages.

Completion checklist:

- `Generate Building With Rooms` now creates room doors.
- Room wall data no longer contains the door cell.
- Door tiles appear on room wall tilemaps.
- Code compiles without errors.

## Stage 10 - Floor And Room Collider Generation

Status: Complete

Goal:

- Generate collider tilemaps from the same data used by visual generation.
- Keep collision/trigger tilemaps separate from visual tilemaps.
- Use `TilemapCollider2D` with `CompositeCollider2D` to reduce collider shape
  count.

Output:

- `Assets/___Test/Scripts/Rendering/GeneratedColliderRenderer.cs`
- `GeneratedTilemapUtility.EnsureColliderTilemap`
- `GeneratedTilemapUtility.PaintWallCollision`
- `GeneratedGroundGenerator > Generate Building With Colliders`

Generation rule:

- Floor ground creates a `Ground Trigger` collider tilemap.
- Room ground creates a `Ground Trigger` collider tilemap per room.
- Outer walls create non-trigger `Upper Wall` and `Under Wall` collider
  tilemaps.
- Room walls create non-trigger `Upper Wall` and `Under Wall` collider
  tilemaps.
- Door cells are already removed from room wall data, so door openings are not
  blocked by generated room wall colliders.
- This stage uses the visual cell grid for reliable first-pass alignment.
  Smaller House 01-style wall collider sub-grids are left for a later precision
  pass.

Completion checklist:

- `Generate Building With Colliders` creates visible building/room visuals plus
  hidden collider tilemaps.
- Generated collider objects contain `Tilemap`, `TilemapCollider2D`,
  `Rigidbody2D`, and `CompositeCollider2D`.
- Ground collider tilemaps are triggers.
- Wall collider tilemaps are non-trigger colliders.
- Door openings remain clear in room wall collider data.
- Code compiles without errors.

## Stage 11 - Basic Stair And Upper Floor Hole Generation

Status: Complete

Goal:

- Generate the first stair module connecting Floor 01 and Floor 02.
- Cut the upper-floor ground hole from generated floor data before rendering.
- Render the stair visual tilemap from generated stair data.

Output:

- `Assets/___Test/Scripts/Generation/GeneratedStairPlanner.cs`
- `Assets/___Test/Scripts/Rendering/GeneratedStairRenderer.cs`
- `GeneratedGroundGenerator > Generate Building With Stairs`
- House 01-like stair profile bounds:
  visual `(-13, 0, 5, 6)`, upper floor hole `(-15, 0, 6, 5)`

Generation rule:

- The first stair connects Floor 01 to Floor 02.
- Floor 02 removes a 30-cell stair entrance hole before ground rendering.
- Stair visual uses the House 01-like 12-cell isometric pattern.
- Stair walkable, rail, entry, and exit masks are generated as basic data for
  later transition logic.
- Stair transition behavior and precise stair rail collider tuning are left for
  later stages.

Completion checklist:

- `Generate Building With Stairs` creates the building, rooms, doors,
  colliders, and stair visual.
- `GeneratedBuildingDataView` should show Floor 02 `holes=30` and
  `groundCells=2490`.
- Floor 01 and Floor 02 should both show `stairs=1`.
- The Stair root should contain a `Stair Visual Tilemaps/Visual Tilemap`
  tilemap with stair tiles.
- Code compiles without errors.

## Stage 12 - Stair Collider And Trigger Generation

Status: Complete

Goal:

- Render generated stair masks into the existing stair collider/trigger
  hierarchy.
- Separate stair walkable/transition triggers from rail collision.
- Keep stair collider data generated from the same stair data used for visual
  output.

Output:

- `GeneratedStairRenderer` now paints stair collider and trigger tilemaps.
- `Stair Collider And Trigger Tilemaps/Walkable Trigger Area Tilemap`
- `Stair Collider And Trigger Tilemaps/Left Rail Collider Tilemap`
- `Stair Collider And Trigger Tilemaps/Right Rail Collider Tilemap`
- `Stair Collider And Trigger Tilemaps/Floor Entry Trigger Tilemap`
- `Stair Collider And Trigger Tilemaps/Floor Exit Trigger Tilemap`

Generation rule:

- Walkable, entry, and exit stair tilemaps are triggers.
- Left and right rail tilemaps are non-trigger colliders.
- All generated stair collider tilemaps use `TilemapCollider2D`,
  `Rigidbody2D`, and `CompositeCollider2D`.
- Generated collider tilemaps now force `CompositeCollider2D.Geometry Type` to
  `Polygons`, including floor ground triggers, room ground triggers, wall
  colliders, and stair utility colliders/triggers.
- This stage uses the visual grid for first-pass alignment with the generated
  stair hole. House 01-style small stair collider grids are left for a later
  precision pass if needed.

Completion checklist:

- `Generate Building With Stairs` creates stair visual and stair collider
  tilemaps.
- Walkable, entry, and exit objects have `Is Trigger` enabled.
- Left and right rail objects have `Is Trigger` disabled.
- Stair collider/trigger tilemaps are under `Stair Collider And Trigger
  Tilemaps`.
- Code compiles without errors.

## Stage 13 - House 01-Style Precise Stair Collider Grid

Status: Complete

Goal:

- Replace the first-pass visual-grid stair utility masks with House 01-style
  fine-grid stair data.
- Use the dedicated stair collider grid size instead of the normal visual grid.
- Keep stair visual tiles unchanged while improving walkable, entry, exit, and
  rail collider precision.

Output:

- `GeneratedStairPlanner` now creates fine stair utility masks from the House
  01 relative stair shape.
- `GeneratedStairRenderer` now creates stair collider/trigger tilemaps with
  `GeneratedBuildingGridProfile.stairColliderCellSize`.
- `GeneratedStairData.BuildSummary` exposes mask counts for verification.

Generation rule:

- Stair visual still uses the 12-cell House 01-like visual pattern on the
  normal visual grid.
- Stair walkable trigger uses the fine stair collider grid and should generate
  `786` cells.
- Floor entry trigger should generate `22` cells.
- Floor exit trigger should generate `21` cells.
- Left and right rail collider masks each generate `27` cells as practical
  fine-grid boundary rails.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- `GeneratedBuildingDataView` log should show `walkable=786`, `entry=22`,
  `exit=21`, `leftRail=27`, and `rightRail=27` for `stair[0]`.
- `Stair Collider And Trigger Tilemaps` should use a `Grid` cell size of
  `0.2, 0.1, 1`.
- The stair visual should remain close to the corrected House 01 stair shape,
  while the collision/trigger area should no longer appear as large visual-grid
  blocks.
- Code compiles without errors.

## Stage 14 - House 01-Style Stair Rail Precision

Status: Complete

Goal:

- Match generated stair rail colliders to the actual `House 01` stair rail
  structure.
- Keep walkable, entry, and exit triggers unrotated.
- Apply the House 01-style `30` degree Z rotation only to rail collider
  tilemaps.

Output:

- `GeneratedStairPlanner` now generates rail masks from House 01 relative rail
  coordinates instead of deriving them from the walkable trigger boundary.
- `GeneratedStairRenderer` now resets stair utility transforms every generation
  and applies `30` degree local Z rotation to `Left Rail Collider Tilemap` and
  `Right Rail Collider Tilemap`.

Generation rule:

- Walkable, entry, and exit stair trigger tilemaps use local rotation `0`.
- Left and right rail collider tilemaps use local rotation `30`.
- Rail coordinates are converted from the House 01 reference stair visual
  position into the current generated stair visual position through the inverse
  rotated isometric rail grid. Do not subtract visual Y directly from rail cell
  Y, because rail tilemaps are rotated.
- Rail masks still generate `27` cells each, matching the House 01 rail count.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- `Left Rail Collider Tilemap` and `Right Rail Collider Tilemap` should show
  local Z rotation `30`.
- Rail collider outlines should sit close to the stair side edges rather than
  spreading outward as unrotated horizontal boundary lines.
- `GeneratedBuildingDataView` should still show `leftRail=27` and
  `rightRail=27`.
- Code compiles without errors.

## Stage 15 - Stair Zone Metadata Binding

Status: Complete

Goal:

- Make generated stair trigger/collider objects identifiable at runtime without
  parsing object names.
- Store stair id, zone kind, and floor transition information directly on the
  generated stair zone GameObjects.
- Prepare the stair data for later Unit floor-transition logic.

Output:

- `GeneratedStairZoneKind` enum added.
- `GeneratedStairZone` component added under `Assets/___Test/Scripts/Scene`.
- `GeneratedStairRenderer` now attaches/configures `GeneratedStairZone` on
  walkable, entry, exit, and rail tilemap objects.

Generation rule:

- `Walkable Trigger Area Tilemap` receives zone kind `Walkable`.
- `Floor Entry Trigger Tilemap` receives zone kind `Entry`, source floor `1`,
  destination floor `2`.
- `Floor Exit Trigger Tilemap` receives zone kind `Exit`, source floor `2`,
  destination floor `1`.
- `Left Rail Collider Tilemap` and `Right Rail Collider Tilemap` receive rail
  zone kinds but do not perform floor transitions.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Select each stair utility tilemap object and confirm it has a
  `GeneratedStairZone` component.
- Entry should show `Source Floor Index = 1` and `Destination Floor Index = 2`.
- Exit should show `Source Floor Index = 2` and `Destination Floor Index = 1`.
- Walkable and rail zones should show `Source/Destination Floor Index = 0`.
- Code compiles without errors.

## Reset Stage 16 - Ceiling Generation

Status: Complete

Goal:

- Generate a ceiling tilemap for the whole building footprint.
- Keep ceiling visuals separate from floor, room, stair, collider, and trigger
  tilemaps.
- Prepare the generated ceiling object for later visibility metadata binding.

Output:

- `Assets/___Test/Scripts/Rendering/GeneratedCeilingRenderer.cs`
- `GeneratedGroundGenerator > Generate Building With Colliders` now renders the
  ceiling.
- `GeneratedGroundGenerator > Generate Building With Stairs` now renders the
  ceiling.

Generation rule:

- Ceiling uses `GeneratedBuildingData.Footprint`.
- Ceiling applies `GeneratedBuildingTilePalette.ceilingCellOffset` because the
  required visual alignment belongs to the selected tile set, not to an
  individual building size profile.
- The House 01 palette uses `(3, 3)`, matching the manually corrected TestScene
  ceiling bounds.
- Ceiling renders into `Ceiling/Ceiling Grid/Visual Tilemap`.
- Ceiling uses `GeneratedBuildingTilePalette.CeilingTile`.
- Ceiling sorting order comes from `GeneratedBuildingSortingProfile.CeilingOrder`.
- Runtime scene validation checks the exact generated ceiling cell bounds, so a
  future offset mismatch fails validation instead of relying only on visual
  inspection.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- `Ceiling/Ceiling Grid/Visual Tilemap` should contain ceiling tiles across the
  generated building footprint.
- Ceiling should not overwrite floor, room, stair, collider, or trigger
  tilemaps.
- Code compiles without errors.

## Reset Stage 17 - Visibility Metadata Structure

Status: Complete

Goal:

- Generate visibility-ready metadata for floors, rooms, ceiling, and stair
  visual targets.
- Avoid connecting player behavior before generated building data is stable.
- Keep the test visibility metadata isolated under `Assets/___Test`.

Output:

- `GeneratedVisibilityGroupKind` enum.
- `Assets/___Test/Scripts/Scene/GeneratedVisibilityGroup.cs`.
- `Assets/___Test/Scripts/Scene/GeneratedVisibilityBinder.cs`.
- `GeneratedGroundGenerator > Generate Building With Colliders` now binds
  visibility metadata.
- `GeneratedGroundGenerator > Generate Building With Stairs` now binds
  visibility metadata.

Generation rule:

- Floor visibility groups are attached to each floor root and reference the
  floor outer `Upper Wall` and `Under Wall` renderers.
- Room visibility groups are attached to each generated room visual group and
  reference the room `Upper Wall` and `Under Wall` renderers.
- Ceiling visibility group is attached to the `Ceiling` root and references
  `Ceiling Grid/Visual Tilemap`.
- Stair visibility group is attached to the `Stair` root and references
  `Stair Visual Tilemaps/Visual Tilemap`.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Floor 01 and Floor 02 root objects should have `GeneratedVisibilityGroup`.
- Each generated room visual group should have `GeneratedVisibilityGroup`.
- `Ceiling` should have `GeneratedVisibilityGroup` with one target.
- `Stair` should have `GeneratedVisibilityGroup` with one target.
- Code compiles without errors.

## Reset Stage 18 - Building Validation System

Status: Complete

Goal:

- Validate generated building data before depending on visual inspection.
- Detect common structural mistakes early with actionable logs.
- Keep validation isolated under `Assets/___Test`.

Output:

- `Assets/___Test/Scripts/Validation/GeneratedBuildingValidationResult.cs`.
- `Assets/___Test/Scripts/Validation/GeneratedBuildingValidator.cs`.
- `GeneratedBuildingDataView > Validate Building Data` context menu.
- Full collider/stair generation paths now validate after visibility metadata
  binding.

Validation rule:

- Profile and palette references must be valid.
- Building footprint must be positive.
- Floors must exist and have non-empty ground masks.
- Room bounds must be inside their floor.
- Rooms on the same floor must not overlap.
- Room doors must match their room/floor and must not remain blocked by room
  wall data.
- Stair data must contain visual, walkable, entry, exit, and both rail masks.
- Stair lower/upper floor indexes must point to existing floors.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Console should show a validation pass/fail summary.
- `GeneratedBuildingDataView > Validate Building Data` should also run the
  same validation against the current stored data.
- Valid current House 01-like generation should not produce validation errors.
- Code compiles without errors.

## Reset Stage 19 - Connectivity Validation

Status: Complete

Goal:

- Validate that generated logical building data is internally traversable.
- Catch rooms that exist structurally but cannot be reached through their doors.
- Catch stair data that is not linked by both connected floors.

Output:

- `GeneratedBuildingValidator` now performs grid-based flood-fill checks.
- `GeneratedBuildingValidator` now checks stair floor references and upper-floor
  stair hole presence.

Connectivity rule:

- Walkable cells are computed from floor `GroundMask`.
- Outer and room wall cells are removed from the walkable set.
- A public start cell is selected from walkable cells outside every room.
- Flood fill uses 4-direction grid adjacency.
- A room is reachable when at least one door cell and adjacent room interior
  cell are reachable from the public floor area.
- A stair is connected when both lower and upper floors reference the stair id
  and the upper floor has stair hole cells.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Console should still show validation passed with `Errors: 0`.
- If a room door remains blocked by wall data, validation should fail.
- If a room has no reachable door, validation should fail.
- If a stair is missing from one of its floors, validation should fail.
- Code compiles without errors.

## Reset Stage 20 - Seed And Regeneration Workflow

Status: Complete

Goal:

- Make deterministic generation easy to verify.
- Confirm same seed produces the same generated data.
- Confirm different seed generation remains valid and can produce variation.

Output:

- `GeneratedBuildingData.BuildDeterministicFingerprint`.
- `GeneratedBuildingData.BuildDeterministicHash`.
- `GeneratedGroundGenerator > Validate Seed Reproducibility` context menu.

Generation rule:

- Fingerprints are built from generated logical data, not from rendered
  Tilemap scene objects.
- Same seed is generated twice and compared by fingerprint and hash.
- A different seed is generated and validated separately.
- Validation must pass for all generated seed samples.

Completion checklist:

- Run `GeneratedGroundGenerator > Validate Seed Reproducibility`.
- Console should report same seed reproducibility passed.
- Same seed hashes should match.
- Different seed hash should normally differ.
- All seed samples should pass building validation.
- Code compiles without errors.

## Reset Stage 21 - Preset Data Extraction

Status: Complete

Goal:

- Move hard-coded stair module values out of `GeneratedStairPlanner`.
- Make House 01-like stair shape values inspectable through profile data.
- Keep planner code focused on applying preset data rather than owning shape
  constants.

Output:

- `Assets/___Test/Scripts/Data/GeneratedStairModulePreset.cs`.
- `GeneratedBuildingProfile` now owns a `GeneratedStairModulePreset`.
- `House01LikeProfile.asset` now stores the House 01-like stair module values.
- `GeneratedStairPlanner` now reads visual rows, walkable rows, triggers,
  rail data, fine grid scale, and fallback bounds from the profile stair
  module preset.

Preset rule:

- Building-wide rules remain in `GeneratedBuildingProfile`.
- Tile references remain in `GeneratedBuildingTilePalette`.
- Stair shape rules live in `GeneratedStairModulePreset`.
- The stair module preset is currently embedded in the profile asset. It can be
  promoted to a separate ScriptableObject asset later if multiple stair module
  assets are needed.

Completion checklist:

- `GeneratedStairPlanner` should not contain raw House 01 coordinate constants.
- `House01LikeProfile.asset` should expose stair module values.
- `Generate Building With Stairs` should still produce the same stair summary:
  `visual=12`, `walkable=786`, `leftRail=27`, `rightRail=27`,
  `entry=22`, `exit=21`.
- `Validate Seed Reproducibility` should still pass.
- Code compiles without errors.

## Reset Stage 22 - External Entrance Data

Status: Complete

Goal:

- Add building entrance data needed for later city placement and road
  connection.
- Ensure the generated entrance is not blocked by outer wall data.
- Include entrance data in validation and deterministic fingerprints.

Output:

- `GeneratedEntranceKind` enum.
- `Assets/___Test/Scripts/Data/GeneratedEntranceData.cs`.
- `GeneratedBuildingData` now stores entrance data.
- `GeneratedBuildingProfile` now stores default entrance side and floor index.
- `GeneratedGroundGenerator` now generates one main entrance and cuts the
  matching outer wall cell.
- `GeneratedBuildingValidator` now validates entrance bounds, wall opening, and
  public-floor reachability.

Generation rule:

- The House 01-like profile uses `Default Entrance Side = East` and
  `Default Entrance Floor Index = 1`.
- The generated main entrance is stored as `main_entrance_01`.
- Entrance data records kind, floor index, cell, and wall side.
- Entrance visual tile rendering is intentionally left for a later visual
  refinement stage.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- `Generated Building Data` summary should show `Entrances: 1`.
- Validation should pass with `Errors: 0`.
- The entrance cell should not remain in outer wall data.
- `Validate Seed Reproducibility` should still pass.
- Code compiles without errors.

## Reset Stage 23 - Multiple Building Presets

Status: Complete

Goal:

- Prove the generator can reuse the same data, rendering, collider, ceiling,
  visibility, entrance, and validation pipeline across different house profile
  sizes.

Output:

- `Assets/___Test/Data/Profiles/SmallHouseProfile.asset`.
- `Assets/___Test/Data/Profiles/SmallHouseProfile.asset.meta`.
- `Assets/___Test/Data/Profiles/LargeHouseProfile.asset`.
- `Assets/___Test/Data/Profiles/LargeHouseProfile.asset.meta`.
- `GeneratedTilemapUtility.ClearTilemapsInChildren`.
- Stale Floor 02 tilemap cleanup when the active profile has only one floor.
- Stale generated room tilemap cleanup when a profile generates fewer rooms
  than the previously generated profile.

Generation rule:

- `House01LikeProfile` remains the structural reference.
- `SmallHouseProfile` is a one-floor profile with no stairs and a smaller
  footprint.
- `LargeHouseProfile` is a larger two-floor profile with more rooms and the
  existing House 01-like stair module.
- Profile switching should not require hand-editing generated scene objects.

Completion checklist:

- Assign `House01LikeProfile`, run `GeneratedGroundGenerator > Generate Building
  With Stairs`, and confirm validation passes.
- Assign `SmallHouseProfile`, run the same command, and confirm validation
  passes with `Floors: 1` and no stale Floor 02/stair visuals.
- Assign `LargeHouseProfile`, run the same command, and confirm validation
  passes with two floors and stair data.
- Code compiles without errors.

## Room Wall And Door Visual Correction

Status: Complete

Reason:

- During preparation for the next stage, generated rooms showed mismatched wall
  ends compared with the manually adjusted `Floor 01 / Room 01 Visual Tilemap`
  reference.
- Room doors looked too narrow because the door system only represented one
  tile cell.

Output:

- `GeneratedDoorData` now stores multiple door cells while keeping the first
  cell as the anchor.
- `GeneratedRoomPlanner` now matches the saved Room 01 reference wall layout:
  Upper Wall paints the full top line plus the right line, Under Wall paints
  the bottom line plus the left line.
- Room corner wall cells are represented as separate `_corner_` wall data so
  they can be rendered with `RoomWallSecondary`.
- `RoomWallSecondary` uses the `Wallpaper A2_E/N/S/W` tile assets from
  `Assets/Tile/03_Room Wall/01/_Tile`.
- The saved `Floor 01 / Room 01 Visual Tilemap` corner mapping is:
  top-left `Wallpaper A2_S`, top-right `Wallpaper A2_W`,
  bottom-right `Wallpaper A2_N`, bottom-left `Wallpaper A2_E`.
- Default generated room doors now occupy two cells.
- Room wall opening cuts remove every generated door cell.
- Door rendering paints every generated door cell.
- Door validation and room connectivity validation now use all door cells.
- Deterministic fingerprints now include all door cells.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Room corners should no longer have missing wall tile gaps.
- Room doors should consume two cells and leave a wider opening.
- Validation should still pass.
- Code compiles without errors.

## Roadmap Runtime Readiness Reset

Status: Complete

Reason:

- The generator can create first-pass house structures, but generated houses are
  not yet proven gameplay-ready with a Unit.
- Moving directly into city generation would multiply unresolved floor,
  collider, trigger, visibility, and stair runtime problems.

Output:

- `RoadmapReset.md` now inserts `Stage 24A` through `Stage 24F` before city
  instance data.
- `Stage 24A`: generated building runtime readiness.
- `Stage 24B`: Unit integration test.
- `Stage 24C`: floor isolation.
- `Stage 24D`: visibility runtime binding.
- `Stage 24E`: stair runtime transition.
- `Stage 24F`: playability validation.
- Former generated building instance data work is now `Stage 25`.
- City generation preparation is now `Stage 26`.

Rule:

- Do not proceed to city-scale generation until generated houses are verified
  with Unit movement, current-floor isolation, stair transitions, and runtime
  visibility behavior.

## Reset Stage 24A - Generated Building Runtime Readiness

Status: Complete

Goal:

- Make generated building objects discoverable by Unit/runtime systems without
  parsing hierarchy names.

Output:

- `GeneratedRuntimeObjectKind` enum.
- `Assets/___Test/Scripts/Scene/GeneratedRuntimeIdentity.cs`.
- `Assets/___Test/Scripts/Scene/GeneratedRuntimeBinder.cs`.
- `GeneratedGroundGenerator > Generate Building With Colliders` now binds
  runtime identities.
- `GeneratedGroundGenerator > Generate Building With Stairs` now binds runtime
  identities.
- `GeneratedBuildingSceneRoot > Log Runtime Identity Summary` context menu.

Runtime identity coverage:

- Building root.
- Floor roots.
- Room visual roots.
- Ceiling root.
- Stair root.
- Floor ground trigger.
- Floor upper/under wall colliders.
- Room ground triggers.
- Room upper/under wall colliders.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Select generated building/floor/room/collider objects and confirm
  `GeneratedRuntimeIdentity` is attached where expected.
- Run `GeneratedBuildingSceneRoot > Log Runtime Identity Summary`.
- Confirm the summary reports Building, Floor, Room, Floor/Room collider, Floor
  Trigger, Room Trigger, Ceiling, and Stair identities.
- Code compiles without errors.

## Reset Stage 24B - Unit Integration Test

Status: Complete

Goal:

- Add a test-only Unit that can be used to verify generated house playability
  before floor isolation, visibility runtime binding, and stair transitions are
  implemented.

Output:

- `Assets/___Test/Scripts/Unit/GeneratedTestUnitController.cs`.
- `Assets/___Test/Scripts/Unit/GeneratedTestUnitGizmos.cs`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` context menu.

Test Unit behavior:

- Creates or resets a `Generated Test Unit` near the generated main entrance.
- Adds Rigidbody2D, CircleCollider2D, `GeneratedTestUnitController`, and
  `GeneratedTestUnitGizmos`.
- Adds a runtime fallback SpriteRenderer visual if no sprite is assigned.
- Selected state changes the Unit color in Game View.
- Left click selects the test Unit.
- Right click moves the selected test Unit.
- Movement uses collider casts to stop against generated wall colliders.
- Left-click, right-click, ignored input, and blocked movement events are logged
  to make input failures diagnosable.
- Trigger enter/exit logs are emitted when the Unit overlaps generated
  `GeneratedRuntimeIdentity` trigger objects.
- Main entrance generation cuts a 2-cell outer wall opening so
  `Generate Building With Stairs` does not restore a blocking Under Wall
  collider at the entrance.
- Floor-specific collision filtering moved to Stage 24C.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Run `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- Enter Play Mode.
- Right-click into the building and rooms.
- If movement does not happen, check whether Console logs show left-click
  selection, right-click destination, or movement-blocked messages.
- Confirm the Unit stops at generated walls.
- Confirm the Unit can pass through generated doors.
- Confirm Floor/Room Ground Trigger enter/exit logs appear.
- Code compiles without errors.

## Reset Stage 24C - Floor Isolation

Status: Complete

Goal:

- Ensure a Unit only collides with and triggers generated objects on its current
  floor.
- Keep floor isolation reusable for later stair transition logic.

Output:

- `Assets/___Test/Scripts/Unit/GeneratedFloorIsolationAgent.cs`.
- `GeneratedTestUnitController` now delegates floor filtering to
  `GeneratedFloorIsolationAgent`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` now adds
  `GeneratedFloorIsolationAgent` and initializes the Unit to Floor 01.

Generation/runtime rule:

- Generated collider and trigger objects keep their floor metadata through
  `GeneratedRuntimeIdentity.FloorIndex`.
- `GeneratedFloorIsolationAgent` applies `Physics2D.IgnoreCollision` between
  the Unit body collider and generated colliders/triggers whose floor index
  does not match the Unit's current floor.
- Floor index `0` objects remain globally active. This is reserved for
  building-level/stair-level objects that are not owned by one floor yet.
- `GeneratedTestUnitController.SetCurrentFloorIndex` remains the entry point
  later stair transition code should call.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Run `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- Confirm `Generated Test Unit` has `GeneratedFloorIsolationAgent`.
- In Play Mode, move on Floor 01 and confirm Floor 02 wall/room/ground
  colliders do not block movement or emit trigger logs.
- Manually set `GeneratedFloorIsolationAgent.Current Floor Index` to `2`,
  refresh floor collision isolation, and confirm Floor 02 colliders become the
  active floor while Floor 01 room/wall colliders are ignored.
- Code compiles without errors.

## Reset Stage 24D - Visibility Runtime Binding

Status: Implementation Complete - Play Mode Confirmation Required

Goal:

- Connect generated visibility metadata to actual Unit-driven transparency.
- Fade generated floor outer walls, room walls, and ceiling targets from cached
  `GeneratedVisibilityGroup` data.
- Avoid per-frame hierarchy scans for renderer discovery.

Output:

- `Assets/___Test/Scripts/Scene/GeneratedVisibilityController.cs`.
- `Assets/___Test/Scripts/Unit/GeneratedVisibilityAgent.cs`.
- `GeneratedVisibilityBinder` now attaches/rebuilds
  `GeneratedVisibilityController` on `Generated House Root`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` now adds
  `GeneratedVisibilityAgent`.

Runtime rule:

- `GeneratedVisibilityController` caches generated visibility groups once when
  generation binds visibility data.
- Floor ground trigger occupancy fades that floor's outer wall visibility group
  and the ceiling visibility group.
- Room ground trigger occupancy fades that room's wall visibility group.
- Floor ground trigger occupancy hides visual tilemaps from floors above the
  Unit's current floor so upper floors do not block lower-floor inspection.
- Stair floor changes also refresh upper-floor visual hiding through the Unit
  visibility agent.
- Stair floor changes keep the ceiling hidden while the Unit is in the
  generated building/stair context, avoiding a visible ceiling flash before the
  next floor ground trigger is registered.
- Ceiling visibility now also respects the Unit's current-floor context, so
  leaving a thin stair endpoint/ground trigger does not restore the ceiling
  while the Unit is still inside the generated building or stair transition.
- Current-floor changes now also register the destination floor visibility
  group, so Floor 02 outer Under/Upper walls fade immediately after a stair
  transition even before a new floor ground trigger enter event is received.
- Current-floor context and Ground Trigger occupancy are tracked separately, so
  a Ground Trigger exit cannot cancel the fade that should remain active
  because the Unit is currently on that floor.
- Floor visual hide/show can directly touch Tilemap alpha, so visibility group
  states now reapply their alpha when already at target. This prevents entering
  a floor Ground Trigger from restoring Under/Upper Wall visuals while the
  controller still thinks they are faded.
- Upper-floor hiding now has priority over floor outer-wall restore. When the
  Unit is on Floor 01, Floor 02 outer wall groups keep the upper-floor hidden
  alpha instead of being restored by the floor visibility group.
- Floor visual hide/show now controls only floor ground and room ground
  tilemaps. Outer walls are controlled by floor visibility groups, and room
  walls are controlled by room visibility groups, preventing current-floor
  restoration from overriding room wall fades.
- Room visibility groups now also respect upper-floor hidden priority, so room
  walls above the Unit's current floor stay hidden until that floor becomes
  active.
- Floor visibility groups hide upper-floor outer walls, but lower-floor outer
  walls remain visible when the Unit moves to a higher floor.
- Room visibility groups hide upper-floor room walls and lower-floor room walls
  that are not currently occupied, preventing inactive room walls from drawing
  over the active floor.
- Floor Ground Trigger enter now also registers current-floor context for the
  visibility agent, so current-floor room walls do not get mistaken for
  non-current-floor walls.
- `GeneratedVisibilityAgent` ignores trigger objects from floors that are not
  active through `GeneratedFloorIsolationAgent`.
- Alpha transitions are centralized on `GeneratedVisibilityController`.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Confirm `Generated House Root` has `GeneratedVisibilityController`.
- Run `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- Confirm `Generated Test Unit` has `GeneratedVisibilityAgent`.
- In Play Mode, entering Floor 01 Ground Trigger should fade Floor 01 outer
  walls and ceiling.
- While the Unit is on Floor 01, Floor 02 visual tilemaps should become nearly
  invisible.
- After the Unit transitions to Floor 02, Floor 02 visual tilemaps should become
  visible again.
- After the Unit transitions to Floor 02, Floor 02 outer walls should fade so
  rooms can be inspected.
- Entering a generated room should fade that room's Upper/Under Wall tilemaps.
- Exiting the floor/room should restore the corresponding visuals.
- Other floors should not react while the Unit is on Floor 01.
- Code compiles without errors.

## Reset Stage 24E - Stair Runtime Transition

Status: Implementation Complete - Play Mode Confirmation Required

Goal:

- Use generated stair trigger metadata to update the Unit's current floor.
- Prevent unwanted floor toggles while the Unit walks across the same stair.
- Keep stair transition logic isolated from movement, visibility, and generation
  code.

Output:

- `Assets/___Test/Scripts/Unit/GeneratedStairTransitionAgent.cs`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` now adds
  `GeneratedStairTransitionAgent`.

Runtime rule:

- `GeneratedStairTransitionAgent` reads `GeneratedStairZone` from stair trigger
  tilemaps.
- Entering an Entry/Exit transition zone changes floor only when the Unit's
  current floor matches the zone's source floor.
- After one transition, the same stair is locked until the Unit leaves the
  stair Walkable trigger area.
- The stair state now follows the MainScene endpoint model: touching the entry
  endpoint begins a transition, touching the target endpoint completes it, and
  touching the original endpoint returns to the source floor.
- Thin Walkable/Entry trigger exits no longer complete or revert the transition.
- Floor isolation and visibility are refreshed through
  `GeneratedTestUnitController.SetCurrentFloorIndex`.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Run `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- Confirm `Generated Test Unit` has `GeneratedStairTransitionAgent`.
- In Play Mode, enter the stair from Floor 01 and confirm the Unit floor changes
  to Floor 02.
- After transition to Floor 02, confirm Floor 02 collider/visibility behavior is
  active and Floor 01 no longer affects the Unit.
- Walk to the opposite stair endpoint and confirm it does not immediately toggle
  back to Floor 01.
- Re-enter the stair from Floor 02 and confirm it can transition back to Floor
  01.
- Code compiles without errors.

## Reset Stage 24E-1 - Test Camera Follow Support

Status: Complete

Goal:

- Let TestScene verification follow the generated Unit like MainScene camera
  behavior.

Output:

- `Assets/___Test/Scripts/Unit/GeneratedTestCameraController.cs`.
- `GeneratedBuildingSceneRoot > Create Or Reset Test Unit` now binds
  `Main Camera` to the generated test Unit.

Runtime rule:

- The camera follows the selected/generated test Unit with `SmoothDamp`.
- First target binding snaps to the Unit immediately.
- The script is isolated to `Assets/___Test` and does not depend on production
  Unit selection systems.

Completion checklist:

- Run `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- Confirm `Main Camera` has `GeneratedTestCameraController`.
- In Play Mode, right-click movement should keep the Unit centered/followed
  smoothly and quickly.
- Code compiles without errors.

## Reset Stage 24F - Playability Validation

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Extend generated building validation from structural correctness to gameplay
  readiness.
- Catch blocked entrances, too-narrow doors, unreachable stairs, missing
  runtime metadata, and invalid generated collider settings before manual play
  testing.

Output:

- `GeneratedBuildingValidator` now includes gameplay-readiness checks.
- `Assets/___Test/Scripts/Validation/GeneratedRuntimeSceneValidator.cs`.
- `GeneratedBuildingSceneRoot > Validate Runtime Scene` context menu.
- `GeneratedGroundGenerator` now logs both data validation and runtime scene
  validation after full generation.

Data validation rules:

- Main entrance openings must be at least 3 contiguous cells.
- Room door openings must be at least 3 contiguous cells.
- Floor connectivity starts from the generated entrance when one exists, not an
  arbitrary public floor cell.
- Rooms must remain reachable from entrance-connected public floor space.
- Lower-floor stair entry must be reachable from public floor space.
- Upper-floor stair hole must be reachable from public floor space.
- Stair entry/exit trigger masks must touch the stair walkable mask.

Runtime scene validation rules:

- Generated collider/trigger objects must have `GeneratedRuntimeIdentity`.
- Collider/trigger identities must have a positive floor index.
- Room identities must have a positive room index.
- Trigger state on `Collider2D` must match `GeneratedRuntimeIdentity`.
- Generated collider tilemaps must have `CompositeCollider2D`.
- Generated `CompositeCollider2D.Geometry Type` must be `Polygons`.
- `GeneratedVisibilityController` and non-empty visibility groups must exist.
- Generated stair zones must include Walkable, Entry, Exit, LeftRail, and
  RightRail zones.
- Entry and Exit stair zones must contain valid source/destination floor
  metadata.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Confirm Console logs two validation results:
  `Generated building validation passed` and runtime scene validation passed.
- Run `GeneratedBuildingSceneRoot > Validate Runtime Scene` manually and confirm
  no errors.
- If validation fails, fix the generator before moving to city generation.
- Code compiles without errors.

## Reset Stage 25 - Generated Building Instance Data

Status: Complete

Goal:

- Expose city-ready metadata from the generated house.
- Keep city placement data independent from visual Tilemap objects.
- Prepare the next step where a city generator can place houses by footprint
  and connect roads to entrances.

Output:

- `Assets/___Test/Scripts/Data/GeneratedBuildingInstanceData.cs`.
- `Assets/___Test/Scripts/Data/GeneratedBuildingInstanceDataView.cs`.
- `GeneratedGroundGenerator` now rebuilds/logs instance data after full
  validation generation.
- `GeneratedBuildingDataView > Rebuild Instance Data` context menu.

Instance data fields:

- Instance id.
- Building id.
- Profile id.
- Display name.
- Seed.
- Deterministic hash.
- Footprint.
- Occupied bounds.
- Floor count.
- Room count.
- Stair count.
- Entrance count.
- Has stairs flag.
- Primary entrance id/kind/floor/cell/side.
- Primary road connection cell.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Confirm Console logs `Generated Building Instance Data`.
- Confirm `Building Generator` has `GeneratedBuildingInstanceDataView`.
- Confirm footprint and road connection cell are available for future city
  placement.
- Code compiles without errors.

## Reset Stage 26A - Architecture Rule Audit

Status: Complete

Goal:

- Separate generic building rules from House01 reference rules.
- Identify generation assumptions that block additional building families before
  city generation begins.

Output:

- `Assets/___Test/Docs/ArchitectureRuleAudit.md`.
- `Assets/___Test/Docs/BuildingGeneralizationPlan.md` now starts from Stage
  26B.
- `Assets/___Test/Docs/RoadmapReset.md` marks Stage 26A complete.

Audit result:

- The generator is not locked to one TestScene result because profiles,
  palettes, stair data, validation, runtime metadata, and instance data exist.
- The generator is still House01-style because scene binding, stair generation,
  room layout, doors, entrances, and rectangular footprint rules contain
  assumptions that are too narrow for city-scale variety.
- The highest-priority blocker is fixed Floor01/Floor02 scene binding.

Classified rule groups:

- Generic rules: generated data before rendering, separated visual/collider
  tilemaps, validation gate, runtime identities, visibility groups, seed
  reproducibility.
- Profile data: footprint, floor count, room count/size, ceiling offset, entry
  side, grid, sorting, stair usage.
- Palette data: floor, wall, room wall, door, stair, ceiling, collision tiles.
- Preset data: stair module masks, rail data, trigger ranges, fine grid.
- House01-like assumptions: fixed two floor roots, one Floor01-to-Floor02
  stair, simple X-axis room sectors, one centered main entrance, hard-coded
  door/entrance width, rectangular footprint.

Next stage:

- Stage 26B - Dynamic Floor Scene Binding.

Completion checklist:

- Audit document exists.
- A clear extraction list exists.
- No city generation work starts yet.
- Code compiles without errors.

## Reset Stage 26B - Dynamic Floor Scene Binding

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Remove Floor01/Floor02-only assumptions from scene binding and generation.
- Make generated floors resolve through floor index so later profiles are not
  blocked by fixed scene fields.

Output:

- `GeneratedBuildingSceneRoot` now provides:
  - `GetFloorRoot`.
  - `EnsureFloorRoot`.
  - `EnsureFloorVisualRoot`.
  - `EnsureFloorRoomRoot`.
  - `EnsureFloorVisualTilemap`.
  - `FindFloorVisualTilemap`.
  - floor naming helpers.
- `GeneratedGroundGenerator` now paints floors by looping over
  `buildingData.Floors`.
- `GeneratedColliderRenderer` now paints colliders by looping over generated
  floors.
- `GeneratedVisibilityBinder` now binds floor visibility groups by looping over
  generated floors.
- `GeneratedRuntimeBinder` now resolves floor roots through
  `GeneratedBuildingSceneRoot.GetFloorRoot`.
- `GeneratedStairPlanner` now creates stair data for adjacent floor pairs
  instead of assuming only Floor 01 to Floor 02.
- `GeneratedBuildingValidator` now validates stair connections over adjacent
  floor pairs.
- Large/Small profile ceiling offsets were corrected by one tile to match the
  manually adjusted `Ceiling Grid` reference in `TestScene`.
- Upper-floor outer wall corner generation now avoids the overlapping Upper
  East corner tile that made Floor 02 exterior corners look unnatural.
- Generated room doors now create 3-cell openings.
- Generated main building entrances now create 3-cell openings.
- Gameplay validation now requires at least 3 contiguous cells for both room
  doors and main entrances.

Compatibility note:

- `floor01Root` and `floor02Root` remain on `GeneratedBuildingSceneRoot` only to
  preserve the current TestScene Inspector bindings.
- New logic no longer depends on direct `GetFloor(1)` / `GetFloor(2)` rendering
  paths.

Completion checklist:

- Run `Generate Building With Stairs` using `House01LikeProfile`.
- Run `Generate Building With Stairs` using `SmallHouseProfile`.
- Run `Generate Building With Stairs` using `LargeHouseProfile`.
- Confirm each generated profile passes structural/runtime validation.
- Confirm switching from a 2-floor profile to `SmallHouseProfile` clears stale
  Floor 02 tilemaps.
- Code compiles without errors.

## Reset Stage 26C - Reusable Preset Assets

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Move reusable architectural rules out of individual building profiles.
- Keep `GeneratedBuildingProfile` as a composition root that references
  reusable presets.
- Preserve fallback values on profiles for compatibility and quick debugging.

Output:

- `Assets/___Test/Scripts/Data/GeneratedStairModulePresetAsset.cs`.
- `Assets/___Test/Scripts/Data/GeneratedRoomLayoutPresetAsset.cs`.
- `Assets/___Test/Scripts/Data/GeneratedDoorPlacementPresetAsset.cs`.
- `Assets/___Test/Scripts/Data/GeneratedEntrancePlacementPresetAsset.cs`.
- `Assets/___Test/Data/Presets/House01StairModulePreset.asset`.
- `Assets/___Test/Data/Presets/House01RoomLayoutPreset.asset`.
- `Assets/___Test/Data/Presets/SmallHouseRoomLayoutPreset.asset`.
- `Assets/___Test/Data/Presets/LargeHouseRoomLayoutPreset.asset`.
- `Assets/___Test/Data/Presets/CommonDoorPlacementPreset.asset`.
- `Assets/___Test/Data/Presets/CommonEntrancePlacementPreset.asset`.

Generation rule:

- Profiles resolve room layout, door placement, entrance placement, and stair
  module values from assigned preset assets first.
- If a preset reference is empty, the profile-local fallback fields are used.
- Door and entrance widths remain set to `3` through shared presets.
- House01-like and LargeHouse share the same House01 stair module preset.
- SmallHouse also references the stair preset for consistency, but still keeps
  `generateStairs` disabled.

Completion checklist:

- In Unity, select each profile and confirm Room Layout, Door Placement,
  Entrance Placement, and Stair Module preset references are assigned.
- Run `Generate Building With Stairs` for House01-like, SmallHouse, and
  LargeHouse.
- Confirm structural/runtime validation logs pass.
- Confirm generated room doors and the main entrance still report/use 3-cell
  openings.
- Code compiles without errors.

## Reset Stage 26D - Room Layout Strategy Upgrade

Status: Implementation Complete - Unity Confirmation Required

Goal:

- Improve generated room placement beyond simple X-axis sector splitting.
- Keep room generation deterministic by seed.
- Keep generated rooms valid for later Unit traversal and city-scale reuse.

Output:

- Added `GeneratedRoomLayoutStrategy`.
- `GeneratedRoomLayoutPresetAsset` now exposes:
  - `layoutStrategy`.
  - `placementAttemptsPerRoom`.
  - `stairClearanceFromRooms`.
- `GeneratedBuildingProfile` now resolves room strategy values from the room
  layout preset.
- `GeneratedRoomPlanner` now supports Random Packed placement with Sector Split
  fallback.
- `GeneratedBuildingValidator` now validates generated room count against
  profile min/max.

Generation rule:

- Random Packed placement samples multiple candidate room rectangles per room.
- Candidate rooms are scored so rooms naturally spread out instead of always
  lining up in equal X-axis sectors.
- Candidate rooms are rejected if they overlap existing rooms plus configured
  room gap.
- Candidate rooms are rejected if they intersect floor holes, stair visual
  masks, stair entry masks, or stair exit masks with configured stair
  clearance.
- Candidate rooms are rejected if they overlap the configured main entrance
  clearance corridor, so generated rooms do not crowd the first-floor entrance.
- Candidate rooms are rejected if the configured door side has no exterior
  floor clearance.
- Sector Split remains available as a deterministic fallback or explicit
  strategy choice.
- Room collider tilemap roots are cleared before regeneration, preventing stale
  invisible Room Collider Tilemaps from blocking the Unit after room layouts
  change.

Completion checklist:

- Run `Generate Building With Stairs` for House01-like, SmallHouse, and
  LargeHouse.
- Confirm validation passes.
- Confirm rooms are no longer always evenly sliced along the X axis.
- Confirm rooms do not cover the generated stair hole or stair endpoint areas.
- Run seed reproducibility validation and confirm same seed remains stable.
- Code compiles without errors.

## Reset Stage 26E - Runtime Visibility And Floor Flow Stabilization

Status: Complete

Goal:

- Stabilize runtime floor/visibility flow before adding more generation rules.
- Prevent repeated regressions where floor hiding, floor wall fade, room wall
  fade, and ceiling fade overwrite one another.
- Keep checks generation-time or validation-time, not per-frame.

Output:

- `GeneratedVisibilityController` responsibility split was kept:
  - Floor visual hide/show controls only floor ground and room ground tilemaps.
  - Floor visibility groups control outer Upper/Under Wall tilemaps.
  - Room visibility groups control room Upper/Under Wall tilemaps.
  - Ceiling visibility group controls the ceiling tilemap.
- `GeneratedRuntimeSceneValidator` now validates visibility target ownership.
- Runtime scene validation now detects:
  - Duplicate TilemapRenderer targets controlled by multiple visibility groups.
  - Floor visibility groups targeting anything other than outer Upper/Under
    Wall tilemaps.
  - Room visibility groups targeting anything other than room Upper/Under Wall
    tilemaps.
  - Generated Floor runtime identities without matching floor visibility
    groups.
  - Generated Room runtime identities without matching room visibility groups.
  - Missing Test Unit `GeneratedFloorIsolationAgent`,
    `GeneratedVisibilityAgent`, or `GeneratedStairTransitionAgent`.

Efficiency notes:

- Added checks run through existing runtime scene validation paths and do not
  add per-frame hierarchy scans.
- Existing visibility cache still drives per-frame fade updates from cached
  Tilemap references.
- `FindObjectOfType` use remains limited to setup/validation/fallback paths,
  not normal per-frame runtime flow.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Run `GeneratedBuildingSceneRoot > Create Or Reset Test Unit`.
- Run `GeneratedBuildingSceneRoot > Validate Runtime Scene`.
- Confirm House01-like and LargeHouse pass Floor 01 entry, Floor 01 room fade,
  Floor 02 stair transition, Floor 02 outer wall fade, Floor 02 room fade, and
  Floor 02 to Floor 01 return.
- Confirm lower-floor outer walls remain visible when on Floor 02, while
  lower-floor inactive room walls do not draw over the active floor.
- Confirm SmallHouse still passes without stair transition logic.
- Code compiles without errors.

## Reset Stage 26F - Generation Pipeline Maintainability Refactor

Status: Complete

Goal:

- Reduce the risk of spaghetti code before adding more building-family rules.
- Keep the current generated result stable while separating responsibilities.
- Avoid per-frame hierarchy scans or runtime allocation increases.

Output:

- `Assets/___Test/Scripts/Generation/GeneratedGenerationRunSupport.cs`.
- `Assets/___Test/Scripts/Generation/GeneratedEntrancePlanner.cs`.
- `Assets/___Test/Scripts/Validation/GeneratedBuildingGameplayValidator.cs`.

Refactor result:

- `GeneratedGroundGenerator` now delegates scene reference resolving, scene root
  validation, instance-data rebuild, full validation logging, and seed
  reproducibility validation to `GeneratedGenerationRunSupport`.
- Main entrance generation and entrance wall opening cuts now live in
  `GeneratedEntrancePlanner`, preparing Stage 26G door/entrance strategy work.
- Gameplay-readiness checks for entrance width, door width, contiguous opening
  shape, and stair trigger/walkable contact now live in
  `GeneratedBuildingGameplayValidator`.
- `GeneratedRoomPlanner` behavior was intentionally left unchanged during this
  pass so room generation output does not shift during a maintainability-only
  stage.

Efficiency notes:

- The refactor does not add per-frame work.
- Added helper allocations remain generation/validation-time only.
- Public generation commands and context menu names remain unchanged.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Confirm structural/runtime validation still passes.
- Run `GeneratedGroundGenerator > Validate Seed Reproducibility`.
- Confirm House01-like, SmallHouse, and LargeHouse still generate cleanly.
- Code compiles without errors.

## Reset Stage 26G - Door And Entrance Strategy Upgrade

Status: Complete

Goal:

- Make room doors and external building entrances more data-driven before
  building-family validation and city preparation.
- Keep generated road connection metadata independent from Tilemap hierarchy.

Output:

- `GeneratedDoorSideSelectionStrategy`.
- `GeneratedEntrancePositionStrategy`.
- `GeneratedDoorPlacementPresetAsset.sideSelectionStrategy`.
- `GeneratedEntrancePlacementPresetAsset.positionStrategy`.
- `GeneratedEntrancePlacementPresetAsset.sidePaddingFromCorners`.
- `GeneratedEntrancePlacementPresetAsset.roadConnectionOffset`.
- `GeneratedEntranceData.RoadConnectionCell`.

Generation rule:

- Room doors can now use a preset-selected side strategy:
  fixed default side, prefer default then available side, or random available
  side.
- Room candidate validation now checks whether at least one valid door side can
  open onto public floor space.
- The common door preset currently uses `PreferDefaultThenAvailable`, so the
  default East-side door remains preferred but generation can recover if that
  side is not viable.
- Building entrance placement can remain centered or use deterministic
  seed-based side placement.
- The common entrance preset currently remains centered with a 3-cell opening
  and a road connection cell one cell outside the entrance.
- `GeneratedBuildingInstanceData.PrimaryRoadConnectionCell` is now sourced from
  the generated entrance data.
- `GeneratedBuildingSceneRoot` uses generated entrance data for Test Unit spawn
  when available.

Validation rule:

- Entrance openings must still be at least 3 contiguous cells.
- Room door openings must still be at least 3 contiguous cells.
- Entrance road connection cells must be outside the floor bounds.
- Entrance road connection cells must align outward from at least one entrance
  cell.

Efficiency notes:

- Door side checks happen only during generation.
- Candidate room checks avoid random-list allocation unless a final random
  door side selection is actually needed.
- No per-frame runtime work was added.

Completion checklist:

- Run `GeneratedGroundGenerator > Generate Building With Stairs`.
- Confirm building data logs show `road=(...)` for the generated entrance.
- Confirm instance data logs show `Road Connection Cell`.
- Confirm validation passes for House01-like, SmallHouse, and LargeHouse.
- Run seed reproducibility validation.
- Code compiles without errors.

## Reset Stage 26H - Building Family Validation Matrix

Status: Complete

Goal:

- Prove that generation is not only correct for the current House01-like
  profile.
- Validate multiple building families before moving toward city generation.
- Keep validation executable from the TestScene without manually swapping
  profile assets one by one.

Output:

- `Assets/___Test/Data/Profiles/WideHouseProfile.asset`.
- `GeneratedGroundGenerator > Validate Building Family Matrix`.
- `GeneratedGroundGenerator.validationMatrixProfiles` optional override list.

Validation rule:

- The matrix auto-loads `GeneratedBuildingProfile` assets from
  `Assets/___Test/Data/Profiles` when running inside the Unity editor.
- Explicitly assigned `validationMatrixProfiles` are also included.
- Duplicate profile references are ignored.
- Each profile is regenerated into the scene before runtime validation.
- Each profile checks:
  - generated data validation
  - runtime scene validation
  - seed reproducibility
  - generated instance footprint and road connection data

Current matrix coverage:

- `House01LikeProfile`: two-floor House 01-like reference.
- `SmallHouseProfile`: one-floor small house without stairs.
- `LargeHouseProfile`: larger two-floor house.
- `WideHouseProfile`: one-floor wide profile variant.

Efficiency notes:

- Matrix validation runs only from an explicit context menu.
- It does not add per-frame runtime logic.
- The final scene after matrix validation remains on the last sorted profile;
  regenerate a specific profile afterward if needed for visual testing.

Completion checklist:

- Run `GeneratedGroundGenerator > Validate Building Family Matrix`.
- Confirm Console reports `Failed: 0`.
- Confirm House01-like, SmallHouse, LargeHouse, and WideHouse all pass.
- Confirm regular `Generate Building With Stairs` still works for the selected
  profile.
- Code compiles without errors.

Confirmed result after the shared ceiling-alignment update:

- House 01 Like: passed.
- Large House: passed.
- Small House: passed.
- Wide House: passed.
- Data errors: 0 for every profile.
- Runtime errors: 0 for every profile.
- Seed and instance checks: passed for every profile.
- Matrix total: `Passed: 4`, `Failed: 0`.

## 2026-07-10 Roadmap And Architecture Reaudit

Status: Complete

Result:

- Stage 26B and 26D behavior has been exercised by later multi-profile and
  gameplay work; the active roadmap treats them as complete.
- Stage 26H matrix rerun passed after the ceiling alignment rule moved from
  duplicated building-profile values to the shared tile palette.
- The project is classified as a functional building-generator prototype, not
  a city-ready runtime system.
- Added Stage 27 building stabilization gates for automated regression,
  responsibility splitting, runtime registries, batched Tilemap rendering,
  memory/data lifetime, and gameplay regression.
- Added Stage 28 building grammar/catalog/output-policy work.
- Added Stage 29 city contracts, placement, roads/districts, streaming, and
  city validation.
- Added Stage 30 navigation, spawn, persistence, and gameplay-world integration.

Primary risks recorded:

- Global identity searches during floor isolation will not scale to multiple
  buildings and Units.
- Cell-by-cell Tilemap painting and duplicated serialized cell masks require a
  measured optimization pass before runtime city generation.
- Large orchestration/planner/validator classes need further responsibility
  splitting before more grammar strategies are added.
- City instance data still lacks world placement, orientation, district type,
  clearance, versioning, and full road-socket contracts.

The active source of truth is now
`Assets/___Test/Docs/BuildingGeneralizationPlan.md`.

## Stage 27A - Automated Building Regression Tests

Status: Complete

Output:

- `Assets/___Test/Scripts/ZombieTown.GeneratedBuilding.asmdef`.
- `Assets/___Test/Scripts/Generation/GeneratedBuildingDataGenerator.cs`.
- `Assets/___Test/Tests/EditMode/ZombieTown.GeneratedBuilding.Tests.asmdef`.
- `Assets/___Test/Tests/EditMode/GeneratedBuildingRegressionTests.cs`.
- `Assets/___Test/Tests/EditMode/GeneratedBuildingRegressionTestRunner.cs`.

Implementation result:

- Extracted a data-only full-generation entry point from the scene generator.
- Existing `GeneratedGroundGenerator` delegates ground/full data generation to
  the same API used by tests, preventing test-only generation behavior.
- Added `20` profile/seed full-generation validation cases.
- Added same-seed reproduction, per-profile different-seed variation, instance
  metadata, ceiling rule, and profile catalog checks.
- Total expected Edit Mode tests: `25`.
- Tests do not mutate or require manual setup of TestScene.

Confirmation:

- Refresh Unity.
- Press `F8`, or select
  `Tools > Zombie Town > Run Building Regression Tests`.
- Confirm the Console summary reports `Failed: 0`.

Confirmed result:

- Passed: `25`.
- Failed: `0`.
- Skipped: `0`.
- Inconclusive: `0`.

## Stage 27B - Generation Responsibility Split

Status: Implementation Complete - Unity Console Confirmation Available

Output:

- `Assets/___Test/Scripts/Rendering/GeneratedBuildingSceneRenderer.cs`.
- `Assets/___Test/Scripts/Validation/GeneratedBuildingFamilyValidationRunner.cs`.
- `Assets/___Test/Scripts/Generation/GeneratedRoomPlacementStrategy.cs`.
- `Assets/___Test/Tests/EditMode/GeneratedBuildingRegressionTestRunner.cs`.

Implementation result:

- Data-only generation remains in `GeneratedBuildingDataGenerator`.
- Scene Tilemap rendering orchestration moved behind
  `GeneratedBuildingSceneRenderer`.
- Building-family matrix execution moved behind
  `GeneratedBuildingFamilyValidationRunner`.
- Room placement now uses a stable strategy interface with random-packed and
  sector placement implementations.
- `GeneratedGroundGenerator` is reduced to editor-facing orchestration and
  profile command entry points.
- Added `F9` menu execution for the family matrix:
  `Tools > Zombie Town > Validate Building Family Matrix`.

Verification:

- `dotnet build ZombieTown.GeneratedBuilding.csproj`: zero warnings, zero
  errors.
- `dotnet build ZombieTown.GeneratedBuilding.Tests.csproj`: zero warnings,
  zero errors.
- Unity imported the new files and completed script compilation/domain reload
  without new `error CS` entries in the latest log.

Confirmation:

- Press `F8`, or select
  `Tools > Zombie Town > Run Building Regression Tests`; confirm `Failed: 0`.
- Press `F9`, or select
  `Tools > Zombie Town > Validate Building Family Matrix`; confirm `Failed: 0`.

## 2026-07-14 Post-27B Roadmap Reaudit

Status: Complete

Verified:

- Runtime and test C# projects build with zero warnings and zero errors.
- Stage 27A remains historically confirmed at `25/25` tests.
- Stage 27B services and strategy files are present and compile.
- `GeneratedGroundGenerator` is now a 358-line editor command facade rather
  than the previous monolithic generator.

Risks that block city work:

- Floor isolation still scans every `GeneratedRuntimeIdentity` in the Scene.
- Visibility still has a global controller fallback.
- Runtime identities do not yet carry a placement-unique building instance id.
- TestScene has not proven two buildings or multiple Units without cross-talk.
- Runtime gameplay behavior is not yet covered by repeatable Play Mode tests.

Roadmap change:

- Split Stage 27C into instance registry, Unit context migration, and
  multi-building isolation tests.
- Split Stage 27D into rendering/cleanup and data-lifetime/profiling work.
- Split Stage 27E into automated gameplay scenarios and final visual acceptance.
- The immediate implementation start is Stage 27C-A after one current `F8` and
  `F9` confirmation run.

## Stage 27C-A - Building Instance Ownership And Registry

Status: Complete

Output:

- `Assets/___Test/Scripts/Scene/GeneratedBuildingRuntimeRegistry.cs`.
- Updated `GeneratedBuildingSceneRoot`, `GeneratedRuntimeIdentity`, and
  `GeneratedRuntimeBinder`.
- Three ownership/regeneration regression tests in
  `GeneratedBuildingRegressionTests`.

Implementation result:

- Every generated building placement receives a GUID independent of profile
  id and seed.
- Copied roots with a duplicated serialized GUID are detected and separated
  before runtime binding.
- One registry on the generated house root owns the current binding revision.
- Identities, colliders, triggers, floors, rooms, stairs, and the visibility
  controller are cached only when their instance id, registry, and revision
  match.
- Regeneration clears caches first, advances the revision, rebinds current
  objects, and excludes stale identities from all registry lookups.
- Stair triggers and rail colliders are included in building ownership.

Confirmed result:

- `dotnet build ZombieTown.GeneratedBuilding.csproj`: zero warnings, zero
  errors.
- `dotnet build ZombieTown.GeneratedBuilding.Tests.csproj`: zero warnings,
  zero errors.
- Unity script compilation: successful with no current `error CS` entries.
- Unity `F8`: passed `28`, failed `0`, skipped `0`, inconclusive `0`.

Next:

- Stage 27C-B - Unit Building Context And Floor Isolation.
- Stage 27C-C - Multi-Building Runtime Isolation Tests.

## Post-27C-A Outer Corner And Ceiling Visual Correction

Status: Complete

Fixes:

- Removed the Floor 01-only condition from the southeast outer-corner rule, so
  every generated floor now receives all four secondary corner tiles.
- `GeneratedCeilingRenderer` originally used a fixed `(0, 1.5, 0)` ceiling
  position, which matched two-floor profiles but left one-floor roofs floating.
- Removed the fixed palette-level ceiling height. The renderer now derives the
  ceiling Root position from the generated building's top floor:
  `(floorCount - 1) * floorVisualOffsetStep`.
- One-floor, two-floor, and three-floor buildings therefore resolve to `0`,
  `1.5`, and `3.0` respectively with the current profile step.

Verification:

- Every full-generation profile/seed regression case now asserts four exact
  outer-corner cells on every floor.
- Added explicit one-floor, two-floor, and three-floor ceiling-position test
  cases, plus per-profile top-floor alignment assertions.
- Runtime and test C# builds: zero warnings and zero errors.

## Post-27C-A Stair Runtime Ownership Validation Fix

Status: Complete

Problem:

- Entry and Exit stair zones have explicit source floors, but shared Walkable,
  Left Rail, and Right Rail zones intentionally keep `SourceFloorIndex = 0`.
- The runtime binder copied that zero into collider/trigger identities, causing
  runtime-scene validation to reject the three shared stair Tilemaps.

Fix:

- Transition zones retain their explicit source floor.
- Shared stair zones use the stair's lower floor as their runtime ownership
  floor, so every collider/trigger identity has a valid positive floor index.
- Added an Edit Mode regression test covering Walkable, both Rails, Entry, and
  Exit ownership assignments.

Verification:

- Unity `F8`: passed `29`, failed `0`, skipped `0`, inconclusive `0`.
- Runtime and test C# builds: zero warnings and zero errors.

## Stage 27C-B - Unit Building Context And Floor Isolation

Status: Complete

Output:

- `Assets/___Test/Scripts/Unit/GeneratedBuildingContextAgent.cs`.
- Updated `GeneratedFloorIsolationAgent`, `GeneratedVisibilityAgent`,
  `GeneratedTestUnitController`, `GeneratedStairTransitionAgent`, and test-Unit
  setup.

Implementation result:

- A Unit explicitly owns at most one current building registry.
- Owned floor/room/stair triggers acquire and retain context until the last
  owned trigger exits.
- Another building cannot replace an active context.
- Floor collision isolation touches only colliders cached by the current
  registry and restores changed pairs on context exit/disable.
- Visibility uses only the current registry controller.
- Global identity scans and global visibility-controller fallback were removed
  from recurring Unit runtime paths.

Automated verification:

- Unity `F8`: passed `30`, failed `0`, skipped `0`, inconclusive `0`.
- Runtime and test C# builds: zero warnings and zero errors.
- Static search: no `FindObjectsOfType<GeneratedRuntimeIdentity>` and no global
  `FindObjectOfType<GeneratedVisibilityController>` remain.

Manual confirmation required:

- Enter/exit building context in Play Mode.
- Floor 1/2 collision isolation and stair round trip.
- Ceiling/floor/room visibility response and full restoration after exit.

Completion confirmation:

- Stage 27C-C now supplies the repeatable two-building/two-Unit Play Mode
  isolation gate for the 27C-B runtime context implementation.
- Unity Edit Mode result: passed `33`, failed `0`.
- Unity Play Mode isolation result: passed `1`, failed `0`.

## Stage 27C-C - Multi-Building Runtime Isolation Tests

Status: Complete

Output:

- `Assets/___Test/Tests/PlayMode/GeneratedMultiBuildingIsolationPlayModeTests.cs`.
- `Assets/___Test/Tests/PlayMode/ZombieTown.GeneratedBuilding.PlayModeTests.asmdef`.
- `F6` runner under `Tools > Zombie Town > Run Multi-Building Play Mode Tests`.

Scenario coverage:

- Two buildings share logical building/profile ids while retaining different
  runtime instance ownership.
- Two Units hold different registry and floor contexts concurrently.
- A different building cannot steal an active Unit context.
- Each Unit changes collision pairs only inside its own registry.
- One Unit's stair transition does not change the other Unit or building.
- Context exit and Unit disable restore collision and visibility ownership.
- The test harness runs outside existing TestScene physics and requires no
  manual hierarchy editing.

Confirmed result:

- Play Mode: passed `1`, failed `0`, skipped `0`, inconclusive `0`.
- Runtime, Edit Mode test, and Play Mode test projects: zero warnings and zero
  errors.

Next:

- Stage 27D-A - Batched Rendering And Regeneration Cleanup.

## Stage 27D-A - Batched Rendering And Regeneration Cleanup (2026-07-14)

Status: Complete

Implemented:

- Replaced cell-by-cell production painting with batched `Tilemap.SetTiles` calls.
- Added a legacy painting switch used by the performance comparison only.
- Regeneration now removes obsolete floor roots instead of leaving cleared
  Tilemaps with stale colliders, identities, visibility groups, or registry data.
- Regeneration removes obsolete visual room groups and rebuilds collider room
  groups without leaving attached Play Mode objects behind.
- One-floor/no-stair regeneration removes old stair visual/collider/trigger roots
  and `GeneratedStairZone` components.
- Added `Profile 27D-A Rendering` to record per-profile and four-profile batch
  elapsed time and thread allocation totals for legacy and batched rendering.
- Added the TestScene validation matrix: Small, House01Like, Wide, and Large.
- Added an Edit Mode batched-output equivalence regression test.
- Added a Play Mode harness Camera so F6 no longer emits irrelevant camera warnings.

Automated build result:

- Runtime, Edit Mode test, and Play Mode test projects: zero warnings and zero
  errors.

Unity verification:

- Multi-building Play Mode `F6`: `1/1` passed with zero warnings and errors.
- Legacy four-profile batch: `168.700 ms`.
- Batched four-profile batch: `108.478 ms`.

## Stage 27D-B - Generated Data Lifetime And Runtime Budgets (2026-07-14)

Status: Complete

Implemented:

- Classified authoring, transient generation, compact runtime, and persistent
  save data in `GeneratedDataLifetimeAndBudgets.md`.
- Full generation masks are released automatically after Play Mode binding and
  validation; debug retention remains explicitly opt-in.
- Compact instance metadata and per-building registry data remain available.
- Test-unit entrance fallback no longer requires full generated masks/data.
- Added `Profile 27D-B Runtime Budgets` for measured managed-memory release,
  Tilemap/collider/active-object counts, and visibility-update cost.
- Defined per-building guardrails and a 16-building active chunk budget.
- Added a regression test proving compact metadata survives transient release.

Automated build result:

- Runtime, Edit Mode test, and Play Mode test projects: zero warnings and zero
  errors.

Unity verification required:

- Edit Mode `F8`: expected `35/35` passed.
- Run `Profile 27D-B Runtime Budgets`; all four profile lines must report
  `withinBudget=True` and Console must have zero warnings/errors.

Confirmed result:

- Edit Mode `F8`: `35/35` passed.
- Four profile budget samples and the matrix total were recorded with zero
  warnings and errors.

## Stage 27E-A - Automated Building Gameplay Matrix (2026-07-14)

Status: Implementation Complete - Edit Mode Confirmation Pending

Implemented:

- Added 20 profile/seed gameplay data cases across all supported profiles.
- Validates door and entrance clearance, including the 0.5-unit test collider,
  and rejects stale floor 2/stairs for one-floor profiles.
- Added a Play Mode scenario for entry, room entry, floor collision, stair up and
  return, wall/room/ceiling fade, fade restoration, and camera following.
- Existing two-building/two-Unit ownership isolation remains in the same suite.
- Failure messages carry profile, seed, floor, room, and scenario context.

Automated build result:

- Runtime, Edit Mode test, and Play Mode test projects: zero warnings and zero
  errors.

Unity verification required:

- Edit Mode `F8`: expected `55/55` passed.
- Play Mode `F6`: `2/2` passed with zero warnings and errors.

## Stage 27E-B - Visual Acceptance And Building Stability Gate (2026-07-14)

Status: Implementation Complete - Visual Acceptance Pending

Implemented:

- Added deterministic visual samples for all four profiles at Seeds `1001` and
  `2027`.
- Added context-menu actions to prepare, structurally validate, accept, advance,
  summarize, and reset visual acceptance samples.
- Each prepared sample creates/resets the test Unit and prints the seven visual
  inspection groups in Console.
- Acceptance progress is serialized in TestScene.
- Added `Stage27EBVisualAcceptanceChecklist.md` with rejection and completion
  rules.

Automated build result:

- Runtime, Edit Mode test, and Play Mode test projects: zero warnings and zero
  errors.

Manual verification required:

- Complete all eight samples and confirm `Accepted: 8/8`, `Complete: True`.
- Edit Mode `F8` must also report `55/55` before Stage 27 can close.
