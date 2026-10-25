# Parque integrado en Windows — 25/10/2026

Alcance: escena grande sobre el área útil del monitor principal, visitas automáticas y retorno después de una estadía configurable. Elección del propietario: 1B y 2A. La escena conserva la identidad visual existente. Esta prueba no incorpora la caminata de ocho cuadros, el selector de 60 FPS ni la optimización general de memoria.

## Evidencia funcional

- Release compiló con cero errores y advertencias. La comprobación interna pasó 1 779 249 aserciones sobre movimientos, límites, ciclos automáticos y cancelaciones en 1/4/8 gatos y distintos anchos/pasos temporales.
- `dotnet run --project tests/native/ParkContracts.csproj -c Release --no-restore`: 48 contratos independientes pasaron. Cubren una visita durante un salto ya iniciado, conservación del objeto/pelaje, regreso exacto al suelo y visualización de intervalos en horas.
- La primera prueba falló con «Ruta inválida» y abrió el aviso que vio el propietario. La llegada ahora asigna exactamente las coordenadas del destino, evitando residuos de redondeo. Se conservó el criterio de profundidad no negativa; las pruebas posteriores pasaron. Los errores de `--self-test` se escriben en su resultado JSON y devuelven código 1, sin abrir un diálogo.
- Las tres pruebas existentes del parque web pasaron. Sus criterios, fixtures y arte no se modificaron.
- `jscpd desktop --pattern '**/*.cs' --ignore '**/obj/**,**/bin/**' --min-lines 10 --reporters console`: cero clones. No se añadieron dependencias a la aplicación.
- Se ejecutaron seis procesos nuevos: escritorio y parque, cada uno con 1/4/8 gatos. `park-measurement-summary.json` es la fuente de las cifras y del hash medido. Una repetición por caso; no caracteriza variabilidad. La muestra del parque empieza después de la entrada. Todas las fases ocultas tuvieron cero cuadros y pasos; todas registraron cero activaciones y cero muestras con el overlay en primer plano.
- GPU: los registros conservan las advertencias de contadores ajenos y sólo usan muestras válidas del PID. El resumen corresponde al motor más ocupado durante la fase visible; no incluye el compositor de Windows.

## UI verification: parque y controles nativos

Environment: Windows 11, .NET 10 WPF, monitor 1920 × 1080 al 100 %, área útil 1920 × 1032. Computer Use con `@oai/sky`, editor aislado y Calculadora. El informe HTML se revisó aparte a 390 y 1440 px con Playwright y axe.

Page measured: aplicación nativa Release, seguida por el ejecutable publicado en `outputs/parque-windows`. El editor sólo registra su propia entrada. Los programas habituales del propietario no se editaron.

Approved reference: parque web y ocho pelajes existentes; integración de escena grande y vuelta temporizada elegida en el chat. La inspección técnica no sustituye la valoración visual del propietario.

Plan: comprobar el ciclo real con intervalos de un minuto; rechazo de cero, guardado, pausa/ocultación y reloj, paso de clics/texto y cambio de programa; ejercitar la vuelta manual y verificar el paquete. Medir las dos escenas por separado sin automatización de interfaz concurrente.

| State | Widths |
|---|---|
| Escritorio, aparición, entrada, visita, salida, desaparición, pausa y oculto | 1920 px, monitor físico |
| Informe de uso y consumo | 390 y 1440 px |

Mechanical checks: compilación, comprobación nativa, contratos independientes, pruebas web, seis mediciones y validación del informe sin fallas de axe ni desbordes.

| Width × height | State | Capture | Finding |
|---|---|---|---|
| 1920 × 1032 | Parque con ocho gatos | parque-windows-visita.png | Fondo, mantitas, estructura, mariposas y ocho identidades visibles. |
| 1920 × 1032 | Paquete final en visita | parque-windows-final.png | Recorrido del césped y apoyo en las plataformas, sin pérdida de gatos. |
| 390 y 1440 | Informe | park-report-capturas | Lectura, tabla desplazable y captura del parque sin desbordes. |

Controls exercised:

