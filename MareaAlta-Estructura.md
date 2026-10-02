# Marea Alta — Estructura del prototipo

## Estado actual

- Proyecto creado con Unity `6000.3.22f1` y la plantilla Universal 2D.
- URP, Input System y paquetes 2D instalados por la plantilla.
- Escena inicial renombrada como `Prototype_PuntaCoral` e incluida en la configuración de build.
- Estructura de carpetas creada dentro de `Assets/_MareaAlta`.
- Primer controlador de nado añadido para teclado y gamepad.
- Underwater Diving Pack importado con su licencia y una herramienta para configurar sus sprites.
- Fase 2 completada: sprites configurados, animaciones Idle/Swim, prefab del jugador, movimiento y límites creados.
- Fase 3 preparada: residuo reutilizable, recolección única, seis residuos de prueba y contador de fragmentos.
- Fase 4 preparada: conversión automática de tres fragmentos en una energía, conservación del sobrante y HUD con ambos valores.
- Fase 5 preparada: interacción con E, gasto único de energía, aviso de energía insuficiente y apertura de compuerta.
- Fase 6 preparada: recorrido provisional, arrecifes con colisión, ruta de residuos, zona de entrada, turbina, compuerta y vista previa de salida.
- Fase 7 preparada: salida final, estado de victoria, resumen de residuos y tiempo, bloqueo del movimiento y reinicio con R.
- Fase 8 preparada: instrucciones, pulido de legibilidad, configuración 1280×720, verificación final y build de Windows.
- Pendiente: ejecutar una prueba completa de la build fuera del editor y conservar el ejecutable junto al proyecto editable.

## Base técnica acordada

Base de diseño derivada del documento de Marea Alta y de la investigación previa. El proyecto todavía no se ha creado.

- **Editor:** Unity 6.3 LTS, instalando el parche `6000.3.x` más reciente disponible en Unity Hub.
- **Plantilla:** Universal 2D (URP).
- **Presentación:** 2.5D construido con sprites 2D, capas de profundidad, iluminación 2D y parallax.
- **Física:** `Rigidbody2D` y `Collider2D`; movimiento libre en los ejes X/Y.
- **Entrada:** Input System, comenzando con teclado y dejando acciones reutilizables para controles táctiles.
- **Arte inicial:** sprites y animaciones existentes del buzo; figuras provisionales para todo lo demás.
- **Plataforma de la primera demo:** PC. La adaptación a móvil se realiza después de validar el ciclo principal.

Este enfoque conserva la sensación 2.5D sin mezclar física 2D y 3D. Si más adelante se decide usar escenarios low-poly reales, se deberá migrar conscientemente a un proyecto 3D URP o convertir únicamente el fondo, sin cambiar las reglas de juego a mitad del prototipo.

## Alcance de la primera demo

La primera demo será una sola escena jugable de 5–10 minutos. Su ciclo completo será:

`nadar → recoger basura → convertir 3 fragmentos en 1 energía → activar una turbina → abrir una compuerta → llegar a la salida`

La energía es un **recurso reciclado para las turbinas**. No es oxígeno ni se consume con el tiempo. La conversación previa propuso ambos comportamientos, pero combinarlos haría confusa la mecánica central y duplicaría los medidores. Si después se necesita presión temporal, la marea cumplirá esa función.

La marea, checkpoints por sección, corrientes, fauna, FMOD, residuos especiales, menú principal y versión móvil quedan para la ampliación posterior al primer ciclo jugable. Se incorporarán en ese orden solo cuando el núcleo sea estable.

## Carpetas dentro de Assets

```text
_MareaAlta/
  Scenes/
  Scripts/
    Core/
    Player/
    Interaction/
    Resources/
    World/
    UI/
  Prefabs/
    Player/
    Interactables/
    Environment/
    UI/
  Data/
  Art/
    Sprites/
      Player/
      Debris/
      Environment/
      UI/
    Materials/
    Animations/
    VFX/
  Audio/
  Settings/
```

Mantener paquetes y assets de terceros fuera de esta carpeta. Separar el objeto de física del hijo visual para reemplazar figuras provisionales por modelos sin rehacer el comportamiento.

## Escenas

