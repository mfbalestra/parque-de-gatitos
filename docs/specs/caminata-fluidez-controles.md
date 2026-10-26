# Natural walking, selectable frame rate and clearer controls

Status: in progress — branch mejora-gatitos-ilustrados, 2026-10-25
Summary: Los ocho pelajes tienen una caminata ligada a la distancia y el panel permite guardar 30 o 60 FPS junto con las preferencias del parque. Implementación verificada; valoración visual del propietario pendiente.
Orchestrator: Full — coordinated artwork, animation timing, persistent settings and native UI verification.
Blocked by: none

## Intent

Improve the existing native WPF companion on the current feature branch. Replace its two-pose sliding walk with an eight-frame cycle for all eight coats, add an explicit 30/60 render target and reorganize the existing configuration window. Preserve the desktop/park lifecycle and passive overlay. Deliver a runnable local Windows package and measured evidence. The owner explicitly approved this scope on 2026-10-25.

## Acceptance criteria

- WHILE any of the eight coats walks on the desktop or through the park, THE SYSTEM SHALL show an eight-frame loop with distinct leg support, passing and swing poses, a stable silhouette and aligned ground contact.
- WHILE a cat travels, THE SYSTEM SHALL advance its walk phase from distance traveled, stop advancing that phase when stationary and preserve its direction without backward sliding of planted paws beyond the visible limits of discrete artwork.
- WHEN rendering changes between 30 and 60 FPS, THE SYSTEM SHALL preserve travel speed, jump duration and park schedule, and target the selected rate subject to display and system capability.
- WHEN the user saves valid preferences, THE SYSTEM SHALL apply and persist cat count, frame-rate target and both park durations together without restarting the visit timer or moving existing cats solely because an unchanged count was saved.
- WHEN existing settings contain only the park durations, THE SYSTEM SHALL retain those durations and supply defaults of four cats and 30 FPS.
- WHEN the user submits invalid fields or saving fails, THE SYSTEM SHALL explain the affected input or failure and retain all previously applied preferences.
- WHILE the park is expanded, THE SYSTEM SHALL disable changing cat count with a visible explanation while allowing frame-rate and duration changes.
- WHEN the controls open, THE SYSTEM SHALL show current activity and next transition, separate immediate actions from preferences, expose selected count/rate, indicate unsaved changes, and provide one Save preferences action.
- WHEN the user navigates the panel by keyboard or at supported enlarged display scaling, THE SYSTEM SHALL keep labels, visible focus, validation messages and all actions reachable without clipped content.
- WHILE paused or hidden, THE SYSTEM SHALL stop animation work and park progression at both render targets, retaining the existing distinction between pause and visibility.
- WHILE cats or the park are visible, THE SYSTEM SHALL keep clicks passing through the overlay and preserve typing and program switching; the deliberately opened controls remain a normal interactive window.
- WHEN delivered, THE SYSTEM SHALL include measured CPU, resident/private memory and actual rendered frame rates for 1/4/8 cats at both targets on desktop and in the park, with sampling limits and no invented savings claim.

## Decisions

- Proposed: eight compact native walking frames per coat, generated from the established illustrations, reviewed as a loop before integration. Eight drawings per cycle and 60 render updates per second are separate concepts. Repeated static poses or whole-body bobbing are not sufficient.
- Proposed: keep 30 FPS as the initial target because resource use matters to the owner; 60 FPS is an explicit saved preference. Increasing render rate must not accelerate simulation.
- Proposed: preserve Windows-native light controls, typography and window behavior; sober operational composition, grouped settings and restrained accents. No new visual identity or UI framework. The HTML composition is a schematic, not a running WPF implementation.
- Proposed: a single save commits all settings atomically; immediate Pause/Hide/Visit/Return actions stay separate. Existing duration ranges and application to the current cycle remain unchanged. CLI cat-count overrides are session-only until the owner explicitly saves preferences.
- User moments: scope and sober direction approved 2026-10-25; native early look delivered and the owner's leg-connection correction applied. Final visual acceptance pending. The walkthrough distinguishes generated artwork, measured behavior and owner acceptance. The four proposed decisions above are approved by that answer.
- Keep reviewable local work on mejora-gatitos-ilustrados, based on 019b0efa7d1ef2371bee569545a9299a1c5464e4. No push or deployment in this request.
- Implementation: eight frozen native drawings per coat are composed from generated body/limb pieces. Complete generated pose sheets were rejected for repeated leg positions. The owner's subsequent correction about disconnected legs is addressed by layering the near shoulders/hips over the torso and blending their upper outlines.
- The early native controls exposed excessive scrolling; Save, feedback and Exit now stay in a fixed footer. The owner chose sober native controls, so no framework or new visual identity was introduced.
- visual acceptance: pending the user. The implemented comparison and runnable package are available; approval of scope is not acceptance of the resulting gait.

## Out of scope

General memory architecture optimization, replacing WPF, new dependencies, startup registration, multimonitor behavior, new web interactions, redesigning the park, other animal animations and public publishing. New walking assets will be decoded at an appropriate size; this does not imply a general memory reduction claim.

## Slices

- Complete distance-driven walking for all eight coats, with loop inspection and motion contracts (native artwork, CatMotion, SpriteCatalog and drawing integration).
- Deliver saved 30/60 FPS selection through the reorganized native controls, preserving settings compatibility and park behavior (OverlayWindow, settings, CLI and tests).
- Verify coexistence, settings, actual frame rates and resource use; update the human guide and package the runnable application with an explicit visual acceptance status.

## Criteria status

| Criterion | Status | Evidence |
|---|---|---|
| Eight-frame loop and stable silhouette | met | 64 native drawings inspected; new walk observed in the park. Owner aesthetic acceptance pending separately. |
| Distance-driven support and rest | met | WalkContracts tests periodic stride, stationary supporting paw model and frozen phase at rest. |
| 30/60 equivalence and target | met | Same positions/phases at both rates; pacer tested against six refresh rates. Actual drawings below target 60 are disclosed. |
| Atomic saved preferences without timer reset | met | Settings contracts, UI save, unchanged countdown continuity and restart with 8/60. |
| Older settings compatibility | met | Old two-field JSON retains durations and receives 4/30 defaults. |
| Invalid or failed save retains state | met | Invalid field UI and locked-file replacement test. |
| Count lock during park | met | Disabled radios and explanation observed; FPS/durations remain available. |
| Grouped panel and one save | met | Native panel exercised; fixed footer and dirty status observed. |
| Keyboard and enlarged layout | met | Focus/accelerators exercised; actual controls rendered at 340/520 DIP and simulated 150%; actual system DPI remained 100%. |
| Pause/hidden stop work | met | Pause clock observed at 30/60; all twelve hidden samples have zero frames and steps. |
| Passive coexistence | met | Final body click and accented typing through park; unchanged native filters/styles; prior Alt+Tab evidence reused with the new attempt's focus limit documented. |
| Measured delivery | met | Twelve cases in desktop/walk-measurement-summary.json; one repetition, CPU, resident/private memory and observed draw rate with limits. |

Review: not required — reversible local change; substantive self-review and tests recorded in desktop/VERIFICACION-CAMINATA.md. Visual acceptance remains pending, so the spec stays open.
