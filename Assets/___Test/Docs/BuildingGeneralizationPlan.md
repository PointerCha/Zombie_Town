# Active Building And City Generation Roadmap

Last audit: 2026-07-14

This is the active source of truth for upcoming work. `RoadmapReset.md` and
`StageProgress.md` remain implementation history.

## Final Goal

Build a profile/preset-driven procedural pipeline that can:

- generate playable and visually stable houses from configured tile sets;
- validate every generated structure before it is accepted;
- reproduce results from a seed;
- expose compact placement data without inspecting generated Tilemap children;
- place building families by district and connect entrances to roads;
- stream, cache, save, and reload a city without regenerating everything every
  frame or keeping every building active.

`MainScene/House 01` is a reference, not a runtime dependency. New work remains
inside `Assets/___Test` until the prototype is explicitly promoted.

## Current Assessment

The project is a functional building-generator prototype, but it is not yet a
city-ready production system.

Confirmed strengths:

- Four building profiles share the same generation pipeline.
- Floors, outer walls, rooms, doors, entrances, stairs, ceilings, colliders,
  triggers, visibility metadata, and runtime identities are generated.
- One-floor and two-floor Unit gameplay paths have been exercised.
- Seed reproducibility, structural validation, connectivity validation, and
  runtime-scene validation exist.
- City-facing instance data exposes footprint, entrance direction, and a road
  connection cell.
- Ceiling alignment is now owned once by the tile palette and validated against
  generated bounds.
- The current C# project compiles with zero warnings and zero errors.

Current gate:

- Stage 27A automated building regression tests remain green and now include
  the Stage 27C-A ownership cases (`28/28` passing).
- Stage 27B responsibility splitting is implemented. Local C# builds for
  `ZombieTown.GeneratedBuilding` and `ZombieTown.GeneratedBuilding.Tests`
  currently report zero warnings and zero errors.
- Stage 27C-A building-instance ownership and the per-building runtime registry
  are complete. Stage 27C-B Unit building context is the current gate.
- City work must still wait for the complete Stage 27 building stability gate.

## Audit Findings

### Maintainability

- `GeneratedGroundGenerator` is now 358 lines and delegates data generation,
  rendering orchestration, and family-matrix execution. It remains the
  editor-facing command facade, which is an acceptable responsibility.
- `GeneratedBuildingValidator` is 906 lines and mixes several validation
  domains.
- `GeneratedRoomPlanner` is 872 lines. Room placement strategies are separated,
  but candidate validation, door viability, and room registration still need
  further extraction before adding substantially more grammar rules.
- The 25-case Edit Mode suite covers deterministic data generation. Runtime
  Unit, stair, collision, and visibility behavior still lacks repeatable Play
  Mode coverage.

### Runtime And CPU

- `GeneratedFloorIsolationAgent` performs global `FindObjectsOfType` scans and
  repeated component lookups whenever floor isolation refreshes. This is
  acceptable for one test house but unsafe for a city with many buildings and
  Units.
- `GeneratedVisibilityAgent` has a global controller fallback. With multiple
  generated houses it can bind to the wrong building even when floor filtering
  itself is correct.
- Tile rendering uses cell-by-cell `Tilemap.SetTile`. It is acceptable for the
  current editor prototype but should become batched painting before runtime
  city generation.
- Visibility fading updates only cached groups and is acceptable at the current
  scale. It still requires a multi-building profiler check.
- Generation and validation allocations are mostly outside per-frame gameplay,
  which is currently the correct tradeoff.

### Memory And Data Lifetime

- Ground, wall, collision, trigger, stair, and room masks store many individual
  serialized cell objects. This is convenient for debugging but duplicates data
  that Tilemaps also hold.
- The project has not decided which data is transient generation data, which is
  compact runtime metadata, and which must be saved permanently.
- Generated building instances have no schema version or migration policy.

### City Readiness

- Runtime identities currently carry a profile-level building id, while the
  generated instance id is derived from building id and seed. Neither contract
  safely distinguishes two placements of the same profile and seed.
- `TestScene` proves one generated root only; it does not yet prove two nearby
  buildings or multiple Units without collision and visibility cross-talk.
- Instance data is local building data only. It lacks world placement,
  orientation, category, district tags, placement clearance, and multiple road
  sockets.
- Building footprints are rectangular and entrance/stair/module variety is
  still limited.
- There is no building catalog, placement solver, road graph, district rule,
  streaming policy, pooling policy, or city-level validation yet.

