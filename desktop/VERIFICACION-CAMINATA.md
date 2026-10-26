# Caminata, fluidez y preferencias — evidencia histórica de 88908cf

La caminata por piezas documentada aquí fue rechazada por el propietario y reemplazada por imágenes completas. Consultar [VERIFICACION-IMAGENES.md](VERIFICACION-IMAGENES.md) para el dibujo y consumo actuales. Las comprobaciones de controles y entrada se conservan como evidencia de componentes que esta corrección no modifica.

Alcance aprobado: ocho cuadros en los ocho pelajes, objetivo guardable de 30/60 FPS y controles nativos agrupados. Corrección posterior del propietario: las patas se veían desconectadas. Las patas cercanas ahora se superponen al torso con una transición de opacidad en hombros y caderas; las lejanas quedan detrás. Se inspeccionaron los 64 cuadros exportados del mismo dibujante WPF. La aceptación visual del propietario sigue pendiente.

## Evidencia funcional y límites

La caminata usa piezas generadas y conserva un cuerpo estable. Se descartaron láminas completas que repetían posiciones de patas. El ciclo tiene cuatro apoyos desfasados; el tramo apoyado compensa el avance del cuerpo y el retorno levanta la pata. Ocho dibujos discretos implican pequeños pasos visibles entre cuadros: no se promete contacto continuo perfecto. La dirección se invierte conservando la identidad.

Release compiló con cero errores y advertencias. La prueba interna pasó 1.779.275 aserciones; el ejecutable de contratos independientes pasó 306.668. Incluyen los ocho cuadros, periodicidad de zancada, apoyo, fase inmóvil en reposo, equivalencia 30/60, cadencia sobre distintas frecuencias simuladas de pantalla, archivos antiguos, guardado conjunto, rechazo de valores inválidos y fallo de escritura sin pérdida de preferencias. No se agregaron dependencias.

Una prueba nueva inicialmente encontró distinta posición entre 30 y 60 FPS al cruzar el despegue. Se corrigió la integración para dividir el paso en los límites de cada conducta; se mantuvo la expectativa original. La primera variante de esa corrección podía quedar iterando en un residuo temporal: se corrigió y el barrido completo volvió a pasar. No se omitieron ni debilitaron pruebas.

El resumen de consumo es `walk-measurement-summary.json`: doce casos (1/4/8 × 30/60 × escritorio/parque), un proceso nuevo y una repetición por caso. El hash identifica el binario medido. La frecuencia observada cuenta dibujos de WPF, no barridos físicos de pantalla. La CPU total se normaliza sobre 16 procesadores lógicos. Memoria residente y privada son medidas diferentes y no se suman. El objetivo de 60 FPS puede quedar por debajo según el compositor y la carga. Una repetición no caracteriza variabilidad ni prueba mejoras universales de consumo.

Las últimas centralizaciones de validadores y escala de pelaje conservan exactamente las expresiones anteriores; no alteran el algoritmo, los recursos ni la cadencia medidos. Las pruebas finales cubren esas centralizaciones. No se atribuyen cifras a una compilación distinta sin declarar esta reutilización.

Se encontró además un defecto previo en el recolector: al exceder su límite podía dejar la instancia propia abierta. El cierre en `finally` ahora se limita al objeto Process recién creado; no busca ni termina instancias del propietario.

## UI verification: controles nativos y comparación de caminata

Environment: Windows 11, WPF .NET 10.0.14, monitor principal 1920 × 1080, escala real 100 %. Computer Use sobre ventanas propias. La comparación HTML usa los dibujos exportados de producción; su movimiento es demostrativo.

Page measured: ejecutable Release local con `--panel`; comparación en `http://127.0.0.1:4173/caminata-preview/index.html`. La ventana decorativa conserva estilos pasivos y no recibe teclado; el panel es deliberadamente interactivo.

Approved reference: composición nativa sobria aprobada en el chat; propuesta `caminata-fluidez-controles.html`, revisión 1, SHA256 b830a4e2a25d02debec38a9c9fa2dbb75a7e0cd3b1a80b28a287d37ca8d89c93. Corrección de patas solicitada sobre la vista temprana; aceptación del resultado pendiente.

Plan: recorrer guardar válido/inválido, cambio de cantidad/frecuencia, pausa/ocultación y bloqueo de cantidad en el parque. Inspeccionar panel normal y angosto, representación de escala aumentada, ocho pelajes en movimiento, paso de entrada y Alt+Tab. Medir sin automatización UI concurrente. Comparación a 390/1440 px, reducción de movimiento, temas y controles.

| State | Widths |
|---|---|
| Panel normal, preferencias pendientes, error, pausa, parque | Ventana nativa, monitor real al 100 % |
| Panel angosto y escala aumentada | 340 DIP y render de los controles reales a 150 % |
| Comparación animada | 390 y 1440 px |

