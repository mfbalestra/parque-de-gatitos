# Bring the Windows improvements to GitHub Pages

Status: in progress — branch mejora-gatitos-ilustrados, 2026-10-31
Summary: Llevar los ocho ciclos de caminata y las funciones trasladables de Windows al parque web, conservar sus juegos y publicar la versión comprobada en GitHub Pages.
Orchestrator: Full — artwork, browser lifecycle, interaction and deployment must agree.
Blocked by: none

## Intent

The owner authorized adaptation and publication: «Ok, hacé eso. Y no sé si quedó algo más que solo se hizo en Windows y no se llevó a la github pages, pero si quedó, adaptalo a pages y llevalo». Audit every native addition since published 9df3aa1; use the complete walking poses from 647d455 without regenerating artwork. Preserve the web playground's existing activities and inputs. The native implementation remains available.

## Acceptance criteria

- WHEN a cat walks on the ground or a platform, THE SYSTEM SHALL use its eight complete poses with phase driven by actual walking distance.
- WHILE resting, jumping or sliding, THE SYSTEM SHALL preserve the walking phase without advancing it by time.
- WHEN opening the page for the first time, THE SYSTEM SHALL show the park with eight cats and a 60 FPS target, retaining the previous web entry experience.
- WHEN opening controls, THE SYSTEM SHALL offer 1/4/8 cats, 30/60 FPS, companion and park durations, pause, hide and immediate transitions.
- WHEN saving valid preferences, THE SYSTEM SHALL persist them in the browser together without resetting elapsed time or replacing unchanged cats.
- IF storage or validation fails, THE SYSTEM SHALL explain the failure without partially applying preferences.
- WHEN companion time expires, THE SYSTEM SHALL fade in the park and move the same cats into it, starting the stay after all arrive.
- WHEN park time expires or the user requests a return, THE SYSTEM SHALL return the cats to the companion strip and fade out the park.
- WHILE paused, hidden or in a background tab, THE SYSTEM SHALL suspend animation and timers and resume without accumulated catch-up.
- WHEN using the park, THE SYSTEM SHALL retain yarn, keyboard aiming, jumps, gym, scratching, blankets, butterflies, shadows and the original eight identities.
- WHEN using narrow or wide screens, THE SYSTEM SHALL keep controls readable, keyboard-operable and accessible, respecting reduced motion.
- WHEN publishing, THE SYSTEM SHALL serve the verified commit and all its required assets from GitHub Pages.

## Decisions

- Existing illustrated park identity remains. A compact controls dialog follows the sober native settings pattern.
- Companion mode runs inside this tab. A normal website cannot implement the native overlay, tray, click-through or operating-system focus behavior.
- Keep web defaults (eight cats, 60 FPS); reuse native duration defaults (30 and five minutes) and validation ranges. Count changes are available during companionship.
- Existing web simulation is retained; a fixed simulation clock separates motion from rendering rate. Native trajectories are not substituted for the web's richer interactions.
- Browser-local preferences belong to this origin. The native file is not imported or overwritten. Timers count visible, unpaused use only.
- Existing approved images are mechanically resized for browser delivery with provenance; no new generation or assembly of separate limbs.
- Publication and push are explicitly authorized by the current request. Remote main must remain an ancestor; no force push.
- Visual acceptance: orange reference accepted earlier; final browser adaptation pending owner evaluation, publication explicitly authorized.

## Out of scope

Operating-system overlay/tray, background services, global input capture, accounts, cloud settings, installation, new artwork and unrelated standards migration. Browser measurements do not inherit WPF performance claims.

## Slices

1. Complete walking resources and distance registration in the existing web renderer.
2. Browser controls, persistence, companion/park lifecycle and regression verification.
3. Human documentation, final review, local commit, push and verified Pages deployment.

## Criteria status

| Criterion | Status | Evidence |
|---|---|---|
| Eight whole-pose cycles / stationary phase | met | tests/walk-web.cjs: 48,128 generated checks; tests/web-controls.cjs excludes jumps, rolls and slides from stride. All 64 rendered drawings inspected. |
| Existing park and eight-cat entry | met | simulation/motion/art regressions pass, including every activity, five jump triggers and gym waypoints. Native defaults are deliberately adapted to existing web defaults. |
| Settings, persistence and elapsed time | met | Generated validation/lifecycle checks plus browser save/reload, error/recovery, counts 1/4/8 and preserved identities. |
| Companion / timed and immediate visits | met | Fixed-step tests of automatic clocks, interrupted entry, active jumps, platform descent and queued revisit; browser transition captures. |
| Pause / hide / background | met | RAF cancellation and no-catch-up contracts; browser controls and simulated visibility. No native OS overlay claim. |
| FPS independence and pacing | met | Same actor state at 30/60; generated callbacks at 59.94/60/75/120/144 Hz with jitter retain cadence. Physical display FPS are not inferred. |
| Accessible controls and responsive states | met | 32 final checker cells at 390/768/1440/1920, no failures; actual keyboard/pointer journeys and 390×360 error/Save visibility. See docs/VERIFICACION-PAGES.md. |
| GitHub Pages delivery | partial | Implementation verified locally; current request authorizes push to main. Final delivery requires built status for the new commit and remote byte/interaction readback. |

Review: accepted implementation — self-review of the complete diff against 647d455, including generated art provenance and every portable native feature since published 9df3aa1. Local reversible preferences and presentation, no new authentication, external data, dependencies or native runtime changes. Five test programs pass; jscpd reports zero clones. The original full-image and motion assertions remain. Deployment readback is the remaining delivery action, not a claim made by local tests. Aesthetic acceptance remains with the owner; publication is already authorized.

Review corrections: cadence drift under jitter was fixed without relaxing its generated checks; the invalid-field label now scrolls fully into view; notices no longer cover the companion strip. Existing simulation harness now represents DOM, storage and lifecycle; its resume assertion waits one full 1/60 step (17 ms). The new proportion check uses 1e-12 tolerance for floating-point division (0.11499999999999999 versus 0.115), preserving the requirement of uniform image scale. No historical trajectory fixture or failing criterion was replaced.
