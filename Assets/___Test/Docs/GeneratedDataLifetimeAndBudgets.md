# Generated Data Lifetime And Runtime Budgets

## Lifetime Classes

| Class | Current data | Owner | Release point |
|---|---|---|---|
| Authoring | `GeneratedBuildingProfile`, tile palette, sorting/grid profiles | Unity assets | Project lifetime |
| Transient generation | `GeneratedBuildingData`, floors, rooms, walls, doors, stairs, all `GeneratedCellMask` cell lists | `GeneratedBuildingDataView` during generation/debug | After rendering, runtime binding, instance-data rebuild, and validation |
| Compact runtime | `GeneratedBuildingInstanceData`, `GeneratedBuildingRuntimeRegistry`, identities, visibility groups | Generated building instance | Building unload |
| Persistent save | Profile/building id, seed, deterministic hash, placement, primary entrance and road connection | Future city/save record derived from instance data | Save-slot lifetime |

Runtime gameplay must not depend on transient masks. It resolves floor, room,
collider, trigger, stair, and visibility ownership from the per-building registry.
Entrance/spawn fallback uses compact instance metadata after transient release.

`GeneratedBuildingDataView.retainInPlayModeForDebug` is an opt-in diagnostic
escape hatch. Production Play Mode generation leaves it disabled.

## Initial Budgets

Per active building:

- Tilemaps: at most 64
- Collider2D components: at most 64
- Active generated GameObjects: at most 256
- Visibility state update: at most 0.25 ms average over 200 alternating-floor updates
- Retained full generation data: zero after runtime binding

Per active city chunk of 16 buildings:

- Tilemaps: at most 1,024
- Collider2D components: at most 1,024
- Active generated GameObjects: at most 4,096

These are guardrails, not asset-authoring targets. Run `GeneratedGroundGenerator >
Profile 27D-B Runtime Budgets` after changes to the generation grammar, profile
matrix, Tilemap hierarchy, collider structure, or visibility system. Every
per-profile line must report `withinBudget=True`; retain the measured Console
output as the current hardware baseline.