Controls exercised:

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Guardar preferencias inválidas | 0 en compañía y Alt+G | Mostró el rango específico, mantuvo valores aplicados. |
| Guardar tiempos | 30/5 y Alt+G | Confirmó guardado; modificó tiempo restante conservando lo transcurrido. |
| Selector de frecuencia | 60 → 30 y Guardar | Mostró cambios pendientes y confirmación; archivo leído con Fps 30. |
| Pausar | Botón | Cambió a en pausa y mantuvo 25:38 después de varios minutos. |
| Cantidad en el parque | Visita activa | Radios deshabilitados y explicación; FPS y tiempos continuaron habilitados. |
| Salir | Alt+S | Cerró la instancia propia antes de compilar la corrección. |
| Cantidad y persistencia final | Elegir 1, guardar; elegir 8, guardar y reiniciar | Cambió el grupo sin reiniciar el contador; apertura sin overrides recuperó 8 gatos y 60 FPS. |
| Error en el paquete final | Compañía 0 y Alt+G | Mostró el error junto al campo, lo enfocó y desplazó hasta hacerlo visible junto con Guardar. |
| Pausa a 60 FPS | Pausar en el parque | Observado 04:33 constante durante varios minutos; dibujos quietos. |
| Ocultar a 60 FPS | Ocultar estando pausado | Cambió a ocultos y en pausa; desapareció la escena y conservó 04:33. |
| Clic sobre el gato | Puntero sobre el cuerpo gris con parque pausado | El editor inferior registró un clic y mantuvo el foco del campo. |
| Escritura y cambio de ventana | Texto con acentos, activar Bloc de notas, Alt+Tab y volver a escribir | El editor conservó ambas frases. La vuelta exacta por Alt+Tab no quedó acreditada por el indicador de foco; se reutiliza la prueba anterior de VERIFICACION-PARQUE.md porque los estilos y filtros de entrada no cambiaron. |
| Comparación | Ocho pelajes, paso, repetir, velocidad, tamaño, pausa y tema | Playwright verificó respuestas de todos los controles a 390/1440 px, sin errores, desbordes ni infracciones axe. |

Walk of the frequent task: elegir preferencias, guardarlas, conservar el contador del ciclo, pausar y cerrar. Los errores se presentan junto al campo y al pie; el campo inválido recibe foco. Guardar y salir se trasladaron fuera del desplazamiento para que permanezcan disponibles.

Accessibility floor: colores nativos del sistema, texto y campos etiquetados, botones de al menos 44 DIP, navegación Tab y aceleradores. Estado por texto, no sólo color. La comparación respeta reducción de movimiento y ofrece pausa. El usuario puede pausar la app; no se añadió un ajuste automático ligado a la animación del sistema. Lector de pantalla y alto contraste del sistema no ensayados.

Look: separación entre acciones inmediatas y preferencias; estado y próxima visita arriba, guardado y salida al pie. La unión del pelaje se corrigió sobre los 64 cuadros, manteniendo el cuerpo estable.

Differences from the approved reference: la propuesta HTML era un esquema; la implementación conserva controles WPF reales. El pie fijo mejora acceso al guardado. El cambio de cantidad recrea gatos únicamente al confirmar un valor distinto en el escritorio.

Mechanical checks: publicación Release y contratos finales pasaron; las tres pruebas del parque web pasaron sin modificar sus expectativas; jscpd informó cero clones. El render de los controles reales a 520/340 DIP y 150 % conservó Guardar y Salir dentro de los límites; se inspeccionaron sus capturas. Es una simulación de escala de dibujo, no una prueba cambiando la configuración DPI del sistema. El binario entregado tiene SHA256 41C01323F1C9968F9B5E37091EF8FB732505B4E85F59CA268980CC0C31E809DE.

Visual inspection: passed — se inspeccionó la escena corregida en la app real, los 64 cuadros exportados, el panel y sus estados de validación/pausa/parque, los renders angostos y la comparación HTML a 390/1440 px. Evidencia de entrega: configuracion.png y gatitos-fluidez-parque.png; renders de escala en el paquete de evidencia. No se cambió el DPI del sistema ni se ensayaron otros monitores. La aceptación estética del propietario permanece pendiente.

## Revisión

Ruta propia: cambio local reversible sin acceso de red, captura global de entrada ni alteración de documentos ajenos. La persistencia de preferencias antiguas y el fallo de reemplazo se cubren con archivos de prueba aislados. Se revisaron reglas de cadencia, integración temporal, caché de dibujos, orden de patas, límites de entrada y guardado. Los validadores compartidos evitan que CLI, panel y archivo acepten rangos diferentes. Las pruebas y observaciones permiten entregar una versión para valoración visual; no acreditan 60 cuadros físicos sostenidos ni consumo universal. La corrección sobre la unión anatómica exige juicio visual; no se propone convertir ese juicio en una regla textual o un test de píxeles frágil.