### Additional Goals That Must Not Be Missed

- Automated regression tests and deterministic failure reproduction.
- Profile/preset authoring validation before generation.
- Save/load schema versioning and migration.
- Navigation/pathfinding and NPC spawn/accessibility metadata.
- Generated-object ownership, cleanup, pooling, and streaming.
- Explicit CPU, memory, and active-object budgets measured in the Unity
  Profiler rather than guessed.
- Debug visualization for footprints, entrances, road sockets, room
  connectivity, and invalid placement reasons.

## Revised Stage Order

### Stage 26H - Building Family Matrix Reconfirmation

Status: Complete

Work:

- Run `Validate Building Family Matrix` after the ceiling alignment change.
- Validate House01Like, SmallHouse, LargeHouse, and WideHouse.
- Regenerate each profile once and inspect ceiling, entrance, room wall,
  collider, visibility, and stair output.

Completion:

- Matrix reports `Failed: 0`.
- No generated object requires a manual correction after regeneration.
- Ceiling bounds validation passes for every profile.
- Build has zero warnings and zero errors.

Confirmed result:

- Profiles: 4.
- Passed: 4.
- Failed: 0.
- Every profile reported zero data/runtime errors, valid seed reproduction, and
  valid instance data after the ceiling-alignment update.

### Stage 27A - Automated Building Regression Tests

Status: Complete

Work:

- Add Edit Mode tests using the installed Unity Test Framework.
- Test all profiles across a fixed seed set.
- Test same-seed determinism, different-seed validity, room reachability,
  openings, stairs, ceiling bounds, and instance metadata.
- Keep visual Play Mode checks as a smaller explicit checklist.

Completion:

- One command runs the structural regression suite.
- A failed seed and profile are printed clearly and can be reproduced.
- Tests do not require hand-editing TestScene.

Implemented coverage:

- Four discovered building profiles.
- Five fixed seeds per profile (`20` full-generation cases).
- Same-seed fingerprint and hash reproduction.
- Different-seed variation for every profile (`4` cases).
- Structural/gameplay validation with zero warnings.
- Floor, entrance, instance footprint, road metadata, and ceiling placement
  rule checks.
- Unique and valid profile id catalog check (`1` case).
- Total expected test cases: `25`.

Run:

- Refresh Unity after script changes.
- Press `F8`, or use `Tools > Zombie Town > Run Building Regression Tests`.
- Confirm `Generated Building Regression Test Result` reports `Failed: 0`.

Confirmed result:

- `Generated Building Regression Test Result`
- Passed: `25`
- Failed: `0`
- Skipped: `0`
- Inconclusive: `0`

### Stage 27B - Generation Responsibility Split

Status: Implementation Complete - Unity Console Confirmation Available

Work:

- Extract data-only building generation from `GeneratedGroundGenerator`.
- Split shell/corner planning, render orchestration, family-matrix execution,
  and validation domains into focused services.
- Split room placement strategies behind a stable strategy interface.
- Keep existing profile output stable unless a documented rule changes.

Completion:

- Data generation can run without a Scene hierarchy.
- Renderer consumes generated data but does not decide architecture.
- Validators are grouped by structural, connectivity, runtime, and city-facing
  concerns.
- Existing deterministic tests remain green.

Implemented:

- `GeneratedBuildingDataGenerator` owns data-only generation.
- `GeneratedBuildingSceneRenderer` owns scene Tilemap rendering orchestration.
- `GeneratedBuildingFamilyValidationRunner` owns family-matrix execution.
- `GeneratedRoomPlacementStrategy` separates room placement strategy selection
  from room data registration.
- `GeneratedGroundGenerator` now acts mostly as an editor-facing orchestration
  component instead of owning every generation responsibility.
- `Tools > Zombie Town > Run Building Regression Tests` (`F8`) runs the Edit
  Mode regression suite.
- `Tools > Zombie Town > Validate Building Family Matrix` (`F9`) runs the
  profile family matrix from the active `GeneratedGroundGenerator`.

Latest local verification:

- `dotnet build ZombieTown.GeneratedBuilding.csproj`: zero warnings, zero
  errors.
- `dotnet build ZombieTown.GeneratedBuilding.Tests.csproj`: zero warnings, zero
  errors.
- Unity imported the new 27B files and performed a script compilation/domain
  reload without `error CS` entries in the latest log.

Manual Unity confirmation:

- Press `F8` and confirm `Failed: 0`.
- Press `F9` and confirm the family matrix reports `Failed: 0`.