- `Prototype_PuntaCoral`: primera y única escena necesaria para el MVP; contiene el ciclo jugable completo y un HUD mínimo.
- `MainMenu`: añadir cuando el prototipo funcione.
- `PuntaCoral`: nivel definitivo, construido a partir del prototipo validado.

La pantalla de resultados puede ser un panel del nivel. No hace falta una escena de carga ni un gestor persistente para la primera prueba.

## Jerarquía propuesta del prototipo

```text
Prototype_PuntaCoral
  Systems
    GameSession
    CheckpointManager
  Main Camera
  GlobalLight2D
  Level
    Section_01
      Geometry
      Debris
      Turbine
      Gate
      RestorationVisuals
    Section_02
    Section_03
    Exit
  Player_Mara
    Visual
    InteractionOrigin
  UI
    HUD
    PausePanel
    ResultsPanel
  EventSystem
```

## Responsabilidades de los scripts

| Script | Responsabilidad |
| --- | --- |
| `GameSession` | Estado de partida: jugando, pausado, restaurando sección o victoria. |
| `PlayerInputReader` | Recibir movimiento, dash e interacción; ofrecer la misma interfaz a teclado y controles táctiles. |
| `SwimController` | Aceleración, desaceleración, velocidad máxima, colisiones y dash con cooldown. |
| `PlayerInteractor` | Elegir un único objetivo cercano y mostrar o ejecutar su interacción contextual. |
| `IInteractable` | Contrato compartido por residuos y turbinas. |
| `DebrisCollectible` | Recolección única, cantidad de fragmentos y, opcionalmente, progreso de desatasco al mantener pulsado. |
| `EnergyInventory` | Contar fragmentos, convertir tres fragmentos en una unidad y validar gastos. |
| `TurbineController` | Comprobar energía, descontarla una sola vez y activar la compuerta asociada. |
| `GateController` | Cambiar el estado físico y visual de una compuerta. |
| `SectionController` | Registrar residuos iniciales, calcular limpieza y coordinar el estado de una sección. |
| `TideController` | Avanzar la amenaza de la sección activa según la basura pendiente. |
| `RestorationController` | Traducir el porcentaje de limpieza en cambios de color y elementos visuales. |
| `HUDController` | Mostrar fragmentos, energía, limpieza, dash e interacción disponible. |
| `ExitTrigger` | Finalizar la partida al atravesar la última salida desbloqueada. |

Usar eventos para actualizar el HUD y la restauración al cambiar recursos o limpieza. Reservar las actualizaciones continuas para movimiento, marea y efectos que realmente las necesitan.

## Reglas que deben quedar claras antes del arte

### Recursos

Propuesta inicial: conversión automática. Por cada tres fragmentos, sumar una unidad de energía y conservar el resto. Recoger siete fragmentos equivale a dos unidades de energía y un fragmento sobrante. Si se desea crafteo manual, será una decisión explícita de diseño.

Una turbina activa no puede cobrar otra vez. Una turbina sin energía suficiente debe indicar cuánto falta.

### Marea

El documento combina nado submarino con una amenaza por inundación. Hace falta comunicar por qué subir el agua resulta peligroso para Mara. Propuesta para validar: el frente de marea representa corrientes y sedimentos peligrosos que vuelven una sección intransitable.

Para la primera prueba, usar una altura visible que avanza en la sección activa. Su velocidad será:

`velocidadBase + incrementoPorContaminacion × proporcionDeBasuraPendiente`

Limpiar reduce la velocidad sin detener completamente la amenaza. Los valores se ajustan durante pruebas; no son cifras definitivas. La derrota se dispara al alcanzar la condición peligrosa de la sección, con aviso visual previo.

### Reinicio por sección

Al entrar en una sección, guardar posición de entrada, inventario, residuos, turbinas, compuertas y estado de marea. Al fallar, restaurar ese punto completo. Conservar las secciones anteriores ya superadas.

Restaurar solo la posición permitiría duplicar recursos o dejar al jugador sin la energía necesaria. Para el prototipo, usar secciones secuenciales y bloquear el regreso después de confirmar un checkpoint; si se requiere exploración hacia atrás, ampliar el modelo de guardado.

### Limpieza y victoria

Limpieza de sección = residuos recogidos / residuos iniciales. Una sección sin residuos se considera limpia. El resultado global debe calcularse con los residuos totales, no promediando porcentajes de secciones de distinto tamaño.

