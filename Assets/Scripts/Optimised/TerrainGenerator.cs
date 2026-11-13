using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{


    private World _world;

    public int _size = 256;
    public int _seed = 23;
    public float _scale = 1;
    public float _worldScale = 2;
    public float _heightScale = 2;
    public float2 _offset = new float2();
    public float _persistance = 0.5f;
    public float _lacunarity = 2f;
    public int _octaves = 6;
    public ComputeShader _computeShader;

    public MeshFilter _meshFilter;
    public MeshRenderer _meshRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate();
    }


    private async void Generate()
    {
        _world = new World();

        World.WorldData worldData = await _world.GenerateWorld(_seed, _size, _scale, _worldScale, _heightScale, _offset, _persistance, _lacunarity, _octaves, _computeShader);

        _meshFilter.mesh = worldData.mesh;
        _meshRenderer.sharedMaterial.mainTexture = worldData.texture;
    }


    private void OnValidate()
    {
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
