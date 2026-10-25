# Parque de gatitos

Escena animada en pixel art: un parque con arcoíris, flores y mariposas donde muchos gatitos de distintos pelajes y contexturas pasean, se persiguen, juegan, ruedan, se lamen, duermen, se meten en una caja, trepan un gimnasio gatuno y se tiran por el tobogán.

Verla en línea: https://mfbalestra.github.io/parque-de-gatitos/

La página muestra solo la escena, a pantalla completa. Tocá el pasto para tirar un ovillo: los gatitos más cercanos corren a buscarlo. Hay ocho gatos con tres pelajes ilustrados: naranja, gris y calicó.

También podés usar el teclado: Tab enfoca el parque, las flechas mueven el punto de lanzamiento, Enter tira el ovillo y Espacio pausa o reanuda. Si el sistema tiene activada la preferencia de movimiento reducido, la escena comienza pausada.

## Abrir una copia local

Descargá la carpeta completa y abrí `index.html`. Conservá a su lado `art-data.js`, `scene-art.js` y la carpeta `assets`. No necesita instalación ni servidor. La versión publicada del enlace de arriba cambia recién cuando se publica el trabajo en GitHub Pages.

## Cómo está hecho

La simulación de `index.html` conserva un mundo de 256 × 144 unidades; el canvas dibuja a 1536 × 864 para mostrar el detalle de las ilustraciones. Las velocidades, las decisiones de los gatos, el recorrido del gimnasio y el límite de siete ovillos conservan las reglas originales.

`scene-art.js` carga las imágenes y dibuja cada postura con los pies apoyados en su posición. `art-data.js` define los recortes: las láminas no forman una grilla exacta, por lo que dividirlas en celdas iguales cortaría algunos gatos. Las poses del gimnasio se apoyan en su plataforma y los gatos de la caja se dibujan antes de su pared frontal.

El fondo y las dos láminas de gatos están en `assets`. Se usan imágenes locales sin dependencias ni servicios externos. Si una imagen falta o no carga, esa parte conserva el dibujo procedural original como respaldo. El origen del arte y el prompt de las poses complementarias están en `assets/ORIGEN.txt`.

## Comprobar un cambio

Con Node.js disponible:

```sh
node tests/simulation.cjs
node tests/art.cjs
```

La primera prueba compara dos recorridos completos con la versión original del commit `5cf72af`: marcha autónoma y lanzamiento de ovillos. También verifica las coordenadas del puntero, el límite de ovillos y el teclado. La segunda comprueba recortes, transparencia declarada por los PNG, apoyo de patas, inversión de dirección y respaldo ante recursos inválidos.

Al cambiar ilustraciones o recortes, mirá además las poses en movimiento, ambos sentidos, la entrada a la caja y el tobogán. Al cambiar las reglas de juego, no reemplaces la referencia de comportamiento sin acordar primero el nuevo resultado esperado. Este README cubre la ejecución y el dibujo del parque; mantenelo junto con esos cambios.
