# Procedural calm and orange interaction pilot

Status: in progress — branch mejora-gatitos-ilustrados, 2026-10-31
Summary: Ritmo tranquilo y elecciones contextuales en Windows y web, con nuevas interacciones mostradas primero con gatos naranjas antes de extender el arte a los demás pelajes.
Orchestrator: Full — two runtimes and shared artwork with an explicit owner review gate.
Blocked by: none

## Intent

Implement the first batch approved in chat on 2026-10-31: durable calm activities, kneading and settling, affectionate rubs, shared rest and occasional playful sparring. Replace predictable action schedules with weighted contextual decisions. Deliver a working orange-only interaction pilot for owner inspection in both runtimes. Existing eight-coat walking and park lifecycle remain intact. The existing paridad-pages and walking specs describe previously implemented behavior and their outstanding visual judgments; reconcile this work against base 64b609b without reopening that implementation.

## Acceptance criteria

- WHEN cats act autonomously, THE SYSTEM SHALL choose from eligible activities using independent per-cat random state, energy, context and recent activity rather than a fixed order or periodic mandatory jump.
- WHILE resting autonomously, THE SYSTEM SHALL sustain sleep for 60–180 seconds, sitting for 20–60 seconds, and grooming for 15–30 seconds unless the owner requests a scene transition.
- WHEN a walk is selected, THE SYSTEM SHALL move to a reachable nearby destination with the existing distance-driven gait and stop upon arrival.
- WHEN a new action ends, THE SYSTEM SHALL reduce the likelihood of immediate repetition and retain individual state across scene transitions.
- WHEN cats interact, THE SYSTEM SHALL reserve both available partners, approach continuously, coordinate contact, and release both on completion or cancellation.
- WHILE a cat sleeps or recovers from play, THE SYSTEM SHALL exclude it from intense play invitations for at least the shared cooldown after play.
- WHEN only one cat exists, THE SYSTEM SHALL use solo actions without waiting for partners.
- WHILE in company, THE SYSTEM SHALL allow at most one intense-play pair; in the park it shall allow at most one pair with four cats and two with eight.
- WHEN the orange pilot is inspected, THE SYSTEM SHALL show kneading, settling, rubbing, shared rest and sparring with complete-cat images, without detached limbs, before any artwork expansion to the other seven coats.
- WHEN pause, hiding, a scene transition, viewport changes or cat-count changes occur, THE SYSTEM SHALL preserve pause semantics and cancel or finish interactions without teleporting or leaving a reserved partner.
- WHEN 30 or 60 FPS is selected, THE SYSTEM SHALL retain action durations and movement speed independently of rendering.
- WHILE running on Windows, THE SYSTEM SHALL retain passive input behavior; on the web it shall remain inside the tab.
- WHEN an individual cat starts a durable rest, THE SYSTEM SHALL reserve an unoccupied place away from the slide and traffic points, walk there continuously and start the complete rest duration upon arrival.
- WHEN a cat descends the slide, THE SYSTEM SHALL keep it supported on the ramp; the browser autonomy reset SHALL preserve an active structure route instead of interrupting it in midair.
- WHEN most peers are quietly resting, THE SYSTEM SHALL progressively favor visible activity at the next available cat's decision, respecting fatigue, current rests, eligibility and cooldowns without a mandatory activity quota.

## Decisions

Approved scope: owner's current message. Orange visual acceptance is pending; do not generate or apply new interaction art to the other seven coats until that review. Calm scheduling applies to all coats immediately; new poses and paired interactions are enabled only where orange pilot assets exist. The normal eight-coat scene therefore has a single orange and cannot demonstrate orange-orange encounters; provide an explicit orange demonstration mode without changing saved coat/count preferences.

The word procedural means weighted choices with memory and context, not unconstrained randomness or machine learning. Short coordinated animation phases within one interaction are required, but the next activity is not prescribed. Shared duration and probability defaults live in behavior-profile.js and are consumed as data by both languages. Use deterministic seeds for tests, varied runtime seeds for normal sessions. No new dependency, network service, sound or notification.

## Out of scope

- New artwork for non-orange coats before owner review; mutual grooming, group butterfly watching, new personality UI and further ideas from the proposal.
- Publishing or pushing this new change without a current publication request.
- General standards migration, new dependencies, architectural replacement or unrelated cleanup.

## Slices