La victoria ocurre al cruzar la salida final. Mostrar porcentaje de limpieza y tiempo. Los umbrales de grados de éxito quedan pendientes de balance.

## Nivel de prueba propuesto

| Sección | Qué enseña | Elementos provisionales |
| --- | --- | --- |
| 1: Entrada | Nadar, recoger y activar una turbina | Pasillo amplio, seis residuos normales, turbina de coste una unidad. |
| 2: Arrecife | Dash, desatasco y presión de marea | Obstáculos, seis residuos, uno atascado, turbina de coste dos unidades. |
| 3: Salida | Combinar acciones y elegir cuánto limpiar | Recorrido más estrecho, seis residuos, turbina final de coste dos unidades. |

Para esta propuesta, cada residuo normal entrega un fragmento. Garantizar que los recursos necesarios sean accesibles antes de cada compuerta; los residuos adicionales permiten probar limpieza opcional.

Usar el sprite disponible para Mara y formas 2D provisionales para obstáculos, compuertas, residuos y turbinas. La restauración inicial puede ser un cambio de color en el entorno.

## Programa de entrega: prototipo en 15 horas

| Bloque | Resultado comprobable | Tiempo máximo |
| --- | --- | --- |
| 1. Proyecto | Crear Unity 6.3 LTS con Universal 2D, carpetas, escena, cámara y capas. | 1 h |
| 2. Buzo | Importar sprites, crear Animator y nadar en ocho direcciones con límites. | 3 h |
| 3. Recolección | Crear un residuo reutilizable, recogerlo una vez y actualizar el contador. | 2 h |
| 4. Energía | Convertir 3 fragmentos en 1 energía y mostrar ambos valores. | 1.5 h |
| 5. Turbina | Gastar energía, activar una turbina y abrir una compuerta. | 1.5 h |
| 6. Escenario | Construir un recorrido pequeño con formas y sprites provisionales. | 2 h |
| 7. Cierre | Condición de victoria, mensaje final y reinicio sencillo. | 1 h |
| 8. Verificación | Corregir errores, ordenar el proyecto y generar una build para Windows. | 2 h |
| 9. Reserva | Resolver retrasos de animación, configuración o compilación. | 1 h |

Total máximo planificado: **15 horas**. Al terminar se entregan el proyecto editable, una build ejecutable y una demostración del ciclo principal.

### Funciones incluidas en la entrega

- Una escena jugable corta.
- Movimiento del buzo en ocho direcciones.
- Animaciones disponibles de reposo y nado; recolección solo si el material importado permite configurarla dentro del tiempo.
- Colisiones y límites del nivel.
- Recolección de basura.
- Conversión de tres fragmentos en una unidad de energía.
- Una turbina y una compuerta.
- HUD sencillo para fragmentos y energía.
- Condición de victoria y reinicio.
- Proyecto ordenado y ejecutable para Windows.

### Fuera de esta entrega

- Marea dinámica y checkpoints.
- Dash, residuos atascados y corrientes marinas.
- Fauna reactiva y restauración visual compleja.
- Menú principal completo, guardado y selección de niveles.
- Controles móviles y publicación en tiendas.
- FMOD, audio dinámico, arte definitivo y efectos avanzados.

## Orden de implementación y comprobación

1. **Movimiento:** nado con inercia, colisiones, cámara y dash. Verificar que las diagonales no sean más rápidas y que el dash respete obstáculos y cooldown.
2. **Interacción y recursos:** recoger, desatascar y convertir fragmentos. Verificar que un residuo no se cobre dos veces y que se conserve el resto de fragmentos.
3. **Progresión:** gastar energía y abrir compuertas. Probar gasto insuficiente y activación repetida.
4. **Restauración y final:** cambios visuales, HUD y resultado al salir.
5. **Prueba completa:** recorrer la demo, corregir bloqueos y generar una build para Windows.
6. **Segunda iteración:** añadir marea y checkpoints; después adaptar controles táctiles.
7. **Expansión:** incorporar fauna, corrientes, efectos, arte definitivo y audio dinámico con FMOD.

El primer objetivo verificable es completar una sección: nadar → recoger tres residuos → obtener una energía → activar turbina → atravesar compuerta. Después se añade la presión de la marea y el reinicio.
