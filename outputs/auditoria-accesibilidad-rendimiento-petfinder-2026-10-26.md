# Auditoría de accesibilidad y rendimiento del frontend PetFinder

**Fecha:** 26 de octubre de 2026<br>
**Aplicación:** PetFinder, ASP.NET Core 8 MVC<br>
**Evaluación global:** riesgo moderado; se recomienda atender primero la conversión de hora de citas y el peso/carga inicial del hero 3D.

## Resumen ejecutivo

Se revisaron las vistas Razor de Home, Adopción, Reportes, autenticación, refugios y paneles; los estilos propios; el JavaScript de GSAP/ScrollTrigger, el sistema de imágenes y las escenas Three.js; además del pipeline estático y los recursos locales. La inspección se complementó con una observación del árbol accesible de Home y pruebas HTTP locales de rutas, imágenes y assets.

El hallazgo de mayor riesgo funcional está en las citas: la interfaz pide la hora local del adoptante, pero el servidor la convierte a UTC usando su propia zona horaria. En accesibilidad, se verificaron un contraste insuficiente en la numeración de pasos y riesgos de asociación de errores y anuncios de resultados dinámicos. En rendimiento, los dos modelos del hero suman **5,549,260 bytes (5.29 MiB / 5.55 MB)** y se cargan al iniciar Home; además, las siete imágenes del catálogo observadas se marcan simultáneamente `loading="eager"` y `fetchpriority="high"`.

No se modificaron archivos fuente ni el comportamiento de la página durante la auditoría. Se generó este informe. El repositorio estaba limpio y `main` sincronizada con `origin/main` antes de crear el informe.

## Alcance y método

La revisión estática se repartió en siete frentes: shell y Home; catálogo de adopción; reportes/resultados; paneles y refugios; CSS visual y responsive; animaciones; y assets/pipeline de entrega. Se contrastaron hallazgos con rutas y selectores concretos.

En el árbol accesible observado para Home se recorrieron **668 nodos** en 10 páginas de observación: no se detectaron nombres vacíos entre los controles interactivos ni entre las imágenes; se observaron 24 encabezados, 41 enlaces, 24 botones y 10 regiones. La consola del navegador no reportó errores en esa observación. Sí apareció una ilustración Three.js como grupo con capacidad de clic, pese a que el canvas se marca decorativo.

La instancia `localhost:5206` no estaba escuchando al iniciar las comprobaciones; se levantó el binario ya compilado disponible, sin reconstruir ni cambiar código. Después respondieron HTTP 200 Home, Adopción y la ruta de resultados de prueba. `/Reportes/Crear` redirigió a inicio de sesión, como corresponde a la protección de la ruta; no se inició sesión.

No se ejecutaron Lighthouse, axe, validador HTML, lectores de pantalla ni mediciones de Core Web Vitals. No se probó cada vista en todos los tamaños de pantalla. Por ello, los resultados no constituyen una certificación global WCAG ni una medición de rendimiento de producción.

## Hallazgos priorizados

### 1. Alta — La hora local de una cita puede convertirse en un instante incorrecto

La etiqueta del campo dice “Fecha y hora (tu hora local)” y el control es `datetime-local`, que no transporta zona ni desplazamiento horario. En `Agendar`, un `DateTime` que no es UTC se convierte con `ToUniversalTime()`, operación que usa la zona del proceso servidor. Si adoptante y servidor tienen zonas distintas, la cita puede quedar desplazada.

**Evidencia:** `src/PetFinder.Web/Views/Adopcion/Detalle.cshtml:10`; `src/PetFinder.Web/ViewModels/DetalleMascotaViewModel.cs:12-14`; `src/PetFinder.Web/Controllers/AdopcionController.cs:52-60`.<br>
**Recomendación:** enviar explícitamente la zona/desplazamiento del navegador o un instante UTC validado, convertir con esa información y confirmar al usuario la hora local interpretada. Probar con cliente y servidor en zonas distintas, incluidos cambios de horario.

### 2. Alta — El hero inicia la descarga y preparación de más de 5.5 MB de GLB

`landing-scene.js` crea perro y gato y llama `loadActors()` durante la inicialización de Home. Los dos modelos responden localmente con 2,428,236 y 3,121,024 bytes. `prefers-reduced-motion` detiene el bucle continuo, pero no evita crear el renderer ni cargar/preparar los modelos; la portada ya dispone de ilustraciones SVG alternativas.

**Evidencia:** `src/PetFinder.Web/wwwroot/js/three/landing-scene.js:5-9,24-29`; `src/PetFinder.Web/wwwroot/js/three/SceneManager.js:43-72,254-265`; `Views/Home/Index.cshtml:17-19`; `wwwroot/models/dog-chibi.glb` y `cat-chibi-rigged.glb`.<br>
**Recomendación:** omitir la escena si `prefers-reduced-motion` está activo; en los demás casos, cargarla cuando el hero se acerque al viewport o tras una acción explícita, reducir peso de geometría/texturas y conservar el fallback 2D.

