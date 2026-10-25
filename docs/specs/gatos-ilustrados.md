# Illustrated cats and park

Status: in progress — branch mejora-gatitos-ilustrados, 2026-10-25
Summary: Integrar los gatos redondeados y el fondo ilustrado aprobados, conservando las animaciones y las interacciones del parque.
Orchestrator: Full — coordinate generated art with the existing simulation and verify every animation state.
Blocked by: none
Visual: yes

## Intent

Replace the procedural scenery and tiny cat artwork with the two visual proposals approved in chat on 2026-10-25. Preserve the full-screen park and its existing self-running simulation. The user explicitly requested implementation: “Sí, me gusta. Implementá esos gatos y el fondo nuevo, manteniendo las animaciones y las interacciones actuales.”

## Acceptance criteria

- WHEN the page loads, THE SYSTEM SHALL display the approved illustrated park and rounded orange, gray and calico cats.
- WHILE the simulation runs, THE SYSTEM SHALL retain eight cats and their wandering, sitting, sleeping, stretching, grooming, rolling, scratching, chasing, playing, box and gym behaviors.
- WHEN a person clicks or taps the scene, THE SYSTEM SHALL throw a yarn ball at the corresponding scene position and attract the nearest eligible cats, retaining the seven-ball limit.
- WHILE cats use the gym, THE SYSTEM SHALL preserve jumps, perching and the slide route with correct ground and platform alignment.
- WHILE cats enter the box, THE SYSTEM SHALL hide their bodies behind its front wall and reveal their heads naturally.
- WHEN the viewport changes, THE SYSTEM SHALL keep scene proportions and pointer coordinates correct without horizontal overflow.
- WHEN an illustrated asset fails to load, THE SYSTEM SHALL keep the original playable fallback visible rather than leave a blank scene.
- WHEN the page is opened directly from disk, THE SYSTEM SHALL load its bundled artwork without a server, external requests or runtime dependencies.

## Decisions

- Visual direction approved in chat on 2026-10-25: generated park and rounded expressive cats, not a new website layout.
- Keep the simulation's 256 by 144 coordinate space. Increase backing resolution for rendering detailed images without changing movement rules.
- Three illustrated coats reflect the approved art; retain eight actors with varied sizes and their existing behavior choices.
- Existing boxes, yarn, gym, clouds, flowers, hearts, sparks and sleep marks remain animated and interactive as applicable.
- Supplementary poses must follow the approved character reference. Verify actual PNG alpha; the initial preview's apparent colored background was not evidence of opacity.
- No runtime packages, backend, tracking, publication or remote writes.
- Final rendered visual acceptance remains pending the user; approval of the design authorizes implementation.

## Out of scope

New gameplay, a redesigned interface, subscriptions, backend services, broad repository standardization and public deployment.

## Slices

1. Prepare local PNG assets and source rectangles for resting and action poses.
2. Integrate detailed rendering with the existing simulation, retaining procedural fallback.
3. Verify poses, movement, interactions, failure recovery, offline loading and responsive rendering; update the existing human documentation and package a playable local copy.

## Criteria status

| Criterion | Status | Evidence |
|---|---|---|
| Approved illustrated park and cats | met | Original approved PNGs bundled unchanged; 30 poses inspected on a rendered contact sheet and the running park. |
| Eight actors and all behaviors | met | node tests/simulation.cjs: two 200-second deterministic traces equal original commit 5cf72af, all eleven states observed. |
| Pointer interaction and ball limit | met | 360 generated viewport/input cases; browser click/touch at 390 and 1440, nearest eligible cats chased, seven-ball limit passed. |
| Gym alignment and route | met | All nine waypoints in both baseline traces; jump, perch, scratching and slide inspected on rendered scene. |
| Box occlusion | met | Head remains above front wall, body drawn behind it in rendered scene. |
| Responsive geometry | met | 390, 768, 1440 and 1920 widths: no overflow, axe violations or final console failures. |
| Asset failure fallback | met | Each PNG blocked independently and all three together: original artwork visible, click still added yarn, no runtime exception. |
| Direct disk opening | partial | Bundled relative classic scripts and images, no fetch/modules/pixel reads; direct file URL could not be exercised because the app browser policy rejects file: URLs. HTTP local version verified. |

Review: accepted — substantive self-review of the full implementation and source rectangles; reversible visual change without external consumers, stored data or network services. Node trajectory and geometry tests pass. Browser verification passes; direct-file runtime verification and final human visual acceptance remain pending.

Visual acceptance: pending the user — design approved; implemented park available for evaluation.

### UI verification: illustrated park