### Stage 27C-A - Building Instance Ownership And Registry

Status: Complete

Work:

- Define a unique runtime instance id that is not inferred only from profile id
  and seed.
- Add a registry owned by each generated building root.
- Register generated identities, colliders, triggers, visibility controller,
  floors, and rooms during binding.
- Rebuild the registry atomically after regeneration and remove stale entries.

Completion:

- Two placements of the same profile and seed have different instance ids.
- Registry lookups return only objects owned by that building instance.
- Regeneration leaves one valid registry with no stale object references.

Implemented:

- `GeneratedBuildingSceneRoot` owns a serialized placement-unique GUID and
  detects/replaces duplicated serialized ids when a building root is copied.
- `GeneratedBuildingRuntimeRegistry` is created once on each generated house
  root and rebuilt through revisioned `BeginRebuild`/`CompleteRebuild` phases.
- Runtime identities carry building instance id, building/profile ids, binding
  revision, and their owning registry.
- Floor, room, collider, trigger, ceiling, stair, and visibility-controller
  references are cached only when their ownership and revision match.
- Stair walkable/entry/exit triggers and rail colliders participate in runtime
  ownership instead of remaining outside the registry.
- Rebinding reuses one registry component and excludes identities left on an
  older binding revision.

Automated verification:

- Added duplicate-placement id, stale-revision, and atomic-rebinding Edit Mode
  regression tests.
- Unity `F8`: `28/28` passed, `0` failed, `0` skipped, `0` inconclusive.
- Runtime and test `.csproj` builds: zero warnings and zero errors.

### Stage 27C-B - Unit Building Context And Floor Isolation

Status: Complete

Work:

- Bind each Unit to an explicit current building registry while it is inside a
  generated building, and clear that context on exit.
- Replace global floor-isolation scans with cached registry collider lists.
- Resolve room, floor, stair, and visibility behavior through the Unit's current
  building context.
- Remove global controller fallbacks from recurring runtime paths.

Completion:

- Floor changes perform no `FindObjectsOfType` scan.
- Collision and visibility changes affect only the Unit's current building.
- Entering, leaving, changing floor, and disabling the Unit restore collision
  pairs and visibility registrations correctly.
- Floor changes allocate no recurring managed garbage after initialization.

Implemented:

- Added `GeneratedBuildingContextAgent` to acquire one explicit building
  registry from owned runtime triggers and clear it after the last owned
  trigger exits.
- A Unit already inside one building rejects trigger context from a different
  building until the current context is cleared.
- `GeneratedFloorIsolationAgent` now iterates only the current registry's
  cached collider array and restores only collision pairs it changed.
- `GeneratedVisibilityAgent` resolves only the current registry's visibility
  controller; the global controller fallback was removed.
- Unit disable/context exit restores collision pairs and clears visibility
  registration/context state.
- Recurring Unit floor/visibility paths contain no global generated-identity
  or visibility-controller searches.

Automated verification:

- Added registry A/B context, collision isolation, visibility selection,
  cross-building rejection, and context cleanup assertions.
- Current Unity `F8`: `33/33` passed, `0` failed, `0` skipped, `0` inconclusive.
- Runtime and test `.csproj` builds: zero warnings and zero errors.

Manual gate:

- Replaced by the repeatable Stage 27C-C Play Mode isolation scenario, which
  confirms registry acquisition/clear, floor collision behavior, stair
  transition isolation, and cleanup with two buildings and two Units.

### Stage 27C-C - Multi-Building Runtime Isolation Tests

Status: Complete

Work:

- Add a deterministic test harness with two nearby generated buildings,
  including two instances that may share profile and seed.
- Exercise at least two Units independently across entrance, room, stair, and
  exit paths.
- Add Play Mode assertions for ownership, collider isolation, trigger routing,
  and visibility-controller selection.

Completion:

- Two buildings never change each other's collisions, triggers, floor state,
  or visibility.
- Two Units can occupy different floors/buildings without shared state.
- The isolation tests are repeatable without hand-editing `TestScene`.

Implemented:

- Added a dedicated Play Mode test assembly that creates two runtime building
  registries with the same building/profile ids but different instance ids.
- Creates two Units and holds them in different building/floor contexts.
- Verifies context theft rejection, per-building collider isolation, visibility
  controller selection, one-Unit stair transition, context exit cleanup, and
  disabled-Unit collision restoration.
- The harness is positioned outside existing TestScene physics so the scenario
  is deterministic and independent of hand-authored scene state.
