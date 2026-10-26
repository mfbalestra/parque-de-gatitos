# Gatitos en Windows: prueba nativa

Una aplicación C# con WPF muestra uno, cuatro u ocho gatos sobre el borde inferior del monitor principal. Caminan, se preparan para saltar, saltan con gravedad, aterrizan, se sientan, se acicalan y duermen. Periódicamente aparece el parque sobre el área de trabajo: los mismos gatos entran, recorren el césped y la estructura, descansan y regresan al escritorio. Usa las ilustraciones y los recortes del parque web. Esta integración nativa no traslada todavía todos los juegos ni las interacciones del navegador.

## Visitas al parque

La configuración inicial es **cuatro gatos, 30 FPS, 30 minutos de compañía y 5 minutos de estadía**. En **Controles**, elegí la cantidad, la fluidez, «Compañía» (1 a 240 minutos) y «Estadía» (1 a 60), y presioná **Guardar preferencias**. Se aceptan minutos enteros. Los cuatro valores se guardan juntos en `gatitos-settings.json`, junto al ejecutable. La carpeta debe permitir escritura; ante un error, el panel conserva todas las preferencias anteriores. Los archivos antiguos que sólo contienen tiempos conservan esos tiempos y reciben cuatro gatos y 30 FPS.

Los tiempos nuevos se aplican al ciclo actual y conservan lo ya transcurrido: si el intervalo nuevo ya venció, la transición comienza al reanudar. Guardar una cantidad sin cambios conserva los gatos y sus posiciones. Cambiar la cantidad recrea el grupo y sólo se permite en el escritorio. Un archivo externo inválido impide el inicio con un mensaje; puede corregirse o retirarse para volver a los valores iniciales.

El parque aparece gradualmente durante dos segundos y cubre el área útil del monitor principal, dejando la barra de tareas disponible. Los gatos que estaban saltando terminan su salto; después caminan hacia el césped. La estadía empieza cuando todos llegaron. Algunos recorren los escalones y descansan en la estructura; los demás pasean, duermen y se acicalan. Las mariposas se mueven de forma independiente.

Cuando termina la estadía, los gatos bajan de la estructura, vuelven al borde del escritorio y el parque desaparece gradualmente. Entonces comienza un intervalo nuevo. **Ir al parque ahora** y **Volver al escritorio** adelantan el recorrido desde el panel o la bandeja. Pedir volver durante la entrada conserva las posiciones; pedir otra visita durante la salida la deja preparada para después de completar la vuelta. La cantidad de gatos se cambia durante la compañía en el escritorio.

El parque tapa visualmente los programas, pero los clics siguen llegando a lo que hay debajo. No bloquea el teclado, no toma el foco y no se incluye en Alt+Tab. Usá **Ocultar** para ver el programa inferior. El panel de controles puede quedar por encima del parque y se abre como una ventana normal. Pausar u ocultar detiene tanto las animaciones como el reloj; al volver continúa desde allí. Los intervalos de suspensión del equipo no disparan visitas acumuladas.

Para ver la escena directamente al iniciar: `Gatitos.Windows.exe --cats 8 --panel --park-now`. El paquete «Parque ahora» usa esta opción; el inicio normal respeta el intervalo configurado.

## Usarla

Abrí `Gatitos.Windows.exe`. Aparecen los gatos según tus preferencias guardadas y un icono de información en la bandeja de Windows, junto al reloj; puede estar entre los iconos ocultos. El clic derecho ofrece **Controles**, **Pausar/Reanudar**, **Ocultar/Mostrar**, **Ir al parque ahora**, **Volver al escritorio** y **Salir**. El doble clic en el icono oculta o muestra los gatos.

Los clics atraviesan incluso el cuerpo de cada gato y llegan al programa de abajo. La ventana de los gatos no toma el foco ni aparece en Alt+Tab. Podés seguir escribiendo y cambiando de aplicación. Los gatos sí pueden tapar visualmente un fragmento de contenido en el borde inferior: ocultalos cuando necesites verlo.

El panel **Controles** muestra la actividad, el tiempo restante, acciones inmediatas y preferencias agrupadas. Los cambios pendientes se indican por texto. Guardar y salir permanecen al pie mientras el contenido puede desplazarse; Tab recorre controles y Alt+G guarda. Se abre deliberadamente y funciona como una ventana normal. Cerrar ese panel deja los gatos activos; **Salir de los gatitos** cierra toda la aplicación. La opción `--panel` abre directamente ese panel al iniciar.

Elegir **60 FPS** aumenta la frecuencia de dibujo según la pantalla y el equipo. La velocidad de caminata, los saltos y el reloj conservan su duración. Las patas recorren ocho cuadros por zancada: la fase depende de la distancia física, tanto en el escritorio como en el parque. Pausar detiene también esa fase. `--fps 60` y `--cats 8` sustituyen las preferencias para esa ejecución; sólo se guardan si presionás Guardar preferencias.

Pausar mantiene los dibujos quietos; ocultar quita la franja. Ambos desconectan el ciclo de animación del dibujo de WPF. Mostrar conserva la pausa si estaba activada. No hay sonido, inicio automático, conexión a Internet ni captura global de teclado o ratón. Para evitar superposiciones accidentales se permite una sola instancia de los gatos.

## Compilar y ejecutar

Necesitás Windows x64 y el SDK de .NET 10, que incluye el runtime de escritorio. Desde la raíz del repositorio:

```powershell
dotnet build desktop/Gatitos.Windows.csproj -c Release --configfile desktop/NuGet.Config
.\desktop\bin\Release\net10.0-windows\Gatitos.Windows.exe --cats 8 --panel
```

