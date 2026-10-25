# Illustrated cats and park

Status: in progress — branch mejora-gatitos-ilustrados, 2026-10-25
Summary: Ampliar el parque a ocho variedades de gatos, dar impulso y aterrizaje a los saltos, mover las patas al rascar y mejorar gimnasio, mariposas, mantitas y sombras.
Orchestrator: Full — coordinate generated art with the existing simulation and verify every animation state.
Blocked by: none
Visual: yes

## Intent

Replace the procedural scenery and tiny cat artwork with the two visual proposals approved in chat on 2026-10-25. Preserve the full-screen park and its existing self-running simulation. The user explicitly requested implementation: “Sí, me gusta. Implementá esos gatos y el fondo nuevo, manteniendo las animaciones y las interacciones actuales.”

The owner's next correction on 2026-10-25 extends this same visual result: minimum eight varieties, natural jumps instead of a standing cat floating upward, improved gym and butterflies, blankets on the grass, forepaw movement rather than body bouncing while scratching, and softer shadows. This explicitly authorizes changes to jump trajectories, timing and poses; the original exact-trajectory criterion is superseded for those motions while the interaction repertoire remains.

## Acceptance criteria

- WHEN the page loads, THE SYSTEM SHALL display the approved illustrated park and at least eight distinct cat varieties, with every variety represented among the eight cats.
- WHILE the simulation runs, THE SYSTEM SHALL retain eight cats and their wandering, sitting, sleeping, stretching, grooming, rolling, scratching, chasing, playing, box and gym behaviors.
- WHEN a person clicks or taps the scene, THE SYSTEM SHALL throw a yarn ball at the corresponding scene position and attract the nearest eligible cats, retaining the seven-ball limit.
- WHILE cats use the gym, THE SYSTEM SHALL preserve jumps, perching and the slide route with correct ground and platform alignment.
- WHILE cats enter the box, THE SYSTEM SHALL hide their bodies behind its front wall and reveal their heads naturally.
- WHEN the viewport changes, THE SYSTEM SHALL keep scene proportions and pointer coordinates correct without horizontal overflow.
- WHEN an illustrated asset fails to load, THE SYSTEM SHALL keep the original playable fallback visible rather than leave a blank scene.
- WHEN the page is opened directly from disk, THE SYSTEM SHALL load its bundled artwork without a server, external requests or runtime dependencies.
- WHEN a cat jumps, THE SYSTEM SHALL show preparation, a continuous airborne trajectory with an appropriate body pose, and a grounded landing and recovery.
- WHILE a cat scratches the structure, THE SYSTEM SHALL alternate its front paws while its rear paws remain planted, without whole-body vertical bouncing.
- WHILE the park is visible, THE SYSTEM SHALL show a detailed gym, recognizable fluttering butterflies, at least three blankets and softly fading elliptical contact shadows.

## Decisions

- Visual direction approved in chat on 2026-10-25: generated park and rounded expressive cats, not a new website layout.
- Keep the simulation's 256 by 144 coordinate space. Increase backing resolution for rendering detailed images; the later motion correction changes jumps within this coordinate space.
- Eight varieties: orange tabby, gray tabby, calico, black, white, tuxedo, Siamese and brown tabby. Retain eight actors with varied sizes and their existing behavior choices.
- Boxes, yarn, clouds, flowers, hearts, sparks and sleep marks retain their functions. The gym and butterflies receive new drawing; blanket decoration sits behind actors.
- Jump timing and poses change to meet the latest explicit request. Keep the original deterministic trace fixture as historical evidence; test new motion against continuous-trajectory, phase, endpoint and interaction invariants rather than replacing its digest with one generated from the new implementation.
- Supplementary poses must follow the approved character reference. Verify actual PNG alpha; the initial preview's apparent colored background was not evidence of opacity.
- No runtime packages, backend or tracking. On 2026-10-25 the owner explicitly authorized push and publication through the existing GitHub Pages configuration (main, repository root).
- Final rendered visual result accepted by the owner on 2026-10-25: “Me gusta cómo quedó. Pusheá los cambios y publicá esta versión en GitHub Pages.”