1. Contextual behavior and meaningful invariant tests in browser simulation and native motion/cycle; shared rule data.
2. Whole-cat orange sprites and render adapters with inspectable individual and paired demonstrations.
3. Both runtime builds, lifecycle and frame-rate checks, real visual inspection and updated existing manuals; hand off the orange pilot for owner acceptance before expanding coats.

## Continuity

Base: 64b609be9ed5cdfadc5a1db4106a5662ae809aef; clean tree at start. The original code used 6–11 second browser naps and a mandatory jump in each native 20-second cycle. Those schedules are intentionally replaced under the owner's explicit correction; tests that assume periodic jumps must set up a jump explicitly while preserving their continuity assertions.

Implementation slices 1–3 are ready as an orange pilot. Shared defaults, contextual choices, durable rests, complete-cat artwork, pair coordination and the five demonstrations are implemented in both runtimes. The browser preview is `.local/disenos/ritmo-convivencia/index.html`; native output is `../../outputs/gatitos-calma`, with six temporary-mode launch shortcuts. Close one native demonstration before opening another. Neither demonstration saves preferences. No push or publication was performed.

Verification, 2026-10-31: all seven browser suites pass (`simulation`, `motion`, `art`, `walk-web`, `web-controls`, `behavior`, `orange-art`). The native contract executable passes 527099 assertions, including variable seeds, pair caps, edge contact, recovery, cancellation, mat arrival and 30/60 timing. Release publication succeeded; the final published executable's self-test reports passed=true, 1764593 assertions and eight coats. The first republish failed because the inspection instance held its DLL; the identified session-owned instance was closed and publication then passed. The self-test output is `../../work/calm-native-checks-final.json`.

Browser visual evidence: `.local/disenos/ritmo-convivencia/verificacion.md`, validated by `check-registro.mjs` with one record and zero failures. Real Chromium checks cover 390, 768, 1440 and 1920 px, five actions, autonomy, keyboard, pause, reduced motion and 30/60 FPS. Five random layouts reach shared-rest contact and cancel cleanly. Natural approach/contact/exit passed for rubbing, shared rest and sparring without accelerating the simulator. The review found excessive shared-rest spacing and close-diagonal partner blocking; both were corrected and checked again, with a permanent regression for the latter. Screenshots show complete cats and cheek/nose contact. Native WPF was visually inspected with four orange cats and shared-rest contact; input-passive flags remain unchanged, but OS click-through/Alt-Tab and native pause UI were not re-exercised in this turn. The native pause attempt was rejected by automation because window bounds changed; it is not counted as a passed UI check.

Review: self-verification and substantive quality pass for this reversible local animation pilot; no new dependency, persisted-data format, service or security boundary. Reviewed the complete tracked diff and new behavior/render/test files against the base above. Fixed durations and caps come from the shared data file; JavaScript and C# retain their own movement rules and are not claimed to produce identical sequences. Identifier searches and direct reading were used for duplication review; jscpd was not run. The historical digest stays unchanged; replaced short-random-trace coverage with explicit physical fixtures while preserving trajectory, gym-route, continuity and prolonged simulation checks. README and desktop/README are the existing human-facing manuals and now explain the behavior, pilot and limits. No unresolved functional finding remains in the pilot. Visual acceptance by the owner remains pending and blocks artwork expansion and full spec closure, not delivery of this inspectable slice.

Next decision: owner judges the orange interactions and their autonomous rhythm in the real preview. Do not generate the other seven new interaction sheets or publish this work before the applicable owner instruction.

## Correction after owner inspection — 2026-10-31

Owner reported durable crowding at common destinations, especially the slide exit, and jumping from the upper platform. This continues the approved pilot correction. Base for this slice: d3f8708a9b19062ba17a1cdc8c7b1524cde8cc9e; branch unchanged and clean at start.

Reproduction: the real browser simulator kept four cats at (237,118) after 30 simulated seconds of sleep. Reset at the upper platform changed gym to sit while preserving z=40/onS=true. Native reproduction failed the explicit requirement to approach a free location before starting sleep; the last gym segment was marked hop=true. Before/after browser evidence is `.local/disenos/ritmo-convivencia/espacio/before.json` and `after.json`, with screenshots alongside. Browser slide support itself already existed; the reset and return were interrupting that path.

