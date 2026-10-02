# Assets de enemigos — Marea Alta

Estas hojas de sprites son conceptos generados para una futura implementación. Todas contienen seis cuadros horizontales y transparencia real.

## Archivos recomendados para Unity

| Archivo | Tamaño | Cuadros | Tamaño de cada celda |
|---|---:|---:|---:|
| `PezDardo_Spritesheet_Unity.png` | 768 × 256 px | 6 | 128 × 256 px |
| `PezAbisal_Spritesheet_Unity.png` | 768 × 256 px | 6 | 128 × 256 px |
| `MinaCoralina_Spritesheet_Unity.png` | 768 × 256 px | 6 | 128 × 256 px |
| `LeviatanResiduos_Spritesheet_Unity.png` | 1536 × 512 px | 6 | 256 × 512 px |

Los archivos terminados en `_Source.png` conservan la resolución original generada.

## Configuración de importación

1. Copiar las versiones `_Unity.png` a `Assets/_MareaAlta/Art/Enemies`.
2. En el Inspector, seleccionar `Texture Type: Sprite (2D and UI)`.
3. Seleccionar `Sprite Mode: Multiple`.
4. Usar `Filter Mode: Point (no filter)`.
5. Desactivar compresión o usar `Compression: None`.
6. Abrir `Sprite Editor > Slice > Grid by Cell Size`.
7. Usar el tamaño de celda indicado en la tabla y confirmar `6` cuadros horizontales.
8. Configurar el pivote en `Center` inicialmente. Ajustarlo si el movimiento visual lo requiere.

## Secuencias propuestas

- **Pez Dardo:** nado 1, nado 2, anticipación, inicio de carga, impulso, recuperación.
- **Pez Abisal:** patrulla 1, patrulla 2, detección, mordida, persecución, recuperación.
- **Mina Coralina:** reposo 1, reposo 2, detección, advertencia, carga máxima, enfriamiento.
- **Leviatán de Residuos:** reposo, rugido, succión, lanzamiento, núcleo vulnerable, recuperación.

Antes de integrarlos definitivamente conviene probar su legibilidad junto a Mara y ajustar escala, colisionadores y duración de cada cuadro dentro de Unity.
