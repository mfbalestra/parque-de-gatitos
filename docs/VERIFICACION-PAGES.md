### UI verification: controles y ciclo de compañía para GitHub Pages

Los JSON, módulos y capturas citados se conservaron como evidencia local bajo `.local/verificacion/`; no son recursos necesarios para ejecutar la página pública. Las pruebas reproducibles de la lógica viven en `tests/`. Este registro describe el corte previo a publicar; el estado del despliegue se consulta en GitHub Pages.

Environment: Chromium 156.0.8078.4, Playwright 1.64.0 y axe-core global, Windows; versiones comprobadas en la instalación local y por el navegador ejecutado. Route: preview del sitio estático real; no hay API, base de datos ni información personal. Content: ilustraciones reales del proyecto y preferencias de prueba. Coverage: 8 estados × 4 anchos = 32 celdas, más transiciones y altura reducida. Fecha: 2026-10-31. Base: rama mejora-gatitos-ilustrados, HEAD 647d455f3f0e02da3d82958ed04dddd3ae228ab8; cambios de la unidad aún sin commit durante la inspección.

Page measured: http://127.0.0.1:4180/, index.html real del proyecto en su marco completo. El servidor pertenece a la sesión raíz; este agente no inició procesos. No se usó HTML sintético para las mediciones.

Approved reference: none — ruta directa sobre el patrón del parque existente en 647d455. El encargo conserva la composición y las ilustraciones, incorpora controles sobrios y adapta la compañía a esta pestaña. La caminata parte del naranja V2 aprobado; los 64 dibujos fueron inspeccionados por la sesión raíz. No hubo maqueta nueva aceptada ni comparación de píxeles contra una maqueta. La aceptación estética final del propietario sigue pendiente.

Plan, written before the first check: pages-plan.md. Tratamiento completo a 390 y 1440 px; revisión a 768 y 1920 px. Checker a 900 px de alto; recorridos adicionales a 390×844, 1440×844 y 390×360. El sitio conserva el encuadre 16:9 existente; en móvil vertical se ve completo, con bandas de fondo. Recorridos: abrir/cerrar; validar/corregir/guardar/recargar; cambiar 1/4/8 en compañía; visitar/volver/revisitar; pausa y ocultación; almacenamiento fallido; movimiento reducido; teclado y continuidad.

| State | Widths |
|---|---|
| Parque inicial | 390 y 1440 completo; 768 y 1920 revisión |
| Diálogo abierto, cantidad bloqueada | 390 y 1440 completo; 768 y 1920 revisión |
| Validación de ambos tiempos | 390 y 1440 completo; 768 y 1920 revisión |
| Error de escritura de preferencias | 390 y 1440 completo; 768 y 1920 revisión |
| Compañía, cantidad habilitada | 390 y 1440 completo; 768 y 1920 revisión |
| Pausa | 390 y 1440 completo; 768 y 1920 revisión |
| Escena oculta | 390 y 1440 completo; 768 y 1920 revisión |
| Movimiento reducido | 390 y 1440 completo; 768 y 1920 revisión |

Mechanical checks: `node ~/.agents/skills/implementar-interfaz/scripts/comprobar-pantalla.mjs --url http://127.0.0.1:4180/ --anchos 390,768,1440,1920 --estado .local/verificacion/pages-ESTADO.mjs --capturas .local/verificacion/CARPETA`; estado inicial sin --estado. Los módulos son dialog, errors, storage, companion, paused, hidden y reduced. Todas las 32 celdas finales tienen `fallas: []`: overflow, mediciones, foco de controles, axe y consola sin fallos. Evidencia vigente: check-initial.json; final-dialog.json; final-storage.json; final-companion.json; final2-errors.json; final2-paused.json; final2-hidden.json; final2-reduced.json. Las celdas previas se reutilizan cuando no cambió su región: el último ajuste sólo desplazó el aviso de pausa/ocultación y el campo inválido. Los informes superficie-oscura son informativos; no hubo avisos de agrupación. El aviso de foco sobre canvas requiere inspección aparte porque su fondo es una imagen.

