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
