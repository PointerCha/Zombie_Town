# Stage 27E-B Visual Acceptance Checklist

## Sample Matrix

Inspect each supported profile at Seeds `1001` and `2027` for eight total
samples. The TestScene `GeneratedGroundGenerator` stores approval progress.

Start with context menu `27E-B Prepare Current Visual Sample`. For every sample,
inspect both Scene/Game view and move the generated test Unit through the
building. If every item passes, run `27E-B Accept Current And Prepare Next`.

## Required Checks

1. All four outer corners use the intended corner tile and have no protruding,
   doubled, or missing wall segment.
2. Main entrance and room doors are open, aligned, and wide enough for the test
   Unit without touching an invisible wall collider.
3. Stair visual, walkable area, entry/exit triggers, and both rails align; the
   Unit can go up and return without an incorrect floor collision.
4. The ceiling/roof sits directly on the highest floor for one- and two-floor
   profiles, with neither sinking nor a vertical gap.
5. Ground, lower/upper walls, room walls, stairs, and ceiling have stable
   isometric sorting while the Unit moves around all sides.
6. Floor entry, room entry, stair travel, and exit fade/restore the intended
   walls and ceiling only; no visual remains transparent afterward.
7. Camera framing keeps the Unit and the traversed building area readable and
   does not jump to another building.
8. Rebuilding the same sample requires no manual hierarchy, Tilemap, collider,
   or transform correction.

Reject the sample on any visible defect, Console warning/error, failed runtime
validation, blocked doorway/stair, stale floor behavior, or manual adjustment.
Do not press Accept until the defect is fixed and the same profile/seed is
prepared again.

Completion requires `Accepted: 8/8`, `Complete: True`, Edit Mode `55/55`, and
Play Mode `2/2`, all with zero warnings and errors.
