# Verificación del prototipo WPF — 25/10/2026

La fuente de las cifras es `measurement-summary.json`, generada desde seis registros de proceso y una repetición del recolector de GPU. Allí quedan las duraciones reales, ambas repeticiones, memoria residente y privada, CPU, cuadros y la identidad SHA-256 del ensamblado. Los registros detallados se entregan como JSON junto al ejecutable. No hay una comparación medida con Electron o Tauri.

## Entorno y comprobaciones

Windows 11 Pro 10.0.26300, Ryzen 7 5700X, 16 procesadores lógicos, Radeon RX 9060 XT, 1920 × 1080 al 100 %. SDK 10.0.403 y runtime 10.0.14. Se midió el proceso compilado en Release. Los programas habituales permanecieron abiertos; no hubo automatización de interfaz durante las muestras. Las mediciones son breves y locales, no una prueba de estabilidad prolongada ni una garantía para otro equipo.

- `dotnet build desktop/Gatitos.Windows.csproj -c Release --configfile desktop/NuGet.Config`: compilación sin errores ni advertencias. La primera ejecución aislada falló por acceso a la configuración de .NET/NuGet; la compilación posterior con acceso normal terminó correctamente.
- `Gatitos.Windows.exe --self-test --output <archivo>`: 1 123 511 comprobaciones pasaron, incluyendo límites para 1/4/8 gatos, cuatro anchos y tres pasos temporales, arco de salto y carga de los ocho pelajes. No se agregó una dependencia de pruebas; se usaron casos generados y propiedades de los movimientos en el propio ejecutable de diagnóstico.
- Las tres pruebas existentes del parque web (`simulation.cjs`, `motion.cjs`, `art.cjs`) pasaron. Sus fuentes y los PNG no se modificaron.
- El temporizador inicial entregaba unos 21 cuadros/s. Se sustituyó por la señal de dibujo de WPF y se repitieron las seis mediciones. Sólo esa segunda serie figura en el resumen final.
- Las seis fases ocultas registraron cero cuadros dibujados y cero pasos de simulación. Todas las ejecuciones registraron cero activaciones de la superposición y cero muestras con ella en primer plano.
- Los estilos leídos en ejecución fueron `0x80800A8`: ventana por capas, paso de clics, no activación, herramienta y siempre visible. El estilo de aplicación para la barra de tareas está ausente.
- La primera captura de GPU dejó de recoger datos ante un contador inválido de otro proceso. Se corrigió el script para conservar sólo muestras válidas del PID observado y se repitió con ocho gatos. No se interpretan muestras ausentes como cero.

### UI verification: superposición nativa y controles

Environment: Windows 11, WPF .NET 10, Computer Use con `@oai/sky`; route: aplicación real Release y editor nativo separado con texto de ejemplo. No es una página web y no corresponden Playwright ni axe para sus ventanas. Coverage: pantalla primaria 1920 × 1080 al 100 %, modos 1/4/8, pausa, movimiento, oculto y salida.

Page measured: `Gatitos.Windows.exe`, la aplicación nativa real con sus imágenes y controles. El editor separado sólo recibe la entrada que atraviesa la superposición; no sustituye la aplicación medida. La Calculadora de Windows se usó como segundo programa.

Approved reference: reutilización de los ocho gatos ya aceptados en el parque; prueba C# WPF autorizada en el chat. La aceptación visual de esta nueva presentación en escritorio corresponde al propietario; la inspección técnica no la sustituye.

Plan, written before the first check: observar uno, cuatro y ocho gatos; hacer clic sobre cuerpo y espacio transparente; escribir, cambiar de aplicación, ejercitar cantidad/pausa/ocultación/salida y medir sin automatización concurrente. El plan quedó registrado en la spec de esta unidad antes de la primera ejecución.

| State | Widths |
|---|---|
| 1, 4 y 8 gatos; pausa y movimiento | 1920, monitor físico |
| Oculto y salida | 1920, monitor físico |

Mechanical checks: pruebas internas generadas, tres pruebas web existentes y mediciones con contadores del proceso. El recolector comprueba que la aplicación finalice y que exista un resultado por cantidad. Inherited outside the changed region: no fallas nuevas en las pruebas del parque.

| Width × height | State | Capture | Finding |
|---|---|---|---|
| 1920 × 1032 de área útil | Un gato | wpf-un-gato.png | Gato completo, apoyo en el borde inferior. |
| 1920 × 1032 de área útil | Cuatro gatos | wpf-cuatro-gatos.png | Cuatro pelajes completos y separados. |
| 1920 × 1032 de área útil | Ocho gatos en movimiento | wpf-ocho-gatos.png | Caminar, agacharse, sentarse y dormir observados; recortes conservados. |