- Added `Tools > Zombie Town > Run Multi-Building Play Mode Tests` (`F6`).

Confirmed result:

- Edit Mode `F8`: `33/33` passed, `0` failed.
- Multi-building Play Mode `F6`: `1/1` passed, `0` failed.
- Runtime, Edit Mode test, and Play Mode test projects build with zero warnings
  and zero errors.

### Stage 27D-A - Batched Rendering And Regeneration Cleanup

Status: Complete

Work:

- Replace cell-by-cell Tilemap painting with batched APIs where practical.
- Make full regeneration idempotent for Tilemaps, colliders, identities,
  visibility groups, and runtime registries.
- Measure generation time and allocations before and after batching.

Completion:

- Small, large, and multi-building batch generation have recorded timing and
  allocation baselines.
- Regeneration cleanup leaves no stale Tilemaps, colliders, components, or
  registry entries.
- Batched output matches deterministic data and existing visual bounds.

Implemented:

- Tile painting uses `Tilemap.SetTiles` batches, with a legacy comparison path
  retained only for the 27D-A profiler.
- Full regeneration removes inactive floors, obsolete visual/collider room roots,
  and stair Tilemaps/zones when the new profile has no stairs.
- `GeneratedGroundGenerator > Profile 27D-A Rendering` records legacy and
  batched elapsed time/allocation samples for each matrix profile and the full
  multi-building batch.
- Edit Mode regression coverage compares legacy and batched tiles and bounds.

Verified:

- Multi-building Play Mode `F6`: `1/1` passed with zero warnings and errors.
- Four-profile legacy batch: `168.700 ms`.
- Four-profile 27D-A batched batch: `108.478 ms`.
- The Console retained all per-profile time/allocation samples as the
  machine-specific baseline.

### Stage 27D-B - Generated Data Lifetime And Runtime Budgets

Status: Complete

Work:

- Classify generated data as authoring, transient generation, compact runtime,
  or persistent save data.
- Avoid retaining duplicate full-cell masks after rendering when gameplay does
  not need them.
- Capture retained memory, Tilemap count, collider count, active-object count,
  and visibility-update cost in the Unity Profiler.
- Define explicit per-building and active-city-chunk budgets.

Completion:

- Performance and memory budgets are measured and documented rather than
  estimated.
- Runtime metadata remains available after transient generation data is
  released.

Implemented:

- Full cell masks are classified as transient generation data and automatically
  released after full Play Mode binding/validation unless debug retention is
  explicitly enabled.
- Compact instance metadata and the runtime registry remain available after
  release; test-unit entrance fallback now reads compact metadata.
- `Profile 27D-B Runtime Budgets` measures released managed bytes, retained heap
  snapshot, Tilemaps, colliders, active objects, and visibility update cost for
  every validation profile.
- Initial per-building guardrails are 64 Tilemaps, 64 colliders, 256 active
  objects, and 0.25 ms average visibility update cost. A 16-building active
  chunk budget is derived explicitly from these limits.
- See `GeneratedDataLifetimeAndBudgets.md` for ownership and lifetime rules.

Verified:

- Edit Mode `F8`: `35/35` passed with zero warnings and errors.
- All four runtime-budget profile samples and the measured matrix total were
  recorded with zero warnings and errors.

### Stage 27E-A - Automated Building Gameplay Matrix

Status: Implementation Complete - Edit Mode Confirmation Pending

Work:

- Add repeatable Play Mode scenarios for Unit entry, room entry, floor collision,
  stair travel, return travel, wall fade, ceiling fade, and camera following.
- Verify door widths and entrance clearances with the Unit collider.
- Verify one-floor profiles contain no stale second-floor behavior.

Completion:

- Every supported profile passes the automated gameplay scenario matrix.
- Failures report profile, seed, building instance, floor, and scenario.
- No other-floor or other-building collision, trigger, or visibility influence
  occurs.

Implemented:

- Added a 4-profile x 5-seed gameplay data matrix covering validation, entrance
  and door clearance, and one-floor stale-floor/stair exclusion.
- Added Play Mode scenarios for building/room entry, floor collision isolation,
  stair ascent and return, wall/room/ceiling fade and restoration, and camera
  target/snap behavior.
- Visibility trigger processing now exposes the same test hook pattern as stair
  transitions, without changing physics callback behavior.
- Scenario assertions include floor, room, profile, seed, and scenario context.

Verification:

- Play Mode `F6`: `2/2` passed with zero warnings and errors.
- Edit Mode `F8`: expected `55/55`; confirmation is still required.

