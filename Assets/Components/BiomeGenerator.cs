using UnityEngine;

public class BiomeGenerator : MonoBehaviour {
    [Header("Biome Settings")]
    public float biomeNoiseScale = 50f;
    public float biomeHeightMultiplier = 20f;
    public float temperatureNoiseScale = 100f;
    public float moistureNoiseScale = 100f;
    public Vector2 biomeOffset;

    [Header("Biome Thresholds")]
    [Range(0f, 1f)] public float snowHeightThreshold = 0.7f;
    [Range(0f, 1f)] public float mountainHeightThreshold = 0.5f;
    [Range(0f, 1f)] public float desertTemperatureThreshold = 0.7f;
    [Range(0f, 1f)] public float forestMoistureThreshold = 0.6f;

    [Header("Biome Colors")]
    public Color desertColor = new Color(0.76f, 0.7f, 0.5f);
    public Color plainsColor = new Color(0.2f, 0.8f, 0.2f);
    public Color forestColor = new Color(0.1f, 0.5f, 0.1f);
    public Color mountainColor = new Color(0.5f, 0.5f, 0.5f);
    public Color snowColor = new Color(1f, 1f, 1f);

    [Header("Biome Textures")]
    public Texture2D plainsTexture;
    public Texture2D forestTexture;
    public Texture2D desertTexture;
    public Texture2D mountainTexture;

    void Start() {
        if (biomeOffset == Vector2.zero) {
            biomeOffset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        }
    }

    public BiomeType GetBiomeAtPosition(Vector3 position, float height) {
        float temperature = GenerateTemperature(position);
        float moisture = GenerateMoisture(position);
        
        // Нормализуем высоту в диапазон 0-1
        float normalizedHeight = Mathf.Clamp01(height / biomeHeightMultiplier);
        
        // Определяем биом на основе высоты, температуры и влажности
        if (normalizedHeight > snowHeightThreshold) {
            return BiomeType.Snow;
        }
        else if (normalizedHeight > mountainHeightThreshold) {
            return BiomeType.Mountain;
        }
        else if (temperature > desertTemperatureThreshold) {
            return BiomeType.Desert;
        }
        else if (moisture > forestMoistureThreshold) {
            return BiomeType.Forest;
        }
        else {
            return BiomeType.Plains;
        }
    }

    float GenerateTemperature(Vector3 position) {
        // Используем два слоя шума для более интересной температуры
        float x = (position.x + biomeOffset.x) / temperatureNoiseScale;
        float z = (position.z + biomeOffset.y) / temperatureNoiseScale;
        
        float temp1 = Mathf.PerlinNoise(x, z);
        float temp2 = Mathf.PerlinNoise(x * 2f, z * 2f) * 0.5f;
        
        return Mathf.Clamp01(temp1 + temp2);
    }

    float GenerateMoisture(Vector3 position) {
        // Используем два слоя шума для более интересной влажности
        float x = (position.x + biomeOffset.x + 1000f) / moistureNoiseScale; // Смещаем для разнообразия
        float z = (position.z + biomeOffset.y + 1000f) / moistureNoiseScale;
        
        float moist1 = Mathf.PerlinNoise(x, z);
        float moist2 = Mathf.PerlinNoise(x * 2f, z * 2f) * 0.5f;
        
        return Mathf.Clamp01(moist1 + moist2);
    }

    public Color GetBiomeColor(BiomeType biome) {
        switch (biome) {
            case BiomeType.Desert:
                return desertColor;
            case BiomeType.Plains:
                return plainsColor;
            case BiomeType.Forest:
                return forestColor;
            case BiomeType.Mountain:
                return mountainColor;
            case BiomeType.Snow:
                return snowColor;
            default:
                return Color.white;
        }
    }

    public Texture2D GetBiomeTexture(BiomeType biome) {
        switch (biome) {
            case BiomeType.Plains: return plainsTexture;
            case BiomeType.Forest: return forestTexture;
            case BiomeType.Desert: return desertTexture;
            case BiomeType.Mountain: return mountainTexture;
            default: return null;
        }
    }
}