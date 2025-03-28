using UnityEngine;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    public GameObject chunkPrefab;
    public int chunkSize = 100;
    public int viewDistance = 2;
    
    [Header("Generation Settings")]
    public float noiseScale = 50f;
    public int octaves = 6;
    public float persistence = 0.6f;
    public float lacunarity = 2f;
    public float heightMultiplier = 30f;
    public float detailNoiseScale = 100f;
    public float detailNoiseStrength = 5f;
    public float detailNoiseOffset = 1000f;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Camera mainCamera;
    private Vector2Int lastChunkPosition;
    private Vector2 worldOffset;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }
        if (chunkPrefab == null)
        {
            Debug.LogError("Chunk prefab not assigned!");
            return;
        }
        
        worldOffset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        
        lastChunkPosition = GetChunkPosition(mainCamera.transform.position);
        GenerateChunks();
    }

    void Update()
    {
        if (mainCamera == null) return;
        
        Vector2Int currentChunkPosition = GetChunkPosition(mainCamera.transform.position);
        if (currentChunkPosition != lastChunkPosition)
        {
            lastChunkPosition = currentChunkPosition;
            GenerateChunks();
        }
    }

    Vector2Int GetChunkPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / chunkSize),
            Mathf.FloorToInt(worldPosition.z / chunkSize)
        );
    }

    void GenerateChunks()
    {
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
        
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkPos = lastChunkPosition + new Vector2Int(x, z);
                chunksToKeep.Add(chunkPos);
                
                if (!activeChunks.ContainsKey(chunkPos))
                {
                    CreateChunk(chunkPos);
                }
            }
        }
        
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }
        
        foreach (var chunkPos in chunksToRemove)
        {
            if (activeChunks.TryGetValue(chunkPos, out GameObject chunk))
            {
                Destroy(chunk);
                activeChunks.Remove(chunkPos);
            }
        }
    }

    void CreateChunk(Vector2Int chunkPos)
    {
        Vector3 worldPos = new Vector3(
            chunkPos.x * chunkSize,
            0,
            chunkPos.y * chunkSize
        );
        
        GameObject chunk = Instantiate(chunkPrefab, worldPos, Quaternion.identity);
        chunk.transform.parent = transform;
        
        WorldGenerator generator = chunk.GetComponent<WorldGenerator>();
        if (generator != null)
        {
            generator.mapSize = chunkSize;
            generator.noiseScale = noiseScale;
            generator.octaves = octaves;
            generator.persistence = persistence;
            generator.lacunarity = lacunarity;
            generator.heightMultiplier = heightMultiplier;
            generator.detailNoiseScale = detailNoiseScale;
            generator.detailNoiseStrength = detailNoiseStrength;
            generator.detailNoiseOffset = detailNoiseOffset;
            
            // Используем мировое смещение + позицию чанка для создания непрерывного ландшафта
            generator.offset = new Vector2(
                worldOffset.x + chunkPos.x * chunkSize,
                worldOffset.y + chunkPos.y * chunkSize
            );
            
            generator.GenerateTerrain();
        }
        else
        {
            Debug.LogError("WorldGenerator component not found on chunk prefab!");
            Destroy(chunk);
            return;
        }
        
        activeChunks.Add(chunkPos, chunk);
    }

    public WorldGenerator GetChunkFromPosition(Vector3 worldPosition)
    {
        Vector2Int chunkPos = GetChunkPosition(worldPosition);
        if (activeChunks.TryGetValue(chunkPos, out GameObject chunk))
        {
            return chunk.GetComponent<WorldGenerator>();
        }
        return null;
    }
} 