# Ciclo aprobado y siete variantes — 25/10/2026

Se integra el naranja V2 aceptado por el propietario y se extienden sus ocho fases a gris, calicó, negro, blanco, blanco y negro, siamés y marrón. El cambio parte de c9cdf85. La app conserva el parque, el panel y el selector de 30/60 FPS; cambia el arte de caminata, su registro horizontal y la longitud de zancada. La aceptación visual de las siete variantes sigue pendiente del propietario.

## Evidencia funcional

- Compilación Release y publicación dependiente de .NET: cero advertencias y errores.
- El paquete publicado ejecutó su prueba interna: 1.779.531 comprobaciones aprobadas. Incluye los 64 cuadros completos, imágenes congeladas, proporciones, márgenes, suelo y anclaje horizontal común.
- Contratos independientes: 296.268 aprobados sobre el cambio de registro y zancada, incluida equivalencia de movimiento a 30/60, periodicidad y reposo. El arte posterior no cambió esas reglas.
- Las pruebas web de simulación, movimiento y arte pasaron; no se modificó el parque web. jscpd examinó los tres archivos C# afectados y no encontró clones.
- Los hashes de los ocho atlas coinciden entre fuentes, build y paquete. El ejecutable no depende de archivos ubicados en la carpeta de generación.

La inspección detectó poses simplificadas, manchas cambiantes y diferencias de escala en las primeras láminas. Se corrigieron imágenes completas individualmente y se normalizó su encuadre contra el naranja correspondiente. La última corrección ajustó el apoyo del cuadro 3 de negro y blanco y negro. Se repitieron exportación, comprobación del paquete y pruebas visuales después de esos cambios. Ninguna prueba fallida se debilitó; las aserciones anteriores de imagen completa, proporción y apoyo permanecen.

## UI verification: ocho ciclos completos

Environment: Windows 11, .NET 10 WPF; exportación real del renderizador y Chromium local con Playwright. HTML autocontenido abierto como file://, a 390 y 1440 px. La página demuestra el movimiento; no mide los FPS físicos de Windows.

Page measured: outputs/caminata-todos/index.html; los dibujos provienen de WalkingArt y SpriteCatalog del mismo arte publicado. La vista muestra un ciclo en el lugar y otro desplazándose, sincronizados.

Approved reference: outputs/caminata-naranja-v2/index.html y sus ocho PNG, aceptados explícitamente por el propietario. Los hashes de sus fuentes y de cada variante quedan en walk-cycle-provenance.json. El naranja sólo se reduce proporcionalmente para armar el atlas.

Plan: conservar la composición de la vista aprobada; revisar las cuatro patas y la continuidad de cada pelaje, el suelo y el encuadre; probar los ocho selectores, ambos recorridos y controles a 390/1440 px.

| State | Widths |
|---|---|
| Ocho pelajes, ocho cuadros, pausa y reproducción, lenta/normal, tamaño y tema | 390 / 1440 px |

Controls exercised:

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Pelaje | Elegir los ocho | Cada secuencia permite visitar sus ocho cuadros. |
| Anterior / siguiente | Clic en extremos; Enter con foco | 1→8 y 8→1; Enter avanza al 2. |
| Miniaturas | Elegir las ocho para cada pelaje | Número de cuadro correcto y pausa. |
| Reproducir / pausar | Clic, espera y pausa | El cuadro deja de avanzar al pausar. |
| Repetir | Clic | Vuelve al cuadro 1. |
| Velocidad y tamaño | Lenta / referencia / doble | Opciones aplicadas y render inspeccionado. |
| Tema | Dos clics | Cambia a oscuro y vuelve a claro. |

Walk of the frequent task: seleccionar un pelaje, mirar ambas vistas, reducir velocidad, pausar, seguir una pata por los ocho cuadros y repetir. Inspección estática de los 64 dibujos exportados y de los cuadros fuente que generaron dudas.

Accessibility floor: axe WCAG2 A/AA y 2.1 AA sin infracciones en ambas anchuras; sin errores de página ni desbordes. Etiquetas, texto de estado, botones de al menos 44 px, foco visible y avance por teclado. La preferencia de movimiento reducido inicia pausado.

