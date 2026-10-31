# Parque de gatitos

También hay una [prueba nativa para Windows con C# y WPF](desktop/README.md): gatos sobre el escritorio, modos de uno, cuatro y ocho, controles de pausa y ocultación, y mediciones reproducibles.

Escena animada en pixel art: un parque con arcoíris, flores y mariposas donde muchos gatitos de distintos pelajes y contexturas pasean, se persiguen, juegan, ruedan, se lamen, duermen, se meten en una caja, trepan un gimnasio gatuno y se tiran por el tobogán.

Verla en línea: https://mfbalestra.github.io/parque-de-gatitos/

La página abre con el parque y ocho gatos. Tocá el pasto para tirar un ovillo: los gatitos más cercanos corren a buscarlo. Cada uno tiene un pelaje distinto: naranja, gris, calicó, negro, blanco, blanco y negro, siamés y atigrado marrón. Caminan con ocho imágenes completas por pelaje; la secuencia avanza según la distancia que recorren y mantiene el cuerpo y las patas juntos.

Los gatos se agachan antes de saltar, se impulsan hacia adelante, recogen las patas en el aire y amortiguan la caída. Al rascar los postes mueven las patas delanteras y mantienen las traseras apoyadas. El gimnasio tiene madera, soga y almohadones; hay tres mantitas en el pasto, mariposas con alas animadas y sombras suaves.

También podés usar el teclado: Tab enfoca el parque, las flechas mueven el punto de lanzamiento, Enter tira el ovillo y Espacio pausa o reanuda. Si el sistema tiene activada la preferencia de movimiento reducido, la escena comienza pausada.

## Compañía y controles

**Controles** abre la configuración. Podés elegir 30 o 60 FPS y los minutos de compañía (1–240) y de estadía en el parque (1–60). Los valores iniciales de la web son ocho gatos, 60 FPS, 30 minutos de compañía y cinco de parque. La frecuencia es un objetivo de dibujo: no altera la velocidad ni garantiza que una pantalla o navegador alcance esa frecuencia.

**Volver a la compañía** lleva los mismos gatos al borde inferior de la escena. Allí podés elegir uno, cuatro u ocho gatos y guardar. Si mantenés la cantidad, conservan sus posiciones. **Ir al parque ahora** adelanta la visita; también se inicia al terminar el intervalo de compañía. El parque aparece gradualmente y los gatos caminan hasta el césped; la estadía comienza después de que todos llegan. Al finalizar vuelven a la franja y el parque desaparece. Las órdenes repetidas conservan la transición en curso; pedir una visita durante la salida prepara otra al terminar la vuelta.

La compañía funciona **dentro de esta pestaña**. Una página normal no coloca gatos sobre otros programas ni implementa la bandeja o el paso de clics de Windows. Mientras están en el parque se conservan sus juegos; en la franja pasean y descansan.

**Pausar** y **Ocultar** detienen animación y reloj. Cambiar de pestaña también los detiene; al volver no se acumulan visitas por el tiempo ausente. Mostrar conserva una pausa previa. Los botones exteriores permiten reanudar o mostrar sin entrar en la configuración.

**Guardar preferencias** conserva los valores juntos en el almacenamiento local de este navegador y de esta dirección. No envía datos ni modifica la configuración de la app de Windows. El ciclo mantiene el tiempo ya transcurrido: si el nuevo límite ya venció, la transición empieza al continuar. Los valores inválidos se señalan junto a los campos; si el navegador no permite guardar, se informa el error y siguen aplicadas las preferencias anteriores. Una configuración guardada ilegible vuelve a los valores iniciales con un aviso. Cerrar y volver a abrir la página inicia una nueva estadía en el parque con las preferencias guardadas.

## Abrir una copia local

Descargá la carpeta completa. Conservá junto a `index.html` los archivos `art-data.js`, `walk-data.js`, `scene-art.js`, `park-details.js`, `park-cycle.js`, `web-controls.js`, `web-controls.css` y la carpeta `assets`. Son recursos relativos y no necesitan servicios externos. Para desarrollo, serví esa carpeta con un servidor HTTP local. La apertura directa desde disco depende de las reglas de almacenamiento del navegador; la comprobación de entrega usa HTTP y GitHub Pages.

## Cómo está hecho

La simulación de `index.html` conserva un mundo de 256 × 144 unidades; el canvas dibuja a 1536 × 864 para mostrar el detalle de las ilustraciones. Se mantienen las actividades, los puntos del gimnasio y el límite de siete ovillos. Los saltos usan un arco balístico continuo con preparación, vuelo, aterrizaje y recuperación; durante esas fases el gato termina el salto antes de retomar su actividad. La simulación avanza en pasos de 1/60 s y el dibujo se limita por separado a la frecuencia elegida. La suspensión no ejecuta pasos de recuperación por el tiempo ausente.

`scene-art.js` carga las imágenes y dibuja cada postura con los pies apoyados en su posición. `art-data.js` define los recortes: las láminas no forman una grilla exacta, por lo que dividirlas en celdas iguales cortaría algunos gatos. Las poses del gimnasio se apoyan en su plataforma y los gatos de la caja se dibujan antes de su pared frontal.

La caminata usa `walk-data.js` y ocho atlas `assets/walk-*.png`, derivados de las imágenes completas revisadas en Windows. El recorte transparente conserva un punto de referencia común y la misma escala durante las ocho fases de cada pelaje. `build-web-walk.cjs` genera los recursos de navegador desde `desktop/assets`, usando Playwright sólo como herramienta de desarrollo; recibe la carpeta que contiene su instalación como primer argumento. `assets/walk-provenance.json` verifica la correspondencia por SHA256. La web no descarga ni necesita .NET. Los cuadros avanzan por distancia al caminar, sin sumar saltos, descansos o el descenso por el tobogán.

`park-cycle.js` contiene las reglas de duración, fases y preferencias; `web-controls.js` conecta esas reglas con el diálogo. Las decisiones de juego permanecen en la simulación. El origen y las limitaciones del arte se documentan en [desktop/WALK-ART.md](desktop/WALK-ART.md); las cifras de rendimiento de Windows no se atribuyen al navegador.

El fondo y las láminas de gatos están en `assets`. Se usan imágenes locales sin dependencias ni servicios externos. Si una imagen falta o no carga, esa parte conserva el dibujo procedural original como respaldo. `park-details.js` dibuja las mantitas, el gimnasio, las mariposas y las sombras sin intervenir en las decisiones de la simulación. El origen del arte y los prompts están en `assets/ORIGEN.txt` y `assets/PROMPTS-VARIEDADES.txt`.

## Comprobar un cambio

Con Node.js disponible:

```sh
node tests/simulation.cjs
node tests/motion.cjs
node tests/art.cjs
node tests/walk-web.cjs
node tests/web-controls.cjs
```

La primera prueba verifica las actividades, las coordenadas del puntero, el límite de ovillos y el teclado. La segunda comprueba los cinco disparadores de salto, la continuidad, la gravedad, las fases, el aterrizaje y el apoyo estable al rascar, a distintas frecuencias de actualización y durante recorridos prolongados. La tercera comprueba los ocho pelajes, los recortes, la transparencia declarada por los PNG, el apoyo, la inversión de dirección y el respaldo ante recursos inválidos.

Las nuevas pruebas de caminata comprueban los 64 cuadros, la escala uniforme, el anclaje, la fase periódica por distancia y la procedencia de los recursos. Las de controles verifican preferencias, transiciones, suspensión y separación entre simulación y frecuencia de dibujo. La comprobación visual y de teclado se realiza además sobre la página completa, con el diálogo abierto y cerrado, las transiciones y el movimiento reducido.

La referencia original de `5cf72af` permanece intacta. `node tests/simulation.cjs --historical` conserva la comparación exacta anterior y falla de manera esperada porque el propietario pidió cambiar los saltos; su digest no se regeneró para aceptar el nuevo movimiento.

Al cambiar ilustraciones o recortes, mirá además las poses en movimiento, ambos sentidos, la entrada a la caja y el tobogán. Al cambiar las reglas de juego, no reemplaces la referencia de comportamiento sin acordar primero el nuevo resultado esperado. Este README cubre la ejecución y el dibujo del parque; mantenelo junto con esos cambios.
