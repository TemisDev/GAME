# Marea Alta — Demo ampliada

## Recorrido jugable

1. **Entrada del arrecife:** recoge tres residuos, genera una unidad de energía y activa la primera turbina con `E`.
2. **Ruinas hundidas:** cruza la primera compuerta, recoge otros tres residuos y activa la segunda turbina.
3. **Fosa de salida:** cruza la segunda compuerta, recupera los tres últimos residuos, activa la tercera turbina y atraviesa la compuerta final hasta la baliza verde.

## Contenido añadido

- Música ambiental y efectos para recolección, generación de energía, error, turbina, compuerta, victoria y botones.
- Sprites propios para residuos, turbinas y compuertas.
- Tres compuertas con animación de apertura y un `BoxCollider2D` que se reduce junto con la parte visual.
- Tres turbinas interactivas con cambio visual al activarse.
- Nueve residuos distribuidos en grupos de tres, uno por cada fase.
- Cámara de seguimiento con límites para todo el escenario horizontal.
- Tres ambientes diferenciados mediante color, fondo, relieve y obstáculos.
- HUD con nombre de zona, recursos, progreso y objetivo actual.

## Prueba recomendada

- Confirmar que la música empieza en el menú y continúa al iniciar la partida.
- Comprobar que cada residuo se recoge una sola vez y reproduce sonido.
- Intentar activar una turbina sin energía para verificar el mensaje y el sonido de error.
- Activar cada turbina con `E` y comprobar el cambio visual y la apertura de su compuerta.
- Verificar que las paredes, arrecifes y compuertas cerradas bloquean al personaje.
- Recorrer las tres zonas y llegar a la baliza verde.
- Probar pausa con `Esc`, reinicio tras ganar con `R` y regreso al menú.

## Créditos de audio

Los archivos, autores, enlaces y licencias están documentados en:

`Assets/ThirdParty/AudioCC0/SOURCES_AND_LICENSES.md`
