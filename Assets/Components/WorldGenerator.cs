using UnityEngine;

public class WorldGenerator : MonoBehaviour {
    [Header("Terrain Settings")]
    public int mapSize = 100;
    public float noiseScale = 50f;
    public int octaves = 6;
    public float persistence = 0.6f;
    public float lacunarity = 2f;
    public float heightMultiplier = 30f;
    public Vector2 offset;

    [Header("Detail Settings")]
    public float detailNoiseScale = 100f;
    public float detailNoiseStrength = 5f;
    public float detailNoiseOffset = 1000f;

    [Header("Biome Settings")]
    public BiomeGenerator biomeGenerator;
    public float biomeNoiseScale = 50f;
    public float biomeHeightMultiplier = 20f;

    [Header("Lighting Settings")]
    public float ambientOcclusionStrength = 0.5f;
    public float ambientOcclusionRadius = 1f;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;
    private Vector2[] uvs;
    private Vector3[] normals;
    private BiomeType[] biomeTypes;
    private bool isInitialized = false;

    void Start() {
        if (biomeGenerator == null) {
            biomeGenerator = GetComponent<BiomeGenerator>();
            if (biomeGenerator == null) {
                biomeGenerator = gameObject.AddComponent<BiomeGenerator>();
            }
        }



        GenerateTerrain();
        isInitialized = true;
    }

    public void GenerateTerrain() {
        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
        }
        
        if (meshCollider == null) {
            meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null) {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
        }

        if (mesh == null) {
            mesh = new Mesh();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
        
        CreateVertices();
        CreateTriangles();
        CreateUVs();
        CreateBiomeTypes();
        CreateColors();
        CalculateNormals();
        CalculateAmbientOcclusion();
        UpdateMesh();
        
    }

    void CreateVertices() {
        vertices = new Vector3[(mapSize + 1) * (mapSize + 1)];
        
        for (int i = 0, z = 0; z <= mapSize; z++) {
            for (int x = 0; x <= mapSize; x++) {
                float y = GenerateHeight(x, z);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
    }

    void CreateTriangles() {
        triangles = new int[mapSize * mapSize * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < mapSize; z++) {
            for (int x = 0; x < mapSize; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + mapSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + mapSize + 1;
                triangles[tris + 5] = vert + mapSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void CreateUVs() {
        uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x / mapSize, vertices[i].z / mapSize);
        }
    }

    void CreateBiomeTypes() {
        biomeTypes = new BiomeType[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            biomeTypes[i] = biomeGenerator.GetBiomeAtPosition(worldPos, vertices[i].y);
        }
    }

    void CreateColors() {
        colors = new Color[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            BiomeType biome = biomeGenerator.GetBiomeAtPosition(worldPos, vertices[i].y);
            colors[i] = biomeGenerator.GetBiomeColor(biome);
        }
    }

    void CalculateNormals() {
        normals = new Vector3[vertices.Length];
        
        // Сначала обнуляем все нормали
        for (int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.zero;
        }

        // Вычисляем нормали для каждого треугольника
        for (int i = 0; i < triangles.Length; i += 3) {
            int vertexIndexA = triangles[i];
            int vertexIndexB = triangles[i + 1];
            int vertexIndexC = triangles[i + 2];

            Vector3 vertexA = vertices[vertexIndexA];
            Vector3 vertexB = vertices[vertexIndexB];
            Vector3 vertexC = vertices[vertexIndexC];

            Vector3 normal = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;
            
            normals[vertexIndexA] += normal;
            normals[vertexIndexB] += normal;
            normals[vertexIndexC] += normal;
        }

        // Нормализуем все нормали
        for (int i = 0; i < normals.Length; i++) {
            normals[i] = normals[i].normalized;
        }
    }

    void CalculateAmbientOcclusion() {
        Color[] aoColors = new Color[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++) {
            float ao = 0f;
            int samples = 8;
            
            for (int j = 0; j < samples; j++) {
                float angle = (j / (float)samples) * 360f * Mathf.Deg2Rad;
                Vector3 samplePos = vertices[i] + new Vector3(
                    Mathf.Cos(angle) * ambientOcclusionRadius,
                    0,
                    Mathf.Sin(angle) * ambientOcclusionRadius
                );
                
                float sampleHeight = GetHeightAtPosition(samplePos);
                if (sampleHeight > vertices[i].y) {
                    ao += 1f;
                }
            }
            
            ao = 1f - (ao / samples) * ambientOcclusionStrength;
            
            // Применяем ambient occlusion к существующему цвету биома
            Color biomeColor = colors[i];
            aoColors[i] = new Color(
                biomeColor.r * ao,
                biomeColor.g * ao,
                biomeColor.b * ao,
                biomeColor.a
            );
        }
        
        colors = aoColors;
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.colors = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        meshCollider.sharedMesh = mesh;
    }

    float GenerateHeight(int x, int z) {
        float baseNoise = GenerateOctaveNoise(x, z);
        float detailNoise = GenerateDetailNoise(x, z);
        float finalHeight = baseNoise + detailNoise;
        finalHeight = Mathf.Clamp(finalHeight, 0f, heightMultiplier);
        return finalHeight;
    }

    float GenerateOctaveNoise(int x, int z) {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        float amplitudeSum = 0f;

        for (int i = 0; i < octaves; i++) {
            float sampleX = (x + offset.x) / noiseScale * frequency;
            float sampleZ = (z + offset.y) / noiseScale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
            noiseHeight += perlinValue * amplitude;
            amplitudeSum += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return (noiseHeight / amplitudeSum) * heightMultiplier;
    }

    float GenerateDetailNoise(int x, int z) {
        float sampleX = (x + detailNoiseOffset) / detailNoiseScale;
        float sampleZ = (z + detailNoiseOffset) / detailNoiseScale;
        
        float detailNoise = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
        return detailNoise * detailNoiseStrength;
    }

    public float GetHeightAtPosition(Vector3 worldPos) {
        if (!isInitialized) return 0f;
        
        Vector3 localPos = worldPos - transform.position;
        
        if (localPos.x < 0 || localPos.x > mapSize || 
            localPos.z < 0 || localPos.z > mapSize) {
            return 0f;
        }
        
        int x = Mathf.FloorToInt(localPos.x);
        int z = Mathf.FloorToInt(localPos.z);
        
        float height00 = vertices[z * (mapSize + 1) + x].y;
        float height10 = vertices[z * (mapSize + 1) + (x + 1)].y;
        float height01 = vertices[(z + 1) * (mapSize + 1) + x].y;
        float height11 = vertices[(z + 1) * (mapSize + 1) + (x + 1)].y;
        
        float u = localPos.x - x;
        float v = localPos.z - z;
        
        float height0 = Mathf.Lerp(height00, height10, u);
        float height1 = Mathf.Lerp(height01, height11, u);
        
        return Mathf.Lerp(height0, height1, v);
    }
}