### Stage 27E-B - Visual Acceptance And Building Stability Gate

Status: Implementation Complete - Visual Acceptance Pending

Work:

- Inspect representative fixed seeds for wall corners, room openings, stairs,
  ceilings, sorting, fades, and camera framing.
- Record the accepted visual checklist separately from structural tests.

Completion:

- All supported profiles are playable without manual hierarchy edits.
- Regeneration does not require manual Tilemap, collider, or transform fixes.
- Stage 27 is the formal definition of a stable generated building.

Implemented:

- Added an eight-sample visual gate: four supported profiles at Seeds `1001`
  and `2027`.
- Context-menu workflow prepares each deterministic sample, creates/resets the
  test Unit, prints the exact checklist, validates data/runtime structure before
  acceptance, records approval, and advances to the next sample.
- Acceptance progress is serialized in TestScene and can be reset or summarized.
- The standalone checklist and rejection rules are documented in
  `Stage27EBVisualAcceptanceChecklist.md`.

### Stage 28A - Building Grammar Expansion

Status: Not Started

Work:

- Add non-rectangular or composed footprints.
- Add circulation rules, room adjacency requirements, optional corridors,
  multiple entrance policies, and more than one stair/module variant.
- Separate residential, shop, warehouse, and special-building requirements.

Completion:

- Variation changes architecture, not only footprint size and room count.
- Every grammar still produces validated and playable output.

### Stage 28B - Building Catalog And Authoring Workflow

Status: Not Started

Work:

- Add a catalog of weighted building archetypes and compatible palettes,
  room presets, entrance presets, and stair modules.
- Add profile/preset validation and preview tooling.
- Add schema versions for generated and saved building data.

Completion:

- A designer can add a building family through assets without editing planner
  code.
- Invalid asset combinations are rejected before city generation.

### Stage 28C - Generation Output Policy

Status: Not Started

Work:

- Decide when buildings are generated at runtime, pre-baked, cached, or loaded
  from saved compact data.
- Define generated-object cleanup, pooling, and ownership rules.
- Define save/load regeneration behavior from profile id, seed, version, and
  deliberate overrides.

Completion:

- Reloaded buildings reproduce the accepted structure.
- Content updates have an explicit migration or regeneration policy.

### Stage 29A - City Data Contracts

Status: Not Started

Work:

- Define building catalog candidates, world transform/orientation, occupied
  bounds, clearance, district/category tags, weight, and road sockets.
- Keep city data independent from Tilemap hierarchy.

Completion:

- A data-only city layout can place a building candidate and connect its main
  entrance without generating its Tilemaps.

### Stage 29B - Data-Only City Placement Solver

Status: Not Started

Work:

- Place buildings without overlap using deterministic seeds and spacing rules.
- Support rejection reasons and bounded retries.
- Validate road-facing entrance orientation and reserved clearance.

Completion:

- The same city seed produces the same valid placement layout.
- Failed placements terminate predictably and report why.

### Stage 29C - Roads, Districts, And Composition

Status: Not Started

Work:

- Generate or consume a road graph.
- Connect building road sockets to roads.
- Apply district ratios for residential, commercial, warehouse, and special
  buildings.
- DeBroglie/WFC may be used later for local road/tile pattern resolution, but
  it does not own city semantics or building placement.

Completion:

- Entrances connect to reachable roads.
- District ratios and required special buildings validate successfully.

### Stage 29D - City Rendering, Streaming, And Validation

Status: Not Started

Work:

- Render only accepted city layout data.
- Add chunk activation, pooling, and distant-building simplification.
- Add city-level overlap, connectivity, orphan-road, entrance, and performance
  validation.

Completion:

- Large layouts stay within measured CPU, memory, collider, and active-object
  budgets.
- Buildings can unload and reload without changing their deterministic data.

### Stage 30 - Gameplay World Integration

Status: Not Started

Work:

- Connect navigation/pathfinding, NPC and zombie spawn metadata, loot, building
  ownership/use type, persistence, and city progression.
- Promote stable systems from `___Test` only after regression and performance
  gates pass.

## Immediate Start Point

Stage 27C-A is complete with `28/28` Edit Mode tests passing. The next
implementation stage is 27C-B: bind each Unit to an explicit current-building
registry and replace global floor/visibility searches with registry caches.
Follow with 27C-C multi-building Play Mode isolation tests. Do not start city
road or district generation before Stage 27E-B and the minimum Stage 28 data
contracts are complete.