Comandos de comportamiento, ejecutados por el transcriptor desde el repo: `node ../browser-pages.cjs`, `node ../pages-transitions.cjs`, `node ../pages-extra.cjs`, `node ../pages-final2.cjs`. Resultados: behavior.json, 9 casos reproducidos; transiciones.json, recorridos a 390 y 1440 reproducidos; extra.json, 2 casos reproducidos; final2-behavior.json, teclado/errores/guardado a 390×360 y 1440×844 reproducidos. `node tests/web-controls.cjs`: 8 grupos aprobados, incluidos guardado atómico, continuidad de actores, temporizadores, pausa/ocultación/visibilidad, 30/60 equivalentes y cadencia sobre 59,94/60/75/120/144 Hz con fluctuación. La regresión restante fue ejecutada y comunicada aprobada por la sesión raíz; este registro no la presenta como ejecución propia. No aplica check:entrega: sitio sin ese kit. No hay listas crecientes: máximo 8 gatos y 7 ovillos, verificados por pruebas.

Prueba adicional: `node ../pages-canvas.cjs`, canvas-input.json, aprobada a 390 y 1440: Tab al canvas, flecha+Enter, Espacio pausa/reanuda, escritura de espacio en formulario, clic en parque, botones sin ovillos y bloqueo de teclado/clic en compañía. Capturas canvas-foco-390.png y canvas-foco-1440.png inspeccionadas: doble contorno blanco/oscuro visible sobre la escena. Fundido de salida inspeccionado a opacidad 0,5 en salida-fundido-mitad-1440.png; la lectura exacta se conserva en su JSON.

Límites de la prueba: los botones, teclado, escritura y selectores se ejercitaron mediante entradas de Playwright. Las capturas de transición usan una ruta que añade observabilidad y llamadas a step/render sólo en la respuesta de prueba; los recorridos se avanzan en pasos de 1/60 y no acreditan una espera real de 30 minutos. El módulo companion acelera el ciclo sólo durante la medición. El conteo de ovillos de la prueba de canvas usa observabilidad inyectada, con entradas reales. Storage corrupto/denegado se simula en el contexto de prueba; document.hidden se simula despachando visibilitychange y verificando el reloj, no mediante Alt+Tab real. No hay hooks de diagnóstico en producción. Inherited outside the changed region: no fallo mecánico bloqueante; el encuadre 16:9 y las bandas exteriores se conservan.

| Width × height | State | Capture | Finding |
|---|---|---|---|
| 390×900 y 1440×900 | Inicial | capturas-initial/1-390-inicial.png; 3-1440-inicial.png | Parque dominante, barra compacta; sin cortes |
| 390×900 y 1440×900 | Diálogo | final-dialog/1-390-dialog-open.png; 3-1440-dialog-open.png | Columnas alineadas; contenido desplazable; Guardar permanece visible |
| 768×900 y 1920×900 | Diálogo | final-dialog/2-768-dialog-open.png; 4-1920-dialog-open.png | Revisión visual de reflujo sin desborde |
| 390×900 y 1440×900 | Validación | final2-errors/1-390-validation-errors.png; 3-1440-validation-errors.png | Campo enfocado y mensaje completo tras corrección |
| 390×900 y 1440×900 | Error de almacenamiento | final-storage/1-390-storage-write-error.png; 3-1440-storage-write-error.png | Mensaje persistente y Guardar accesible; el cuerpo admite scroll |
| 390×900 y 1440×900 | Compañía con diálogo | final-companion/1-390-companion-controls.png; 3-1440-companion-controls.png | Fondo tranquilo y selector habilitado; estado explícito |
| 390×900 y 1440×900 | Pausa | final2-paused/1-390-paused.png; 3-1440-paused.png | Aviso arriba, sin tapar gatos |
| 390×900 y 1440×900 | Oculto | final2-hidden/1-390-hidden.png; 3-1440-hidden.png | Mostrar y Controles siguen disponibles |
| 390×900 y 1440×900 | Movimiento reducido | final2-reduced/1-390-reduced-motion.png; 3-1440-reduced-motion.png | Pausa explícita; escena cargada, sin blanco inicial |
| 390×360 | Diálogo bajo | dialog-390x360-save.png; final2-error-completo-390.png | Guardar permanece visible; el error completo y su campo caben en la región desplazable |
| 1440×844 | Validación | final2-error-completo-1440.png | Mensaje de dos líneas completo; ya no queda debajo del pie |
| 390×360 y 1440×844 | Compañía pausada | final2-franja-pausada-390.png; final2-franja-pausada-1440.png | Aviso superior separado de la franja de gatos |
| 390×844 y 1440×844 | Entrada y salida | transicion-ANCHO-{entrada,salida}-{inicio,mitad,fin}.png | Doce capturas revisadas; continuidad visual y fade de entrada legible; aviso inferior de esa ronda corregido en final2 |