Look: la ilustración muestra el cuerpo y las patas unidos. El pelaje conserva su identidad durante el ciclo; el registro uniforme de la imagen entera reduce cambios de tamaño entre fuentes. Las capturas de la página y las imágenes de producción permiten comparar el naranja y las siete variantes sin un modelo articulado paralelo.

Differences from the approved reference: colores y marcas de los siete pelajes; reducción proporcional al tamaño de escritorio y registro de encuadre. Se conservan pequeñas variaciones del dibujo y del apoyo entre cuadros. La revisión cualitativa sigue contacto, paso y recuperación de las cuatro patas; no acredita una simulación físicamente exacta ni la aceptación estética del propietario.

Mechanical checks: node work/check-all-cycles.cjs, salida 0; registros y capturas en work/all-cycles-checks. Ocho pelajes por ocho selecciones en ambas anchuras, sin errores, desbordes ni infracciones axe. La política del navegador integrado había denegado abrir file://; se entrega el enlace al archivo y no se presenta una apertura automática como realizada.

La tabla final de consumo desplegada y el aviso histórico de la guía anterior también pasaron axe y ausencia de desbordes a 390/1440 con work/check-cycle-delivery.cjs. Se inspeccionaron sus cuatro capturas. La redacción del comparador no produjo avisos; check-registro informó un registro y cero fallas. La guía conserva sus datos históricos y enlaza la versión actual, sin atribuir las cifras anteriores a este arte.

Visual inspection: passed for owner evaluation — inspeccionados los 64 cuadros del renderer final y los renders a 390/1440. Las imágenes conservan el gato completo y las cuatro fases desfasadas de apoyo y recuperación. No se obtuvo una captura nueva de la superposición pasiva sobre otros programas; la evidencia visual nueva es la exportación WPF y su reproducción comparativa.

## Consumo y convivencia

Las doce combinaciones terminaron. El resumen actual es cycle-measurement-summary.json, con el SHA256 del ensamblado medido. Registra CPU, memoria privada/residente y dibujos por segundo en escritorio y parque. El objetivo de 30 produce aproximadamente 30 dibujos/s; el de 60 produjo 48–50 en esta corrida. No se presenta el objetivo como frecuencia alcanzada. Todas las fases ocultas tienen cero cuadros y pasos; ninguna muestra dio foco a la superposición. GPU no disponible. Una repetición por caso; no representa consumo universal.

Se reutilizan únicamente las cuatro muestras de un gato naranja, cuyo arte y código permanecieron iguales; SpriteCatalog.Load(1) no carga los pelajes corregidos. Las ocho de 4/8 gatos se repitieron después de la última corrección de negro y blanco y negro. Los registros crudos finales están en work/cycle-measurements-final; los de la corrida anterior se conservan separados. No hubo automatización de interfaz durante las muestras.

Los gestos de clic, escritura y Alt+Tab se respaldan en VERIFICACION-CAMINATA.md y VERIFICACION-PARQUE.md. No se repitieron esos gestos manuales en esta corrección: sus rutas, filtros de entrada y estilos de ventana no cambiaron. Las activaciones y el estado oculto sí se verifican de nuevo en la medición. Los contadores de GPU no devolvieron muestras en esta corrida; no se informa consumo cero.

## Revisión

Revisión propia: cambio local, reversible, sin contratos externos ni datos de usuario alterados. Se revisaron los tres archivos de código, los 64 cuadros, la documentación y los hashes del paquete. Se buscó la clase de afirmaciones obsoletas sobre las láminas previas: WALK-ART y README se actualizaron; VERIFICACION-IMAGENES y la guía HTML inicial se rotularon como históricas. No se atribuye la aprobación del naranja a los siete pelajes nuevos. Unidad implementada y verificada para valoración del propietario; la spec permanece abierta por aceptación visual. La calidad del movimiento dibujado no admite una prueba mecánica suficiente: no se propone convertir el juicio estético del propietario en una aserción de carga de archivos.