### 3. Media — Contraste bajo en el número de los pasos

`.step-number` usa `#c8aa91` sobre el fondo blanco de `.step-card`. El cálculo WCAG de esos colores da **2.18:1**, inferior a **4.5:1** para texto normal. El número puede comunicar el orden de la secuencia.

**Evidencia:** `src/PetFinder.Web/wwwroot/css/site.css`, selectores `.step-number` y `.step-card` (`color:#c8aa91`, `background:#fff`, `font-size:1.02rem`).<br>
**Recomendación:** oscurecer el color hasta superar 4.5:1 sobre el fondo efectivo y comprobar los fondos alternativos/estados visuales.

### 4. Media — Los errores de validación no quedan asociados semánticamente a cada campo

En el formulario de reporte, cada control tiene un `span asp-validation-for`, pero el marcado revisado no vincula explícitamente el mensaje mediante `aria-describedby` ni refleja `aria-invalid`. Los mensajes están visibles en el DOM, pero la asociación programática no está garantizada; los errores de campo tampoco aparecen en el resumen `ModelOnly`.

**Evidencia:** `src/PetFinder.Web/Views/Reportes/Crear.cshtml:8-18`; errores por clave de campo en `src/PetFinder.Web/Controllers/ReportesController.cs:30-38,46-62`.<br>
**Recomendación:** asociar cada error al control con `aria-describedby`, actualizar `aria-invalid` y revisar el foco/resumen cuando el servidor devuelve errores. Validar el caso de error de carga de foto con lector de pantalla.

### 5. Media — Los filtros del mapa cambian el recuento sin anunciar el nuevo estado

El directorio actualiza `#refuge-result-count` y muestra/oculta el estado vacío al filtrar, pero ese recuento no está marcado como región viva. El `role="status"` existente anuncia geolocalización/carga del mapa, no los resultados ordinarios de búsqueda y especie.

**Evidencia:** `src/PetFinder.Web/Views/Home/_MapaRefugios.cshtml:17-18,31`; actualización en `wwwroot/js/scroll/refugios-map.js:242-262,315-316`.<br>
**Recomendación:** anunciar el nuevo recuento o el estado vacío en una región `role="status"`/`aria-live="polite"`, evitando emitir un mensaje por cada pulsación; verificar con lector de pantalla.

### 6. Media — Las siete fotos iniciales del catálogo tienen prioridad alta

En el HTML renderizado de `/Adopcion`, las **7 de 7** etiquetas de imagen devueltas usaban `loading="eager"` y `fetchpriority="high"`. Las fuentes son fotos locales y respondieron HTTP 200; las etiquetas observadas tenían texto alternativo significativo. El problema no es la disponibilidad actual de las fotos, sino competir por prioridad con el recurso LCP y las demás descargas. Las siete instancias suman aproximadamente **466,889 bytes**.

**Evidencia:** HTML real de `/Adopcion`; lógica en `src/PetFinder.Web/Views/Adopcion/Index.cshtml:74-83` (`isDemoPhoto` asigna `eager`/`high`).<br>
**Recomendación:** reservar `fetchpriority="high"` y `eager` para la foto que realmente sea LCP/visible al inicio; usar `loading="lazy"` y prioridad automática para las demás tarjetas. Conservar los fallbacks locales.

### 7. Media — Estado ARIA de las FAQ queda fijo aunque se cierre la respuesta

Las cuatro preguntas usan `<details open>` y `<summary aria-expanded="true">`. Al cerrar una pregunta con el comportamiento nativo, el atributo `aria-expanded` no se actualiza porque no se encontró sincronización en el código revisado.

**Evidencia:** `src/PetFinder.Web/Views/Home/_FaqSection.cshtml:5-8`.<br>
**Recomendación:** confiar en la semántica nativa de `details/summary` y retirar los atributos redundantes, o mantener `aria-expanded` sincronizado en cada evento `toggle`. Decidir también si las respuestas deben iniciar todas abiertas.

### 8. Media — Dependencias de animación globales para vistas sencillas

El layout inserta GSAP y ScrollTrigger en todas las vistas. También declara un importmap remoto de Three.js; el importmap por sí solo no implica que cada página descargue los módulos, pero sí expone dependencias externas globalmente. Google Fonts también se carga desde el layout. No se midieron bytes o waterfall externos.

**Evidencia:** `src/PetFinder.Web/Views/Shared/_Layout.cshtml:8-19`; escena de Home en `Views/Home/Index.cshtml:39-42`.<br>
**Recomendación:** cargar GSAP/ScrollTrigger solo en las páginas que los usen y mantener Three.js bajo demanda; revisar `main.js` y módulos compartidos antes de retirar scripts. Considerar fallback/autohospedaje para recursos críticos de terceros.

### 9. Media — Sin política explícita observada de compresión o caché para estáticos

En las respuestas `HEAD` de `site.css`, `motion.css`, `main.js` y los GLB no aparecieron encabezados `Content-Encoding` ni `Cache-Control`. `Program.cs` configura `UseStaticFiles`, pero el pipeline inspeccionado no agrega middleware de compresión ni política explícita de caché. Un proxy, IIS o CDN de producción podría cubrirlo; eso no se comprobó.

