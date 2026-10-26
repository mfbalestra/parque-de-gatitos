# Caminata basada en el naranja aprobado

El propietario aprobó el ciclo naranja V2 el 25/10/2026 y autorizó extenderlo a los otros siete pelajes. Las ocho poses naranjas conservan sus imágenes completas; los otros pelajes se generaron con la herramienta integrada de imágenes, tomando cada pose como referencia. Se corrigieron cuadros individuales cuando las láminas cambiaron la posición de una pata, las manchas o el contorno.

Cada atlas contiene cuatro columnas y dos filas en orden 1–8, con 3072 × 1024 píxeles. La preparación recorta celdas completas y reduce proporcionalmente las imágenes individuales. Las variantes se registran contra el cuadro naranja correspondiente: altura opaca, centro horizontal y suelo, con una sola transformación uniforme de la imagen entera. No se deforman partes, no se dibujan patas independientes ni se mantienen torsos fijos. El registro corrige diferencias de encuadre que introdujo la generación.

WPF decodifica cada atlas a 896 píxeles de ancho y conserva el alfa. Recorta el margen transparente, usa una escala uniforme para todo el ciclo del pelaje y mantiene el anclaje horizontal del lienzo original (800/1536). El borde inferior se apoya en la misma línea. La fase avanza por distancia; una zancada corresponde a 60 unidades multiplicadas por el tamaño del gato. Los FPS no cambian esa distancia.

## Secuencia de apoyo

La pata trasera cercana apoya delante en 1, pasa bajo la cadera y termina atrás en 5; se levanta en 6, se recoge en 7 y vuelve hacia delante en 8. La delantera cercana recupera en 8–2, toma contacto en 3 y recorre el apoyo en 4–7. La trasera lejana recupera en 2–4 y apoya en 5–1; la delantera lejana recupera en 4–6 y apoya en 7–3. La inspección sigue las patas por su profundidad y solapamiento, no por ordenar archivos solamente.

Hay variaciones pequeñas de dibujo entre cuadros. El ciclo aceptado es una ilustración discreta, no una simulación física exacta de contacto. La aprobación del naranja no se declara como aceptación visual automática de las otras siete variantes.

## Procedencia

[walk-cycle-provenance.json](walk-cycle-provenance.json) registra el origen y SHA256 de cada cuadro, su transformación de encuadre y los hashes de los ocho atlas. [walk-cycle-prompts.json](walk-cycle-prompts.json) conserva las instrucciones usadas con imagegen. Los recursos de runtime viven en assets; ninguna ruta de generación externa es necesaria para ejecutar la app. La verificación actual está en [VERIFICACION-CICLO.md](VERIFICACION-CICLO.md).

| Atlas | SHA256 |
|---|---|
| walk-blanco.png | c66629b37f89db47ee3608e73923e3b9c09fba7bc55c83f9075f12e047589b6a |
| walk-calico.png | 0b57b77b406a2ee30d31f9ca88511d30ac5f35e93506395f432abf605baa1efe |
| walk-gris.png | 0fcbc23d9722a91f027bef2c929651c070465a58df6470413c27542a7a788fac |
| walk-marron.png | 3dff3b2feb23f6bb0bebf7cc1c61ada16220416f2286a6cc460fc5302ddfeb01 |
| walk-naranja.png | 0223db427c2520b78b03258332291bac73e9b25c974561732d4d09f1702ac7e8 |
| walk-negro.png | 7df0cdc919c0a328053e164e968fdc3c75bf81d8f5237b289ac9427450e5c435 |
| walk-siames.png | 256655317c21314f124a86d7ac0456161b67dd099a68c28269e3c29801a5374d |
| walk-tuxedo.png | e4890993931ea90125fe41b6abc94628ac987fee5ed5591a697498cbc3f7b293 |