| Control or gesture | Input | Evidence it responded |
|---|---|---|
| Guardar tiempos inválidos | 0 minutos de compañía | Mensaje de rango; el contador anterior siguió vigente. |
| Guardar tiempos válidos | 1 minuto de compañía y estadía | Confirmación visible; aparición automática, visita, vuelta y nuevo intervalo observados. |
| Configuración inicial del paquete | Nuevo directorio publicado | Campos 30 y 5 visibles; estadía de cinco minutos al entrar. |
| Pausar/Reanudar | Botón | Cambió a «en pausa» y el reloj permaneció en 00:11; reanudar restauró «en movimiento» y la cuenta regresiva. |
| Ocultar/Mostrar | Botón | El parque desapareció; el reloj quedó en 03:57 y se conservó hasta mostrar. |
| Clic a través del parque | Puntero sobre escena opaca | El editor registró el clic y mantuvo el foco de su campo. |
| Teclado bajo el parque | Texto con acentos | El editor conservó «El parque apareció y el editor sigue recibiendo texto.». |
| Otro programa | Tecla 2 en Calculadora con el parque visible | Al ocultarlo, Calculadora mostraba 2. Su árbol accesible no estuvo disponible; se verificó por imagen. |
| Cambio de programa | Calculadora → Alt+Tab → editor | Se recuperó el campo y se escribió «Volví con Alt+Tab y puedo seguir escribiendo.». |
| Ir al parque ahora | Botón desde el escritorio | Cambió a «El parque está apareciendo»; pedir volver durante la entrada inició la salida conservando las posiciones. |
| Volver al escritorio | Botón durante la visita del paquete final | Estado «Los gatos vuelven al escritorio», descenso desde la estructura y nuevo intervalo de compañía observados. |
| Salir | Botón en la instancia de desarrollo | Se cerró la instancia propia antes de recompilar. |

Walk of the frequent task: iniciar en compañía, configurar los minutos, esperar una visita, trabajar en el editor bajo el parque, cambiar de aplicación, pausar/ocultar y volver al escritorio. Las operaciones de entrada se hicieron sobre ventanas de prueba, sin hooks globales.

Accessibility floor: botones y campos nativos etiquetados, acceso por teclado y texto de validación. La escena decorativa no recibe el foco. El panel tiene desplazamiento para alturas menores. Alto contraste, lector de pantalla y animación reducida del sistema no fueron verificados.

Look: ilustración existente a pantalla grande, escena estática exportada desde las funciones originales, cinco mariposas animadas y gatos que mantienen su tamaño de escritorio. La barra de tareas queda disponible. El panel se puede activar por encima del parque.

Differences from the approved reference: recorrido nativo acotado; no incluye todos los juegos del navegador. Mantiene las dos poses actuales de caminar. Los tiempos de prueba se acortaron por controles reales; el paquete nuevo inicia con 30/5.

Transitions: aparición automática y desaparición observadas. El regreso desde plataformas incluye descenso; la escena se retira después de la llegada al suelo. Las cancelaciones en distintas fases y la entrada durante un salto tienen cobertura de contratos generados.

Visual inspection: passed — se inspeccionó la aplicación real y el paquete publicado. Los controles, tiempos, estado oculto y paso de entrada respondieron. El informe pasó a 390/1440 px y se inspeccionaron sus capturas. No se verificaron otros monitores, escalas, sesiones remotas, escritorios virtuales o juegos exclusivos; el menú de bandeja comparte acciones revisadas, pero no se operó directamente.

## Revisión

Revisión propia por cambio local reversible, sin red, captura global de entrada ni modificación de documentos ajenos. Se revisaron el diff completo, estado del ciclo, límites de tiempo, persistencia local, manejo de pausas, recursos de dibujo y cierre. Se conserva el registro histórico del primer prototipo en `VERIFICACION.md`; sus cifras no describen esta integración. No se alteraron otros cambios de trabajo.

El arte exportado proviene de `assets/parque.png` y las funciones `drawGround`/`drawGym` de `park-details.js`, con tiempo cero. El exportador de desarrollo y su resultado están versionados; el navegador no forma parte del paquete. El informe humano conserva método, cifras, límites y propuestas pendientes.