## Out of scope

New gameplay, a redesigned interface, subscriptions, backend services and broad repository standardization.

## Slices

1. Prepare local PNG assets and source rectangles for resting and action poses.
2. Integrate detailed rendering with the existing simulation, retaining procedural fallback.
3. Verify poses, movement, interactions, failure recovery, offline loading and responsive rendering; update the existing human documentation and package a playable local copy.

## Previous verification — ece4d72, before the owner's motion correction

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

Previous review: accepted for ece4d72 — substantive self-review of the original illustration integration. This evidence predates the owner's requested eight varieties and motion/detail corrections and does not certify the revised implementation.

Visual acceptance: accepted 2026-10-25 — the owner approved the rendered version and requested its publication. The verification records below retain their state at the time of inspection.

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

## Current verification — eight varieties and motion correction, 2026-10-25

Scope: the correction requested after ece4d72. This section supersedes the earlier verification for the changed motion and artwork. The earlier table remains dated evidence of the first implementation.

| Criterion | Status | Evidence |
|---|---|---|
| Eight distinct varieties | met | Eight identities assigned once each; 128 current poses inspected on two rendered contact sheets. |
| Existing activities and input | met | Eleven activities, nine gym waypoints, 360 generated pointer cases, seven-ball limit, keyboard and pause checks pass. Browser touch and click pass at 390 and 1440. |
| Natural jumps | met | Preparation, ballistic travel, rising/tucked/descending poses, planted landing and recovery. Five jump triggers at four update rates; twenty simulated minutes remain continuous and bounded. |
| Scratching | met | Forepaw frames alternate; rear support and world coordinates remain fixed. Shared scale and registered anchors keep the pair aligned; neighboring fragments excluded by clip polygons. |
| Gym, butterflies, blankets and shadows | met | Rendered scene inspected: padded timber platforms and rope posts, shaped fluttering wings, three woven blankets, radial elliptical shadows. Gym route unchanged. |
| Box and platform alignment | met | Rendered gym/box scene inspected, head above front wall and bodies resting on their supports. |
| Responsive geometry and accessibility | met | Mechanical runner and axe pass at 390, 768, 1440 and 1920; rendered captures inspected. |
| Missing artwork | met | Six browser scenarios, including all ten PNGs blocked; scene remains playable, no runtime exceptions. |
| Direct disk opening | pending verification | Relative local resources retained. The earlier browser policy block on file: remains; only HTTP runtime is certified. |

Historical discrepancy: the first run after changing motion failed `autonomous must preserve original simulation`. Expected digest: `66d33f9b275bbcc7467cb3dd874a6c6516274dc2310190cb64b8b1270083cb1a`; actual: `d73063760bdfe1a4e90a8258653242864aea1be4d3d74cfeca04403600c4fcf1`. The owner explicitly requested different jumps, so exact historical trajectories are no longer an acceptance criterion. `original-behavior.json` is unchanged and `--historical` retains that comparison. Activity, route and input assertions remain, with additional motion criteria in `tests/motion.cjs`. Art anchor assertions now cover registered rear-paw anchors rather than requiring every crop to be centered.

### UI verification: eight varieties and motion correction

Environment: Chromium via Playwright 1.64.0 and app browser, HTTP local preview without backend. Production has no test API; isolated test instrumentation controls diagnostic states.

Page measured: http://127.0.0.1:4175/ — actual index.html with isolated instrumentation for diagnostic states.

Plan: retain the initial verification matrix, add all eight varieties, both scratch frames, jumping phases and new scenery. Full input coverage at 390 and 1440; layout review at 768 and 1920. The generated style remains the approved rounded pixel art.

