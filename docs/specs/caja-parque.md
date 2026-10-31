# Cardboard box visual refresh

Status: in progress — branch mejora-gatitos-ilustrados, 2026-10-31
Summary: Caja de cartón acorde al parque ilustrado, con textura, solapas con volumen y sombra suave. Conserva la interacción de los gatos al entrar y salir.
Orchestrator: Lite — localized rendering change with existing movement and layering contracts.
Blocked by: none

## Intent

Implement the owner's approved box refresh in the browser park after observing that the published box retained its original flat drawing. Match the existing illustrated structure through the same canvas drawing helpers. Preserve physical behavior and world placement.

## Acceptance criteria

- WHEN the box is visible, THE SYSTEM SHALL show textured cardboard, dimensional flaps and a soft ground shadow consistent with the existing park.
- WHEN a cat occupies the box, THE SYSTEM SHALL draw its visible upper body between the rear and front box layers for every coat.
- WHEN a cat enters or leaves, THE SYSTEM SHALL preserve the existing movement, waiting time and exit jump without changing its world location or footprint.
- WHEN the scenic detail module is unavailable, THE SYSTEM SHALL retain the existing fallback drawing.

## Decisions

The owner approved the direction in chat. Reuse the structure's native canvas technique and palette instead of adding a new bitmap or dependency; this gives exact foreground occlusion and matches the nearby structure. Keep the original interaction logic. This is the browser box: the native park uses its existing background image and has no equivalent interactive box renderer. The owner rejected the first result's inconsistent perspective: a slanted side conflicted with the frontal opening. The correction uses one centered frontal view, horizontal front edges, mirrored flaps and depth confined to the opening. Visual acceptance: pending the user for the corrected result; the first version is rejected.

## Out of scope

New box behavior, native park changes, other scenery, new dependencies and publication without a current push request.

## Continuity

Base 1c7eeaf39b63b4ab185ebbc20cfaba7f86a753a8; clean tree on the existing branch. Implementation changes the two box layers only. Comparison and evidence live in `.local/disenos/caja/`; the local server remains on port 4180. No push performed.

## Criteria status

| Criterion | Status | Evidence |
|---|---|---|
| Matching cardboard appearance | met | Corrected frontal construction retains texture and shadow. Technical rendering inspection is separate from the owner's pending acceptance; the first angled result was rejected. |
| Cat between layers in every coat | met | check-box.cjs exercises all eight coats; inspected occupied box and unchanged rear/cat/front order. |
| Existing movement and placement | met | Real entry/occupancy/exit completes at both widths; simulation, motion, art and web-controls pass, including continuous box jump and lifecycle release. No behavioral code changed. |
| Fallback | met | Original drawing remains in the missing-detail branches; simulation and art tests retain missing-resource coverage. |

Review: accepted — substantive self-review of the full localized rendering diff against the base above. Reversible scenery change without data, dependency, external contract or security consequences; no independent reviewer required. Reused existing rounded, line and shadow helpers, factoring the repeated cardboard face drawing once. Identifier search for drawBox/cardboard found only the intended definitions and call sites; no jscpd run. The existing manual now documents layering and fallback. The real park and comparison pass comprobar-pantalla with axe; `.local/disenos/caja/verificacion.md` records initial fixture/accessibility failures and corrections. No pending functional finding; local visual result awaits the owner's judgment. No permanent test was added for this bounded drawing change; existing movement/lifecycle coverage and temporary browser inspection verify the preserved contract.

Perspective correction, 2026-10-31: base c8c5a83ea7289179a37bad06b01cab0318576c95, clean same branch. The owner rejected the angled result. Removed the visible side plane, aligned the front edges, centered the tape, mirrored the side flaps and used one symmetric opening. Only scenic geometry and its documentation change; movement, layering and dimensions remain as before. The comparison now freezes the rejected scenic module as its before reference. Fresh check-box.cjs passes both widths, eight coats, occupied/empty/exit states and real entry/exit; comprobar-pantalla with axe passes the comparison and actual park at 390/1440. Empty and occupied corrected renders were inspected directly. Prior movement and lifecycle suites remain applicable because none of their code or data changed. Substantive self-review of this correction found no functional regressions; aesthetic judgment remains the owner's. The perspective complaint requires visual judgment and is not represented as an automatically checkable style rule. No push.
