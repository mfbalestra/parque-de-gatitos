# Caminata con imágenes completas — 25/10/2026

Registro histórico de c9cdf85. El propietario rechazó posteriormente la lógica de estas poses. Sus pruebas de carga y controles no acreditaron la calidad del ciclo. La versión basada en el naranja V2 aprobado se documenta en VERIFICACION-CICLO.md; las cifras de este documento no se atribuyen al arte nuevo.

El propietario rechazó el torso fijo con patas articuladas de 88908cf y eligió las imágenes completas. Se reemplazaron las ocho láminas por sus fuentes originales de ocho poses, incluida la naranja adjunta. Cada cuadro es un único ImageDrawing congelado: sólo se recorta el margen transparente y se registra la pose al suelo, con escala uniforme por pelaje. El código de movimiento, los controles, la persistencia, el ciclo del parque y los filtros de entrada permanecen iguales.

## Pruebas y revisión

Release: cero advertencias y errores. Prueba interna: 1.779.467 aserciones aprobadas, con los 64 cuadros completos, proporción conservada, márgenes y suelo alineados. Contratos independientes: 296.268 aprobados, incluida periodicidad, ocho cuadros alcanzables, fase inmóvil en reposo y equivalencia de movimiento entre 30 y 60 FPS.

Se retiró la fórmula de patas del armado anterior y sus aserciones de compensación matemática porque el propietario pidió expresamente sustituir ese método por imágenes. No había una prueba fallando que se quisiera hacer pasar. La cobertura pertinente se reemplazó por comprobaciones sobre las imágenes completas y su registro; las pruebas de distancia, reposo y equivalencia permanecen. La calidad anatómica y el apoyo que dibujó el ilustrador requieren inspección visual: un test de coordenadas del viejo esqueleto no los acredita.

Durante la primera exportación, PreserveNewest conservó copias del arte anterior porque los PNG originales tenían una fecha previa. Se corrigieron las fechas de los archivos copiados, se recompiló y se inspeccionó la exportación nueva. Los bytes de las láminas coinciden con las fuentes; su procedencia y hashes están en WALK-ART.md.

El binario de esta corrección tiene SHA256 7F9A30CD2BD35633108B156B64CD167AE3F9BA76B27979D56E35F9FAA0EC3FFC. El resumen de doce mediciones nuevas se conserva en walk-measurement-summary.json, identificado por este hash. Las cifras del armado anterior son históricas y no se atribuyen a este dibujo.

Se reutiliza la evidencia de panel, guardado, teclado, clics y cambio de ventanas de VERIFICACION-CAMINATA.md y VERIFICACION-PARQUE.md: esta corrección no modifica esas rutas ni los estilos nativos de ventana. No se presenta esa evidencia como una nueva ejecución de todos esos gestos.

## UI verification: imágenes completas

Environment: Windows 11, .NET 10 WPF; monitor principal 1920 × 1080 al 100 %. Comparación HTML local de los cuadros exportados directamente desde WalkingArt y SpriteCatalog de producción; su desplazamiento es demostrativo.

Page measured: ejecutable Release de esta corrección y outputs/caminata-preview/index.html, servido en 127.0.0.1:4173 para las pruebas de navegador.

Approved reference: lámina naranja adjunta por el propietario; ocho fuentes y SHA256 documentados en WALK-ART.md. Se usan sus poses completas sin repintar partes. La elección de fuente no se cuenta como aceptación de la animación final.

Plan: inspeccionar los 64 cuadros, la comparación a 390/1440 px y la caminata en el parque real. Ejercitar los selectores y la reproducción. Reutilizar la evidencia de las preferencias y del paso de entrada, cuyo código no cambia. Medir las doce combinaciones sin automatización de interfaz concurrente.

| State | Widths |
|---|---|
| Comparación, ocho pelajes, reproducción normal/lenta, pausa y paso | 390 y 1440 px |
| Estado del parque y panel del paquete | Monitor nativo 1920 × 1080 |

Controls exercised:

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Pelaje | Seleccionar los ocho | La prueba observó cada estado y avanzó al cuadro 2. |
| Siguiente cuadro y Repetir | Clic | La prueba pasó de cuadro 1 a 2 y volvió a 1. |
| Velocidad, tamaño y reproducción | Seleccionar lenta, referencia, reproducir y pausar | La prueba ejercitó las opciones y observó el estado pausado final. |
| Tema | Dos clics | La prueba comprobó el atributo de tema oscuro y regresó a claro. |
| Salir del paquete nativo | Botón Salir de los gatitos | Se cerró la instancia de prueba; el panel había mostrado ocho gatos en el parque y 60 FPS. |

Walk of the frequent task: elegir el pelaje, avanzar un cuadro, repetir y comparar el ciclo anterior y el de imágenes completas. La nueva versión conserva el mismo acceso al parque y a las preferencias.

Accessibility floor: la comparación conserva pausa, paso manual, reducción de movimiento, etiquetas, botones de 44 px y estado por texto. Playwright y axe pasaron a 390/1440 px, sin errores de página, desbordes ni infracciones de las reglas WCAG ejecutadas.

Look: cuerpo, cabeza y patas pertenecen a una sola ilustración en cada cuadro. Se conserva el estilo y los contornos de las fuentes elegidas. Las diferencias entre poses provienen de esas láminas; no se promete un contacto físico continuo perfecto entre ocho dibujos discretos.

Differences from the approved reference: reducción proporcional para el tamaño de escritorio y recorte del espacio transparente exterior. El pie se registra a una línea común, sin deformación de partes. La comparación anterior usa la exportación del armado de 88908cf, no las poses antiguas de dos cuadros.

Mechanical checks: build, contratos y comparación aprobados. Las doce mediciones terminaron con cero dibujos/pasos ocultos y cero activaciones de la superposición. Las tres pruebas web pasaron sin cambios; jscpd informó cero clones en los cuatro archivos C# afectados. El hook validó el mensaje del commit fuera del sandbox; dentro, bash no pudo crear su objeto de sistema.

Visual inspection: passed — se inspeccionaron los 64 cuadros exportados por el dibujante WPF de producción y la comparación a 390/1440 px. Se abrió el paquete final y se observó el panel con ocho gatos en el parque y 60 FPS. Límite: Computer Use sólo expuso el panel, no la ventana decorativa pasiva; esta revisión no acredita una captura nueva de los gatos sobre el parque real. La evidencia visual nueva corresponde al renderer exportado y la comparación; el juicio final del ciclo sigue pendiente del propietario.

Review: revisión propia aceptada para valoración visual, sobre el cambio desde 88908cf. Se revisaron imágenes completas, recortes, proporción, caché congelada, registro al suelo y fase por distancia. No altera permisos, persistencia ni contratos externos. Se buscó la clase de descripciones obsoletas sobre piezas/torso fijo en desktop y la spec: README, WALK-ART y la spec se actualizaron; VERIFICACION-CAMINATA se rotuló como histórica. No quedaron hallazgos funcionales. La preferencia por imágenes completas queda comprobable en el test de carga; la valoración estética del ciclo queda a cargo del propietario. El límite de captura nativa anterior sigue declarado.