Mechanical checks: `comprobar-pantalla.mjs --url http://127.0.0.1:4175/ --anchos 390,768,1440,1920 --simultaneas 1` passes with no failures. Simulation, motion and art tests pass; art covers 131 rectangles and 1,572 anchor/mirror cases. `jscpd` reports no clones in changed executable code and tests; generated sprite metadata is reviewed through bounds/alpha checks and rendered poses. No new runtime dependencies or network calls.

| Width × height | State | Capture | Finding |
|---|---|---|---|
| 390 × 900 | Initial, touch and keyboard | .local/capturas-v2/1-390-inicial.png; .local/browser-v2/interaction-390.png | Fits with original portrait letterboxing; no horizontal overflow. |
| 768 × 900 | Initial and missing assets | .local/capturas-v2/2-768-inicial.png; .local/browser-v2/fallback-10-parque.png.png | New scenery visible; all-art failure remains playable. |
| 1440 × 900 | Poses, jumping, gym/box and input | .local/browser-v2/all-poses-0.png; .local/browser-v2/all-poses-1.png; .local/browser-v2/jump-sequence.png; .local/browser-v2/gym-box.png | Clean final crops, distinct coats, stable scratching support and recognizable jump poses. |
| 1920 × 900 | Initial | .local/capturas-v2/4-1920-inicial.png | Scene fits with original side letterboxing; butterflies and textile details legible. |

Controls exercised: touch/click throw and nearest-cat pursuit, arrows/Enter aim and throw, Space pause/resume, repeated throws capped at seven. Frequent task walked at both full-treatment widths; app browser also opened the actual park. Reduced motion begins paused. Focus treatment and accessible description are retained. No variable text or catalog stress page applies; no scroll effects.

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Throw yarn | Mouse and touch | Ball added at delivered pointer coordinates; eligible cats chase it at 390 and 1440. |
| Aim and throw | Arrows, Enter | Aim moved and ball appeared at x=134 after ArrowRight and Enter. |
| Pause and resume | Space | Running state toggled twice; reduced-motion startup stayed at time zero. |

Look and differences: the approved landscape stays unchanged. Five additional coats join the original three; jumps now change body posture, scratching moves forepaws, the structure has warm wood/rope/cushions, butterflies have curved fluttering wings and three blankets sit behind the actors. Shadows fade through alpha rather than ending at rectangular edges. These are the owner's requested corrections; final visual acceptance is pending.

| Round | Failures found | Corrections | Cells checked again |
|---|---|---|---|
| 1 | Generated scratching pairs mirrored bodies or omitted tails; bounding crops included neighboring fragments. | Dedicated lateral scratching sheet, generated tail correction, shared scale, rear-paw anchors and component-derived clip polygons. | All eight scratch pairs on the rendered contact sheet and gym scene; art bounds and browser tests. |
| 2 | No remaining HTTP runtime, responsive, input or source-crop failure. | No further changes required by checks. | Four widths, 128 current poses, six fallback scenarios and motion tests. |

Visual inspection: passed — final HTTP render, contact sheets, gym/box and responsive captures inspected. Human visual acceptance and direct disk verification remain pending.

Review: accepted for the verified HTTP implementation — substantive self-review against ece4d72, including all new drawing code, motion transitions, tests, generated metadata and documentation. This is a reversible local animation change without data, external consumers or new trust boundaries, so independent review is not required. Existing drawing helpers were examined before adding the scenery module; original fallback remains. Historical test change is justified above. No blocking finding in the implemented HTTP scope; the spec stays open for visual acceptance and the explicitly unverified file: path. No remote publication requested.

Publication decision, 2026-10-25: the subsequent explicit request accepts the visual result and authorizes publishing the reviewed HTTP implementation. The repository was clean at 83b01ac; origin/main was still 5cf72af, with no divergent commits. Pages already serves main at `/`. Prior implementation evidence remains applicable because this publication changes no runtime files. The remaining file: verification limit is unrelated to the requested HTTPS deployment and is not reported as resolved. This documentation-only acceptance update was self-reviewed against the owner's message.
