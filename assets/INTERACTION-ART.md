# Interacciones de los ocho pelajes

El propietario aprobó el piloto naranja el 31 de octubre de 2026 y pidió extenderlo a los otros siete pelajes. La lámina naranja y las ocho caminatas se conservan sin cambios. Se generaron siete láminas nuevas con la herramienta integrada `image_gen.imagegen`, tomando las poses naranjas como objetivo de edición y la caminata de cada pelaje como referencia de identidad.

Cada lámina contiene 16 gatos completos: cuatro cuadros de amasado, cuatro de preparación para dormir, cuatro de roce de cabeza y cuatro de juego con las patas. El último cuadro de preparación también representa el sueño. No se separan patas, cabeza ni cola para animarlas. La transparencia se conserva del PNG generado; los colores RGB de las zonas totalmente transparentes no forman parte del fondo visible.

Los archivos finales son `acciones-gris.png`, `acciones-calico.png`, `acciones-negro.png`, `acciones-blanco.png`, `acciones-tuxedo.png`, `acciones-siames.png` y `acciones-marron.png`, junto al original `acciones-naranja.png`. Los prompts completos y sus referencias están en [interaction-art-prompts.json](interaction-art-prompts.json). Los hashes y la proporción de transparencia quedan en [interaction-art-checks.json](interaction-art-checks.json); las dimensiones se conservan en `interaction-art-data.js`.

`build-interaction-art.cjs` lee el alfa de cada lámina y encuentra los espacios transparentes entre animales. Produce `interaction-art-data.js`, compartido por web y Windows, con recortes de cuerpo completo, una escala por pelaje y anclajes al suelo. El naranja reutiliza exactamente sus metadatos aprobados. Regeneración: `node build-interaction-art.cjs <carpeta-node_modules-con-playwright>`.

La revisión técnica incluye los 128 recortes, cuatro composiciones renderizadas con el cargador real de WPF y los encuentros mixtos en el navegador. La muestra local permite elegir el pelaje de cada demostración. Las imágenes ampliadas quedan disponibles para la evaluación visual del propietario; la aceptación del piloto naranja no se presenta como aceptación automática de cada variante.
