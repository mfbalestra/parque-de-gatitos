# Gatitos en Windows: prueba nativa

Una aplicación C# con WPF muestra uno, cuatro u ocho gatos sobre el borde inferior del monitor principal. Caminan, se preparan para saltar, saltan con gravedad, aterrizan, se sientan, se acicalan y duermen. Usa las ilustraciones y los recortes del parque web. Esta prueba permite evaluar la compañía en el escritorio y su consumo; no traslada todavía todos los juegos del parque.

## Usarla

Abrí `Gatitos.Windows.exe`. Aparecen cuatro gatos y un icono de información en la bandeja de Windows, junto al reloj; puede estar entre los iconos ocultos. El clic derecho ofrece **1 gato**, **4 gatos**, **8 gatos**, **Pausar/Reanudar**, **Ocultar/Mostrar**, **Controles** y **Salir**. El doble clic en el icono oculta o muestra los gatos.

Los clics atraviesan incluso el cuerpo de cada gato y llegan al programa de abajo. La ventana de los gatos no toma el foco ni aparece en Alt+Tab. Podés seguir escribiendo y cambiando de aplicación. Los gatos sí pueden tapar visualmente un fragmento de contenido en el borde inferior: ocultalos cuando necesites verlo.

El panel **Controles** ofrece las mismas acciones con botones y navegación de teclado. Se abre deliberadamente y funciona como una ventana normal. Cerrar ese panel deja los gatos activos; **Salir de los gatitos** cierra toda la aplicación. La opción `--panel` abre directamente ese panel al iniciar, útil para probarlo o manejarlo con teclado.

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

El script intenta recoger los motores de GPU atribuidos al proceso. Los contadores ajenos que desaparecen se registran como advertencias; las muestras propias válidas se conservan. Si Windows no devuelve datos, se registra la causa sin inventar consumo cero. Para repetir solamente ocho gatos, agregá `-Counts 8`; usá otra carpeta de resultados.

Para comprobar la entrada sin tocar documentos propios:

```powershell
.\desktop\bin\Release\net10.0-windows\Gatitos.Windows.exe --probe --output "$PWD\entrada.json"
```

Ese editor de ejemplo registra únicamente los clics, teclas y texto que recibe su propia ventana. Abrí también los gatos. Maximizá el editor, pausá los gatos, hacé clic sobre un cuerpo y escribí; repetí en un espacio transparente. Cambiá con Alt+Tab a otra aplicación y volvé. El registro del editor, la observación y los estilos de ventana se complementan: ninguno demuestra por sí solo todos los casos.

## Qué está cubierto

El resultado local y sus límites quedan en [VERIFICACION.md](VERIFICACION.md). No se comprobaron otros monitores, escalas de pantalla, escritorios virtuales, juegos exclusivos, escritorio seguro ni sesiones remotas. No se implementó detección automática de pantalla completa; se puede ocultar manualmente. La prueba no registra el inicio automático ni instala servicios.

## Mantener el prototipo

`OverlayWindow.cs` contiene la ventana pasiva y los controles; `CatMotion.cs`, el movimiento determinista; `SpriteCatalog.cs`, la lectura de los recortes canónicos. No dividir las láminas en celdas iguales: sus recortes y máscaras son irregulares. Los estilos nativos de ventana son necesarios además de desactivar las pruebas de impacto de WPF. No agregar captura de entrada para implementar interacciones sin acordar primero cómo convivirían con las otras aplicaciones.

El modo de medición es diagnóstico local y no representa consumo universal. Comparar con Electron o Tauri requiere medir una aplicación equivalente, con el mismo arte, escena, tamaño y equipo.
