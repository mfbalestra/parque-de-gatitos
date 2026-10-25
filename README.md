# Parque de gatitos

Escena animada en pixel art: un parque con arcoíris, flores y mariposas donde muchos gatitos de distintos pelajes y contexturas pasean, se persiguen, juegan, ruedan, se lamen, duermen, se meten en una caja, trepan un gimnasio gatuno y se tiran por el tobogán.

Verla en línea: https://mfbalestra.github.io/parque-de-gatitos/

La página muestra solo la escena, a pantalla completa. Tocá el pasto para tirar un ovillo: los gatitos más cercanos corren a buscarlo. Cada uno de los ocho gatos tiene un pelaje distinto: naranja, gris, calicó, negro, blanco, blanco y negro, siamés y atigrado marrón.

Los gatos se agachan antes de saltar, se impulsan hacia adelante, recogen las patas en el aire y amortiguan la caída. Al rascar los postes mueven las patas delanteras y mantienen las traseras apoyadas. El gimnasio tiene madera, soga y almohadones; hay tres mantitas en el pasto, mariposas con alas animadas y sombras suaves.

También podés usar el teclado: Tab enfoca el parque, las flechas mueven el punto de lanzamiento, Enter tira el ovillo y Espacio pausa o reanuda. Si el sistema tiene activada la preferencia de movimiento reducido, la escena comienza pausada.

## Abrir una copia local

Descargá la carpeta completa y abrí `index.html`. Conservá a su lado `art-data.js`, `scene-art.js`, `park-details.js` y la carpeta `assets`. Los recursos son relativos y no necesitan servicios externos. La verificación se hizo con un servidor HTTP local; la apertura directa desde disco quedó sin comprobar porque el navegador de la herramienta bloquea direcciones `file:`. La versión publicada del enlace de arriba cambia recién cuando se publica el trabajo en GitHub Pages.

## Cómo está hecho

La simulación de `index.html` conserva un mundo de 256 × 144 unidades; el canvas dibuja a 1536 × 864 para mostrar el detalle de las ilustraciones. Se mantienen las actividades, los puntos del gimnasio y el límite de siete ovillos. Los saltos usan un arco balístico continuo con preparación, vuelo, aterrizaje y recuperación; durante esas fases el gato termina el salto antes de retomar su actividad.

`scene-art.js` carga las imágenes y dibuja cada postura con los pies apoyados en su posición. `art-data.js` define los recortes: las láminas no forman una grilla exacta, por lo que dividirlas en celdas iguales cortaría algunos gatos. Las poses del gimnasio se apoyan en su plataforma y los gatos de la caja se dibujan antes de su pared frontal.

El fondo y las láminas de gatos están en `assets`. Se usan imágenes locales sin dependencias ni servicios externos. Si una imagen falta o no carga, esa parte conserva el dibujo procedural original como respaldo. `park-details.js` dibuja las mantitas, el gimnasio, las mariposas y las sombras sin intervenir en las decisiones de la simulación. El origen del arte y los prompts están en `assets/ORIGEN.txt` y `assets/PROMPTS-VARIEDADES.txt`.

## Comprobar un cambio

Con Node.js disponible:

```sh
node tests/simulation.cjs
node tests/motion.cjs
node tests/art.cjs
```

La primera prueba verifica las actividades, las coordenadas del puntero, el límite de ovillos y el teclado. La segunda comprueba los cinco disparadores de salto, la continuidad, la gravedad, las fases, el aterrizaje y el apoyo estable al rascar, a distintas frecuencias de actualización y durante recorridos prolongados. La tercera comprueba los ocho pelajes, los recortes, la transparencia declarada por los PNG, el apoyo, la inversión de dirección y el respaldo ante recursos inválidos.

La referencia original de `5cf72af` permanece intacta. `node tests/simulation.cjs --historical` conserva la comparación exacta anterior y falla de manera esperada porque el propietario pidió cambiar los saltos; su digest no se regeneró para aceptar el nuevo movimiento.

Al cambiar ilustraciones o recortes, mirá además las poses en movimiento, ambos sentidos, la entrada a la caja y el tobogán. Al cambiar las reglas de juego, no reemplaces la referencia de comportamiento sin acordar primero el nuevo resultado esperado. Este README cubre la ejecución y el dibujo del parque; mantenelo junto con esos cambios.