Implemented: shared clearance and traffic exclusion data; individual rest reservations consider actors and reserved destinations. Timers wait for arrival. Intentional partner contact remains allowed, and third cats do not reserve those footprints. The single-row browser companion strip uses spaced candidate positions so eight cats fit; park positions retain nearby sampling. Brief overlaps while walking remain possible; this is not a general collision-physics engine. Native full-mat occupancy falls back to a free sitting location. Native final gym segment slides seated without a hop and walks clear on arrival. Browser reset and return preserve the existing supported descent. No artwork, dependency, preference format or publication changed.

Verification: eight JavaScript suites pass, including permanent `tests/spatial.cjs` regression across 24 clustered scenarios, timers, continuity, ramp support and interrupted return. Existing motion tests exposed an out-of-bounds yarn takeoff after a ground step; the ground step now clamps before recording the takeoff, and the unchanged assertion passes. Native contract suite passes 566806 checks, including 18 clustered configurations, partner reservations and occupied mats. Release output `../../outputs/gatitos-calma-espacio` built successfully; its actual executable self-test passed 1752299 assertions across eight coats (`../../work/spacing-native-final.json`). These are deterministic invariant checks plus runtime self-tests, not a new resource benchmark or OS input compatibility measurement.

Review: substantive self-review of the complete correction diff and new regression file; reversible local motion change with no external consumer, data or security consequence. Reused existing MoveTo, GYM route, behavior profile, paired reservations and test harness. Searched rest reservation and route identifiers across both runtimes; no jscpd run. Existing test expectations were preserved. UI checks and visual inspection passed as recorded in `.local/disenos/ritmo-convivencia/espacio/verificacion.md`; five actions, 30/60, pause, keyboard and reduced motion passed at 390/1440. Native movement was checked through its real classes and published executable; this slice does not claim a separate native visual recording of the slide or a new click-through/Alt-Tab test. The existing manuals were updated. Ready for owner inspection; visual acceptance and extension to the other seven coats remain pending.

## Group rhythm correction — 2026-10-31

Owner noted that accumulated rests can leave the whole scene feeling still. Base: 7e25b4c61e543915025a636744afcd8a0458e479, clean tree on the same branch. Reused the existing weighted choice in both runtimes and the shared profile. The new groupBalance data progressively changes weights only above a majority of quiet peers, with smaller influence on tired cats. Every eligible activity retains positive probability, solitary behavior stays identical and existing sleep/recovery timers are untouched. No fixed train or minimum activity count is imposed.

Measurement source: `../../work/measure-group.cjs`, runs of 20 simulated minutes for 4/8 cats and seeds 19, 71 and 7123; before/after JSON in `../../work/group-before.json` and `group-after.json`. Visible activity means movement, jumping, grooming, kneading or a non-sleeping encounter, not just travel. For four cats, mean active counts were 0.57/0.48/0.63 before and 0.96/1.12/1.13 after; completely quiet time was 56.6/55.8/50.6 percent before and 21.9/16.0/24.6 after. For eight cats, counts were 1.19/1.03/1.01 before and 2.20/2.28/2.15 after; quiet time was 38.9/35.0/35.9 percent before and 3.8/1.5/6.7 after. These are browser simulation samples, not guaranteed schedules or native measurements.

Verification: nine JavaScript suites pass, including new tests/group.cjs for increasing activity preference, fatigue, strictly positive weights, aggregate balance across seeds, unchanged solo behavior and uninterrupted sleep. Its initial solo assertion compared arrays from separate VM realms; equal serialized values were confirmed and the fixture comparison now compares those values, without changing its criterion. Native contracts pass 571630 checks with corresponding generated group-factor properties. Release output `../../outputs/gatitos-calma-grupo` builds; actual executable self-test passes 1756455 checks and eight coats (`../../work/group-native-final.json`). The real preview ran autonomously at 390 and 1440 with no page errors and pause preserving the full snapshot; screenshots and samples are in `.local/disenos/ritmo-convivencia/grupo/`.

Review: self-verification and complete diff review, bounded reversible probability adjustment with no new external contract or data change. Group-factor formulas in JavaScript and C# consume the same profile and have matching monotonicity/fatigue properties; runtime movement remains distinct. Eligibility, cooldowns and partner rules still apply after weight adjustment. Existing spatial, slide and 30/60 tests remain green. Prior UI composition evidence remains applicable; fresh autonomous captures show complete cats and mixed rest/activity. Human judgment of the pilot rhythm and artwork remains pending. No push, publication or additional coat artwork.