Controls exercised:

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Clic sobre cuerpo de gato | Puntero, 1/4/8 gatos | El contador del editor pasó de 0 a 1 y de 1 a 2 en las pruebas respectivas; recibió el clic inferior. |
| Clic en zona transparente | Puntero con ocho gatos | El editor registró un clic a través de la franja sin gato. |
| Escritura | Texto de ejemplo con acentos | Se observó el texto completo en el editor y en su registro local. |
| Cambio de programa | Alt+Tab, ida y vuelta | Editor → Calculadora → editor; se observó la ventana esperada al frente y se continuó escribiendo con ocho gatos en movimiento. |
| Teclado en otra aplicación | Tecla 2 | La Calculadora mostró 2 mientras los gatos seguían activos. |
| Cantidad 1/4/8 | Botones del panel | Cambió el estado y se observó la cantidad correspondiente en el escritorio. |
| Navegación del panel | Tab y Espacio | El foco pasó a Reanudar; Espacio cambió a movimiento y se observaron poses nuevas. |
| Pausar | Botón del panel | Cambió a en pausa y quedaron quietos los gatos. |
| Ocultar/Mostrar | Botón y Espacio | Desaparecieron todos los gatos y volvieron al estado en movimiento. |
| Salir | Botón del panel | Desapareció el panel y el proceso PID 24120 terminó. |

Walk of the frequent task: iniciar la aplicación, elegir cantidad, dejar el editor activo, hacer clic a través de gatos, escribir, cambiar a Calculadora y volver, ocultar/mostrar y salir. La herramienta restauraba ventanas maximizadas al activarlas; se volvió a maximizar y observar antes de repetir cualquier clic. Dos intentos de entrada en Bloc de notas detectaron actividad del usuario: no se continuó allí ni se escribió en documentos existentes.

Accessibility floor: controles nativos con etiquetas, foco visible y manejo Tab/Espacio verificados. Pausa manual disponible. No se verificaron alto contraste, lector de pantalla ni configuración de animación reducida del sistema. La superposición decorativa no participa del orden de foco.

Look: se conservaron el arte aprobado, las máscaras y el apoyo de las patas. La franja inferior es transparente, con sombras suaves. Los gatos pueden cubrir visualmente texto del programa inferior; Ocultar permite retirarlos.

Differences from the approved reference: el escritorio muestra sólo los gatos; el parque y sus objetos no forman parte de esta prueba. Los movimientos nativos usan una secuencia determinista para comparar consumos.

Transitions: se observaron poses de caminar, preparación, descanso y acicalado en ejecución; el arco balístico y el regreso al suelo se comprobaron también con las propiedades generadas. Mostrar después de ocultar conservó la aplicación inferior disponible.

Visual inspection: passed — inspección directa de los tres modos sobre el editor real y los controles nativos; las capturas se incluyen en la entrega. El menú de bandeja no fue operado directamente por Computer Use; se revisaron sus conexiones a las mismas acciones ejercitadas desde el panel. Otros monitores, escalas, sesiones remotas y pantalla completa quedan fuera de lo comprobado.

## Revisión

Revisión propia por tratarse de un prototipo local reversible, sin modificación de documentos, red ni hooks globales. Se inspeccionaron el conjunto del cambio, el paso de entrada, los estados oculto/pausa, el cierre de recursos, la lectura de arte local y las opciones de diagnóstico. La aplicación no expone servicios ni ejecuta el JavaScript del parque: extrae y analiza su objeto JSON. Los resultados y límites anteriores corresponden al código entregado.

La revisión también corrigió la protección del script de medición para conservar tanto resultados de CPU como de GPU que ya existan; una prueba con un JSON de GPU previo confirmó el rechazo antes de lanzar otra instancia. No se modificaron criterios ni fixtures del parque. La lectura del código y búsquedas de identificadores sustituyeron a jscpd, que no se ejecutó.

El paquete publicado en la carpeta local pasó las mismas comprobaciones internas y su ensamblado coincide con el SHA-256 medido. El informe HTML pasó la revisión a 390 y 1440 píxeles, incluyendo axe y ausencia de desbordes; se corrigieron las cifras tabulares y el largo de línea detectados en la primera revisión. Se inspeccionaron las capturas y se ejercitaron el tema y los desplegables. El resultado visual nativo queda disponible para la evaluación del propietario.