Controls exercised: entradas reales del navegador, salvo donde se explicita harness o avance inyectado.

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Abrir Controles | Click y Enter | behavior.json y final2-behavior.json: abrió el diálogo |
| Cerrar | Click y Escape | behavior.json: cerró y restauró foco a Controles |
| Pausar/Reanudar en barra y diálogo | Click y Enter | behavior.json y final2-behavior.json: cambió texto y estado; pruebas de lógica verifican reloj/RAF detenido |
| Ocultar/Mostrar en barra y diálogo | Click y Enter | behavior.json y final2-behavior.json: ocultó/mostró sin perder la pausa previa |
| Volver a compañía | Click | transiciones.json: fase companion alcanzada con avance inyectado, mismos actores; caja liberada en prueba de lógica |
| Ir al parque ahora | Click repetido | transiciones.json y extra.json: visita preparada durante salida; volvió al parque con avance inyectado |
| Cantidad 1/4/8 | selectOption y click Guardar | transiciones.json: conteo observable 1, 4 y 8; selector bloqueado en parque y habilitado en compañía |
| Fluidez 30/60 | selectOption y click Guardar, recarga | behavior.json: selección persistió en ambos casos; velocidad y cadencia comprobadas por pruebas de lógica |
| Minutos de compañía | fill, click Guardar | behavior.json: 241 rechazado; 30 aceptado; final2: campo y error completos, foco conservado |
| Minutos de parque | fill, click Guardar | behavior.json: 0 rechazado; 5 aceptado; prueba de lógica cubre enteros y límites |
| Guardar preferencias | Click y Enter | behavior.json y final2-behavior.json: mostró confirmación; error de almacenamiento conservó preferencias anteriores |
| Scroll del diálogo y foco inválido | Foco/scroll automático del navegador, desplazamiento explícito al label | final2-behavior.json: error y campo dentro del diálogo; Guardar de 44 px visible a 390×360 y 1440×844 |
| Flechas, Enter y Espacio del canvas | Tab, teclado y puntero reales a390/1440; conteo y avance a compañía inyectados | canvas-input.json: apuntó/lanzó en parque; Espacio pausó/reanudó; compañía ignoró clic/teclas; espacio en formulario no pausó |

Walk of the frequent task: abrir Controles → editar tiempos y fluidez → guardar → recargar → pausar → ocultar/mostrar, con puntero y teclado en Chromium, zoom predeterminado. En 390×360 y 1440×844 se repitieron error/corrección/guardado por Enter y cierre por Escape. Guardar y Cerrar siempre alcanzables; la lista de preferencias desplaza su cuerpo sin desplazar el pie. Las opciones tienen rótulos claros y el siguiente paso queda junto al estado. Los avisos describen el resultado y la recuperación. No se probó zoom de sistema ni dispositivos táctiles físicos; los objetivos táctiles se midieron a los anchos móviles.

Accessibility floor: checker y axe sin fallos en las 32 celdas. Pares calculados desde los colores efectivos del CSS: texto #2e3828 sobre #fff9ed 11,69:1; ayuda #536048 sobre #fff9ed 6,39:1; blanco sobre botón #38552d 8,38:1; foco #183b77 sobre #fff9ed 10,37:1; error #8d241d sobre #fff9ed 8,33:1. Objetivos de al menos 44 px, controles nativos etiquetados, diálogo semántico y foco visible; los estados usan texto, no sólo color. Movimiento reducido inicia en pausa y permite reanudar deliberadamente. El foco de canvas conserva contorno blanco y borde oscuro #183326 (13,63:1 entre ambos); las capturas específicas canvas-foco-390/1440 muestran el contorno reconocible y resuelven el aviso informativo sobre fondo con imagen.