El archivo de NuGet deja vacías las fuentes porque no se usan paquetes externos: las bibliotecas provienen del SDK instalado. La distribución es dependiente del **.NET Desktop Runtime 10 x64**; no incluye un navegador ni lleva un runtime privado. Conservá el ejecutable, sus archivos acompañantes, `art-data.js` y `assets` en la misma carpeta.

## Repetir las comprobaciones

Primero cerrá la instancia normal desde sus controles. Ejecutá:

```powershell
.\desktop\bin\Release\net10.0-windows\Gatitos.Windows.exe --self-test --output "$PWD\checks.json"
.\desktop\Measure.ps1 -OutputDirectory "$PWD\mediciones" -Repetitions 2
```

La comprobación interna recorre límites, saltos, direcciones, cantidades admitidas y carga de los ocho pelajes. Las mediciones abren procesos nuevos para cada cantidad; no cambies los gatos ni uses sus controles mientras se mide. Cada proceso calienta durante cinco segundos, mide veinte segundos visible, espera dos segundos oculto, mide diez segundos oculto y sale. Se guardan muestras de un segundo, duración real, CPU del proceso, memoria residente y privada, cuadros dibujados, pasos de simulación y activaciones de la superposición. Los resultados se escriben en archivos nuevos; el script se detiene si ya existen.

La CPU se expresa como porcentaje de un núcleo lógico y del total de núcleos disponibles. La memoria se expresa en MiB (1 048 576 bytes). La memoria privada es la memoria comprometida por el proceso; la residente incluye páginas compartidas y no se suma a la privada. Ocultar conserva las imágenes en memoria para poder volver a mostrarlas. El pequeño temporizador de medición sigue activo durante la muestra oculta; el ciclo de animación se detiene.

El script intenta recoger los motores de GPU atribuidos al proceso. Los contadores ajenos que desaparecen se registran como advertencias; las muestras propias válidas se conservan. Si Windows no devuelve datos, se registra la causa sin inventar consumo cero. Para repetir solamente ocho gatos a 60 FPS, agregá `-Counts 8 -FrameRates 60`; usá otra carpeta de resultados. Por defecto recorre ambas frecuencias. Agregá `-Park` para medir el parque. Un límite de 65 segundos por caso cierra únicamente el proceso de prueba creado por el script si no termina.

Para comprobar la entrada sin tocar documentos propios:

```powershell
.\desktop\bin\Release\net10.0-windows\Gatitos.Windows.exe --probe --output "$PWD\entrada.json"
```

Ese editor de ejemplo registra únicamente los clics, teclas y texto que recibe su propia ventana. Abrí también los gatos. Maximizá el editor, pausá los gatos, hacé clic sobre un cuerpo y escribí; repetí en un espacio transparente. Cambiá con Alt+Tab a otra aplicación y volvé. El registro del editor, la observación y los estilos de ventana se complementan: ninguno demuestra por sí solo todos los casos.

## Qué está cubierto

La integración y sus límites quedan en [VERIFICACION-PARQUE.md](VERIFICACION-PARQUE.md); [VERIFICACION.md](VERIFICACION.md) conserva la prueba anterior. No se comprobaron otros monitores, escalas de pantalla, escritorios virtuales, juegos exclusivos, escritorio seguro ni sesiones remotas. No se implementó detección automática de pantalla completa; se puede ocultar manualmente. La prueba no registra el inicio automático ni instala servicios.

## Mantener el prototipo

`OverlayWindow.cs` contiene la ventana pasiva y los controles; `CatMotion.cs`, el movimiento determinista; `SpriteCatalog.cs`, la lectura de los recortes canónicos. Las poses antiguas usan recortes y máscaras irregulares; las nuevas láminas de caminata tienen una cuadrícula explícita de 4 × 2 descrita en WALK-ART.md. Los estilos nativos de ventana son necesarios además de desactivar las pruebas de impacto de WPF. No agregar captura de entrada para implementar interacciones sin acordar primero cómo convivirían con las otras aplicaciones.

El modo de medición es diagnóstico local y no representa consumo universal. Comparar con Electron o Tauri requiere medir una aplicación equivalente, con el mismo arte, escena, tamaño y equipo.

La medición con `--park-now --output archivo.json` espera a que todos hayan llegado antes de recoger la fase visible. El resultado identifica `sceneMode` y el estado de cada muestra. Los resultados del prototipo anterior, en `measurement-summary.json`, son históricos; la integración tiene su propia serie en `park-measurement-summary.json`.

El ciclo y sus reglas viven en `ParkCycle.cs`; `ParkScene.cs` dibuja el fondo y las mariposas. El fondo incluye las mantitas y la estructura originales: `node desktop/build-scenery.cjs <node_modules global>` exporta `assets/parque.png` y `park-details.js` a una sola imagen de 1536 × 864. Playwright sólo se usa para esa exportación de desarrollo. El programa entregado usa WPF y no incluye un navegador. Al salir del parque se suelta la referencia a su imagen; la devolución efectiva de memoria depende del runtime.

La caminata usa ocho imágenes completas por pelaje, descritas en [WALK-ART.md](WALK-ART.md). El cuerpo, la cabeza y las patas cambian juntos según cada dibujo; las imágenes mantienen su proporción y una línea común de suelo. Las otras poses conservan los recortes anteriores. La optimización general de memoria sigue fuera de este cambio. La corrección se verifica en [VERIFICACION-IMAGENES.md](VERIFICACION-IMAGENES.md); [VERIFICACION-CAMINATA.md](VERIFICACION-CAMINATA.md) conserva la evidencia histórica de los controles y la primera caminata.
