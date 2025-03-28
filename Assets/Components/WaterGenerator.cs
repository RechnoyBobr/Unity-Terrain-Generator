using UnityEngine;
using System.Collections.Generic;

public class WaterGenerator : MonoBehaviour {
    [Header("Water Settings")]
    public GameObject waterPrefab;
    public float waterLevelOffset = 0f;
    [Header("Terrain Reference")]
    public ChunkManager chunkManager;
    [Header("Wave Noise")]
    public float waveSpeed = 0.5f;
    public float waveScale = 10f;
    public float waveHeight = 0.5f;

    private Dictionary<Vector2Int, GameObject> waterChunks = new Dictionary<Vector2Int, GameObject>();
    private float lastUpdateTime = 0f;
    private float updateInterval = 0.5f;

    void Start() {
        chunkManager = FindFirstObjectByType<ChunkManager>();
        if (chunkManager == null) {
            Debug.LogError("ChunkManager not found!");
            return;
        }
    }

    void Update() {
        if (chunkManager == null) return;
        
        UpdateWaterChunks();
        
        if (Time.time - lastUpdateTime >= updateInterval) {
            UpdateWaterPositions();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateWaterChunks() {
        // Получаем позицию камеры в координатах чанков
        Vector3 cameraPos = Camera.main.transform.position;
        Vector2Int currentChunk = new Vector2Int(
            Mathf.FloorToInt(cameraPos.x / chunkManager.chunkSize),
            Mathf.FloorToInt(cameraPos.z / chunkManager.chunkSize)
        );

        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
        for (int x = -1; x <= 1; x++) {
            for (int z = -1; z <= 1; z++) {
                Vector2Int chunkPos = currentChunk + new Vector2Int(x, z);
                chunksToKeep.Add(chunkPos);
            }
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in waterChunks) {
            if (!chunksToKeep.Contains(chunk.Key)) {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove) {
            if (waterChunks.TryGetValue(chunkPos, out GameObject waterChunk)) {
                Destroy(waterChunk);
                waterChunks.Remove(chunkPos);
            }
        }

        foreach (var chunkPos in chunksToKeep) {
            if (!waterChunks.ContainsKey(chunkPos)) {
                CreateWaterChunk(chunkPos);
            }
        }
    }

    void CreateWaterChunk(Vector2Int chunkPos) {
        Vector3 worldPos = new Vector3(
            chunkPos.x * chunkManager.chunkSize + chunkManager.chunkSize / 2f,
            0,
            chunkPos.y * chunkManager.chunkSize + chunkManager.chunkSize / 2f
        );

        GameObject waterChunk = Instantiate(waterPrefab, worldPos, Quaternion.identity);
        waterChunk.transform.localScale = new Vector3(chunkManager.chunkSize, 1, chunkManager.chunkSize);
        waterChunks.Add(chunkPos, waterChunk);
    }

    void UpdateWaterPositions() {
        foreach (var waterChunk in waterChunks) {
            if (waterChunk.Value != null) {
                UpdateWaterPosition(waterChunk.Value, waterChunk.Key);
            }
        }
    }

    void UpdateWaterPosition(GameObject waterChunk, Vector2Int chunkPos) {
        float totalHeight = 0f;
        int samples = 5;
        int validSamples = 0;

        Vector3 chunkCenter = waterChunk.transform.position;
        
        for (int i = 0; i < samples; i++) {
            for (int j = 0; j < samples; j++) {
                float x = chunkCenter.x - chunkManager.chunkSize/2f + chunkManager.chunkSize * (i / (float)samples);
                float z = chunkCenter.z - chunkManager.chunkSize/2f + chunkManager.chunkSize * (j / (float)samples);
                
                WorldGenerator chunk = chunkManager.GetChunkFromPosition(new Vector3(x, 0, z));
                if (chunk != null) {
                    float height = chunk.GetHeightAtPosition(new Vector3(x, 0, z));
                    if (height != 0f) {
                        totalHeight += height;
                        validSamples++;
                    }
                }
            }
        }

        if (validSamples > 0) {
            float averageHeight = totalHeight / validSamples;
            Vector3 pos = waterChunk.transform.position;
            pos.y = averageHeight + waterLevelOffset;
            waterChunk.transform.position = pos;
        }
    }

    void UpdateWaveEffect() {
        foreach (var waterChunk in waterChunks) {
            if (waterChunk.Value != null) {
                Renderer renderer = waterChunk.Value.GetComponent<Renderer>();
                renderer.material.SetTextureOffset("_MainTex", 
                    new Vector2(Time.time * waveSpeed, 0));
                renderer.material.SetFloat("_WaveScale", waveScale);
                renderer.material.SetFloat("_WaveHeight", waveHeight);
            }
        }
    }
}