Environment: Chromium via Playwright 1.64.0 and app browser; local static preview, no backend or user data. All images generated for this project. Coverage: full input checks at 390 and 1440, layout review at 768 and 1920, thirty poses and gym/box states.

Page measured: http://127.0.0.1:4173/ — real index.html, with isolated test-only instrumentation for internal state assertions. Production code has no test API.

Approved reference: parque.png SHA256 FCEED412784D409151F4D741B5ACFA1EBD76C87B703DEDD3EC4048282990536D; gatos.png SHA256 71BB1D2EC8838EFB47F2C45B2F6F382BE9830C392DAF5010973EA7EE5C9870C6. Both approved in chat on 2026-10-25.

Plan, written before the first check: .local/verification-plan.md. Full initial scene, input and visual checks at 390 and 1440; review at 768 and 1920. Automated layout runner uses height 900; custom full input checks also used 900. Original 16:9 containment retained.

| State | Widths |
|---|---|
| Initial, focus, click/touch and keyboard | 390, 1440; layout review at 768, 1920 |
| All poses, gym, box and reduced motion | 1440 |
| Each missing PNG and all missing | 768 |

Mechanical checks: comprobar-pantalla.mjs --url http://127.0.0.1:4173/ --anchos 390,768,1440,1920 --simultaneas 1: no final failures. Node simulation and art tests pass, including 960 anchor/mirror cases. Browser gestures and asset failures passed. Inherited outside the changed region: none reported by the final mechanical check.

| Width × height | State | Capture | Finding |
|---|---|---|---|
| 390 × 900 | Initial and input | .local/capturas-final/1-390-inicial.png; .local/interaction-390.png | No clipping or overflow; original portrait letterboxing retained. |
| 768 × 900 | Initial | .local/capturas-final/2-768-inicial.png | No layout failures. |
| 1440 × 900 | Initial, poses, gym/box | .local/capturas-final/3-1440-inicial.png; .local/all-poses.png; .local/gym-box.png | Two scratch crops originally included pixels of a neighboring row; crop polygons corrected, inspected again. |
| 1920 × 900 | Initial | .local/capturas-final/4-1920-inicial.png | No layout failures; original side letterboxing retained. |

Controls exercised: pointer interaction retained, keyboard equivalents added.

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Throw yarn | Click and touch | Browser tests at 390 and 1440 passed: ball added at delivered pointer coordinates and eligible cats chased it. |
| Aim and throw | Arrows and Enter | Browser and VM tests passed: aim moved, ball appeared at the new position. |
| Pause and resume | Space | Browser and VM tests passed; app browser announced paused state and resumed after Space. |

Walk of the frequent task: open the park, tap meadow, observe pursuit, repeat throws, focus with Tab, aim, throw and pause. Performed with browser touch and mouse at the full-treatment widths and manually in the app browser; no runtime exceptions.

Accessibility floor: axe passed at all four widths; canvas has accessible description and keyboard access; white inset outline and dark outer ring visibly verified after fixing the previous hidden inset shadow. Reduced motion starts paused. The park has no visible text controls or data tables; canvas touch area exceeds 44px in each dimension at checked widths.

Look: rounded expressive cats and illustrated landscape match the accepted direction. Foreground decorations retain the original coarse pixel art. Background and cat assets have genuine alpha; contact-sheet inspection confirmed clean silhouettes.

Differences from the approved reference: static concept poses expanded to animated poses; pose scales and feet registration adapted to existing platforms. Eight cats reuse the three approved coats. Existing gym, clouds, flowers, yarn and box remain procedural. These are within the requested preservation of current interactions.

Transitions: walk and grooming frames inspected; jump, perch and slide states inspected; autonomous traces traverse every gym segment. Reduced-motion scene remained still until explicitly resumed.

Scroll effects: none — full scene fits viewport with the original containment behavior.

Stress page: not applicable — no catalog or variable text content added.

| Round | Failures found | Corrections | Cells checked again |
|---|---|---|---|
| 1 | Parallel browser resource error ERR_NO_BUFFER_SPACE at 768; touch assertion assumed subpixel coordinates; inset focus shadow invisible; two scratch crops included neighboring pixels. | Reran layout serially; test now asserts against actual browser-delivered coordinates and independently bounds touch rounding to one CSS pixel; visible outline; precise crop polygons. | All four layout widths; full input and fallback suite; all thirty poses; live focus. |
| 2 | No remaining HTTP runtime, layout, input or asset-crop failures. | None. | Final poses and browser suite passed. Direct file scheme remains policy-blocked, not reported as executed. |

Visual inspection: passed — HTTP preview and rendered captures inspected; final human acceptance remains pending.