**Evidencia:** respuestas locales de `localhost:5206`; `src/PetFinder.Web/Program.cs:32-53`; referencias/versionado en `_Layout.cshtml:11-19,70-73`.<br>
**Recomendación:** verificar encabezados del entorno de despliegue antes de cambiar la app; configurar compresión Brotli/Gzip para texto y caché/versionado por clase de asset si la infraestructura frontal no lo proporciona.

### 10. Baja — Las tablas administrativas no explican que no hay filas

Los `foreach` de usuarios y refugios generan un `tbody` vacío si no hay registros; no hay condición de lista vacía ni mensaje equivalente. Otras vistas sí ofrecen estados vacíos.

**Evidencia:** `src/PetFinder.Web/Views/PanelAdministrador/Index.cshtml:8-9`.<br>
**Recomendación:** agregar una rama condicional con texto claro en una fila `colspan` o un estado vacío relacionado con cada tabla.

### 11. Baja — Las coincidencias no ofrecen una acción visible para continuar

Las tarjetas de posibles coincidencias se renderizan como artículos estáticos y no contienen enlace a un detalle/contacto. La interfaz revisada permite crear otro reporte, pero no continuar desde el candidato. El propio sitio aclara que el porcentaje de similitud es simulado.

**Evidencia:** `src/PetFinder.Web/Views/Reportes/Resultados.cshtml:14-30`.<br>
**Recomendación:** si existe un destino autorizado, añadir un enlace accesible por tarjeta; de lo contrario, indicar una vía verificable para continuar sin presentar la similitud simulada como identificación.

### 12. Baja — La escena decorativa responde a clic de puntero sin equivalente de teclado

El árbol accesible expone la ilustración como grupo con capacidad de clic, mientras el renderer se marca `aria-hidden`/`presentation` y no se identificó una acción equivalente de teclado. El impacto es bajo si se trata únicamente de decoración; si se espera interacción, falta alternativa de teclado.

**Evidencia:** `src/PetFinder.Web/wwwroot/js/three/SceneManager.js:63-66,141-174`; `Views/Home/Index.cshtml:19`; árbol accesible observado de Home.<br>
**Recomendación:** si el clic aporta una función, ofrecer un botón enfocable con nombre accesible y activación con Enter/Espacio; si es solo decoración, retirar la respuesta al clic.

## Resultados positivos

- El layout declara español, incluye enlace “Saltar al contenido”, landmark `main` y navegación etiquetada.
- Home agrupa las secciones con encabezados y regiones nombradas; en el árbol observado no se encontraron botones/enlaces/campos o imágenes sin nombre.
- Hay breakpoints responsive y reglas `prefers-reduced-motion`; los módulos de revelado/contadores y la historia de scroll consultan esa preferencia.
- El formulario de cita es POST y está protegido por token antifalsificación y rol. Los filtros de adopción tienen etiquetas; existen validaciones para rangos de edad y estados vacíos en algunas vistas.
- El catálogo servido emplea fotos demo locales con `alt`; las siete fuentes comprobadas respondieron 200. Las hojas `site.css` y `motion.css`, `main.js` y los dos GLB también respondieron 200.
- El mapa mantiene lista alternativa y mensajes de carga/error; los porcentajes de coincidencia están identificados como simulados.

## Recomendaciones de ejecución

1. **Primero:** corregir la interpretación de zona horaria de las citas y reforzar la asociación de errores del formulario.
2. **Accesibilidad:** corregir contraste, anunciar cambios de filtros, arreglar el estado de FAQ y verificar teclado/foco con lector de pantalla.
3. **Rendimiento:** bajar la prioridad de seis imágenes del catálogo, diferir la carga 3D y evitar iniciar WebGL con movimiento reducido; después medir el LCP y la transferencia real.
4. **Entrega:** condicionar librerías por página y confirmar compresión/caché en el despliegue, incluyendo proxy/CDN.
5. **Regresión:** ejecutar axe/Lighthouse, validar 320/375/768/1024 px y zoom, recorrer flujos con teclado, y probar errores de servidor, estados vacíos, carga de imágenes y fallback WebGL.

## Límites y estado del repositorio

La revisión combina código estático y smoke tests locales, no una auditoría formal de conformidad. No se probaron sesiones autenticadas, lector de pantalla, teclado en todas las rutas, zoom/reflow completo ni Core Web Vitals; `/Reportes/Crear` redirige a login y no se inició sesión. Las métricas de modelos e imágenes son tamaños/encabezados del host local, no datos de red móvil ni de producción.

No se hicieron cambios de código, HTML, CSS o JavaScript. La aplicación fue iniciada desde el binario precompilado ya existente para las comprobaciones; no se recompiló. `main` estaba limpia y sincronizada con `origin/main` (`0/0`) antes de generar este informe; el último commit observado fue `fc43230 fix: correct responsive homepage overflow and hero spacing`.