Look: el parque conserva protagonismo con controles cerrados; el panel tiene jerarquía sobria, tipografía del sistema, papel cálido y una sola acción principal verde. Compañía se identifica explícitamente como contenido de esta pestaña y mantiene los mismos personajes en una franja del canvas. No se agregan adornos genéricos, iconos improvisados, cifras ficticias ni nuevos assets de identidad. El panel puede servir a otro formulario, pero su agrupación y textos responden a las visitas, la pausa y los gatos. La imagen y la caminata continúan el arte aprobado. No se inventa una aceptación estética a partir del resultado mecánico.

Differences from the approved reference:

| Difference, where and at which widths and states | Cause | Evidence: reference → implementation | Resolved by |
|---|---|---|---|
| Barra y diálogo en todos los anchos | Alcance aprobado: controles web | Ausentes en la página previa → acciones, preferencias y errores verificables | Implementado por agente; aceptación estética pendiente del usuario |
| Franja neutra y fade del parque | Alcance aprobado: compañía dentro de pestaña | Parque único → mismo grupo alterna escenas | Implementado y probado; aceptación estética pendiente |
| Ocho cuadros de caminata | Paridad con Windows aprobada | Dos poses temporizadas → ocho poses según distancia | Implementado con arte entregado por raíz; límites de la ilustración conservados |
| Foco del error y aviso de pausa superior | Defectos observados durante inspección | Mensaje parcialmente oculto/aviso sobre gatos → label completo/aviso arriba | Corregido por agente y repetido en final2 |

Transitions: se inspeccionaron inicio, punto intermedio y final de entrada/salida a 390 y 1440. La entrada intermedia muestra el fade a un segundo; la salida intermedia muestra desplazamiento a dos segundos y el final demuestra vuelta a franja. Se agregó salida-fundido-mitad-1440.png con phase disappearing y opacity 0,5000000000000018: gatos nítidos sobre fondo a media opacidad, sin superposición del aviso. La continuidad física y el descenso se comprueban además en pasos de 1/60, incluidos saltos activos. No se presenta el punto de desplazamiento de salida como la mitad exacta de un trayecto de duración variable. La pausa/reduced motion conserva la escena, y una petición de visita durante salida queda encolada. Las capturas fueron obtenidas con observabilidad y avance inyectados únicamente en pruebas.

Scroll effects: no se añadieron efectos de scroll; el cuerpo del diálogo se desplaza de forma nativa entre encabezado y pie fijos. El contenido recortado por su borde de scroll permanece alcanzable. Sin filas horizontales, parallax ni scroll vinculado a animación.

Stress page: no hay componente de catálogo que reciba texto externo sin límite. Se comprobaron los casos pertinentes en la página real: errores de ambos campos, mensaje largo de almacenamiento, cantidad máxima8, duración máxima240/60 y altura360. Sin contenido personalizado ni listas abiertas; no corresponde inventar un caso diez veces mayor que el dominio permitido.

| Round | Failures found | Corrections | Cells checked again, each with the correction that could alter it |
|---|---|---|---|
| 1 | text-wrap ausente en textos del diálogo y diferencia entre columnas | pretty/balance; separaciones coherentes y campos de duración alineados | Diálogo, errores, storage y compañía × cuatro anchos: final-*.json sin fallos |
| 2 | Inspección detectó mensaje inválido cortado por el pie y aviso de pausa sobre gatos | Mostrar label completo al validar; aviso a top76 con box-sizing | Errores, pausa, oculto y reduced × cuatro anchos: final2-*.json sin fallos; recorridos390×360/1440×844 y capturas finales revisados |
| Lógica | Preparación de test de 4 s fuera del rango de timestep; pérdida de cuadros con jitter en primer pacer | Preparación en240 pasos fijos; deadline con crédito acotado | tests/web-controls.cjs: ocho grupos aprobados, sin quitar ni debilitar aserciones |

Visual inspection: passed. Estados, foco de canvas y correcciones inspeccionados; sin hallazgos abiertos en el alcance. La aceptación estética del propietario y la publicación corresponden a la sesión raíz.

Visual acceptance: pending the user.
