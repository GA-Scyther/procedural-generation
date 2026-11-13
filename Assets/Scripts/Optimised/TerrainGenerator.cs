using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{


    private WorldInstance _world;
    private World _world2;

    public int _size = 256;
    public int _seed = 23;
    public float _scale = 1;
    public int _chunkSize = 64;
    public float _worldScale = 2;
    public float _heightScale = 2;
    public float2 _offset = new float2();
    public float _persistance = 0.5f;
    public float _lacunarity = 2f;
    public int _octaves = 6;
    public ComputeShader _computeShader;

    public MeshFilter _meshFilter;
    public MeshRenderer _meshRenderer;
    public MeshCollider _meshCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Generate();
    }


    private async void Generate()
    {
        _world = new WorldInstance();

        WorldInstance.WorldData worldData = await _world.GenerateWorld(_seed, _size, _scale, _worldScale, _heightScale, _offset, _persistance, _lacunarity, _octaves, _computeShader);

        _meshFilter.mesh = worldData.mesh;
        _meshRenderer.sharedMaterial.mainTexture = worldData.texture;
    }

    private async void Generate2()
    {
        _world2 = new World(_size, _computeShader, _chunkSize, _heightScale, _octaves, _persistance, _lacunarity);
        _world2.GenerateWorld();
    }    

    private void OnValidate()
    {
        //Generate();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
