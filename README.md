# Procedural Terrain Generation in Unity

Проект по процедурной генерации ландшафта в Unity, включающий систему биомов, водную поверхность и растительность.

## Основные функции

- Процедурная генерация ландшафта с использованием шума Перлина
- Система биомов (пустыня, равнина, лес, горы, снег)
- Динамическая водная поверхность
- Система растительности с учетом биомов
- Бесконечная генерация мира с системой чанков
- Камера с полным контролем движения и обзора

## Управление камерой

- **W/S** - движение вперед/назад
- **A/D** - движение влево/вправо
- **Q/E** - вращение камеры влево/вправо
- **R/F** - изменение высоты камеры
- **Правая кнопка мыши + движение мыши** - вращение камеры
- **Колесико мыши** - приближение/отдаление

## Настройка в Unity Editor

### WorldGenerator
- Height Multiplier: 30
- Ambient Occlusion Strength: 0.5
- Ambient Occlusion Radius: 1

### BiomeGenerator
- Biome Height Multiplier: 20
- Temperature Noise Scale: 100
- Moisture Noise Scale: 100
- Snow Height Threshold: 0.7
- Mountain Height Threshold: 0.5
- Desert Temperature Threshold: 0.7
- Forest Moisture Threshold: 0.6

### Цвета биомов
- Пустыня: (0.76, 0.7, 0.5)
- Равнина: (0.2, 0.8, 0.2)
- Лес: (0.1, 0.5, 0.1)
- Горы: (0.5, 0.5, 0.5)
- Снег: (1, 1, 1)

### CameraController
- Move Speed: 20
- Rotation Speed: 100
- Mouse Rotation Speed: 2
- Smooth Time: 0.1
- Min Height: 5
- Max Height: 100
- Height Change Speed: 20
- Min Zoom: 10
- Max Zoom: 100
- Zoom Speed: 5

## Цель работы
- Создать собственную процедурную генерацию ландшафта
- Написать собственные материалы, простые шейдеры
