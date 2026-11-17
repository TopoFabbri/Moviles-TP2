# Trabajo Practico Final

### Desarrollo de Juegos para Dispositivos Móviles

**Profesor:** Rodrigo Corral

---

## Consigna

Desarrollo de un juego específicamente para la plataforma de móviles que cumpla a la perfeccion con lo requerido en el **Segundo Parcial - Trabajo Practico Nro 2: Desarrollo para Móviles**

Para la entrega se deberá de incorporar al menos **2 de las 4 siguientes características** al juego, quedando esto a elección del estudiante:

- **Ads:** Implementación de 2 tipos distintos de Ads:
  - **Interstitial Ads**, son aquellos ads cortos que aparecen cuando el usuario pasa de nivel o pierde alguna vida. Estos no son opcionales, se aparecen sin que el usuario haga nada.
  - **Reward Ads**, aquellos ads que se le ofrece al usuario y si lo acepta, se le concede un reward de algún tipo como monedas, experiencia, etc. Suelen estar en los stores y tener cooldown.
- **Analytics:** Implementar un sistema de analytics y presentar un informe (no mas de 3 o 5 carillas, incluyendo graficos) del juego donde se muestre lo siguiente:
  - Desde qué dispositivos se juega.
  - Estabilidad del juego (% de crashes).
  - Hasta donde juegan los usuarios (cuantos niveles ganan, o cuanto tardan y donde en perder).
  - En que gastan más dinero dentro del juego.
- **Cloud Save:** Implementar un sistema de guardado del progreso del juego en la Nube (Google play). Que deberá guardar todo progreso de forma periódica. En caso de volver a instalar la aplicación del juego, y si este detecta que hay un save en la nube, tiene que aparecer un popup al usuario donde se le consulte si quiere seguir jugando con el save local o bajarse el de la nube; este popup tambien debera mostrar los stats de ambos saves para ser comparados (nivel, monedas, etc). Esta decisión debe ser recordada y no se le debe preguntar de vuelta al usuario durante esta instalación.
- **Social Media:** Se deberá implementar el Plugin de Facebook y las siguientes funcionalidades:
  - **Login**, para acceder con un usuario de facebook, este debe quedar logueado incluso si se cierra el juego (se tiene que mostrar el usuario logueado en algún lado, como en el menú de settings por ej.).
  - **Sharing**, para compartir el score del juego, ya sea del nivel, o de la partida, lo que sea en realidad. Esta opción puede salir después de completar el nivel.
  - **Logout**, el usuario tiene que tener la posibilidad de cerrar sesión.

  > **PD:** agregar a Facebook el mail `rm_cf@hotmail.com` para testing.

### Optimizaciones del juego:

- Transición de escenas de forma asincrónica.
- Uso de **Sprite Atlases** para las texturas.
- **LOD**, uso de **Frustum** y **Occlusion Culling**, dependiendo cual sea el caso de sus juegos.

### Aclaraciones:

- Prestar especial atención a la iconografía del juego: tanto dentro del juego, como icono, splash screen, etc; como en la página del store: capturas, videos, achievements, etc.
- Toda la UI debe ser adaptativa y no entrar en conflicto con las áreas seguras.
- Cuidado con los Términos y Condiciones de sus juegos una vez que tengan los nuevos requisitos implementados porque es probable que tengan que actualizarlos.
- Prestar especial atención a los tiempos de verificación de los juegos en Google Play y Facebook, suelen tardar un tiempo hasta que se aprueban o no.

---

## Entrega

- **Fecha de entrega 1ra:** 12/12/2025
- **Fecha de entrega 2da:** 15/12/2025

### Archivos de entrega:

- Mismos requisitos del TP2
- Url a la pagina del juego en el store de Google Play.
- Link a un repositorio con el proyecto.