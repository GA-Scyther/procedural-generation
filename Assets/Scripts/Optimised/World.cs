using UnityEngine;

public class World
{
    // Generate Chunks around player
    // Store loaded chunks in a Dictionary<Vector2Int, Chunk>
    // 

    public int _worldSize;
    public ComputeShader _meshGenerator;

    [Header("Noise Settings")]
    public int _chunkSize = 16;
    public float _heightMultiplier = 5f;

    public int _octaves = 6;
    public Vector2[] _octavesOffset;

    public float _persistance = 0.5f;
    public float _lacunarity = 2f;

    public float _minValue;
    public float _maxValue;

    private Chunk[,] _chunks;

    public World(int worldSize, ComputeShader meshGenerator, int chunkSize, float heightMultiplier, int octaves, float persistance, float lacunarity)
    {
        _worldSize = worldSize;
        _meshGenerator = meshGenerator;
        _chunkSize = chunkSize;
        _heightMultiplier = heightMultiplier;
        _octaves = octaves;
        _persistance = persistance;
        _lacunarity = lacunarity;


    }

    public void GenerateWorld()
    {
        _chunks = new Chunk[_worldSize, _worldSize];

        for (int x = 0; x < _worldSize; x++)
        {
            for (int y = 0; y < _worldSize; y++)
            {
                _chunks[x, y] = new Chunk(new Vector2Int(x, y), this);
                _chunks[x, y].LoadChunk();
            }
        }
    }



}
