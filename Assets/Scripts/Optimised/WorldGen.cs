using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static World;

public class WorldGen : MonoBehaviour
{

    private World world;

    public int _size = 32;
    public int _seed = 23;
    public float _scale = 50;
    public float _heightScale = 5;

    [Range(0, 10)] public int _collisionLod = 0;

    public int _octaveQuantity = 6;
    public float _persistance = 0.5f;
    public float _lacunarity = 2f;

    public AnimationCurve _fallofCurve;


    public MeshRenderer _meshRenderer;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollidier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void OnValidate()
    {
        World.CombinedWorldData combinedWorldData = GenerateWorld(_size, _seed, _octaveQuantity, _persistance, _lacunarity, _scale, _heightScale, _collisionLod, _fallofCurve);
        _meshFilter.sharedMesh = combinedWorldData.meshData.GetMesh();
        Texture2D texture = combinedWorldData.meshData.GetTexture();
        texture.filterMode = FilterMode.Point;
        _meshRenderer.sharedMaterial.mainTexture = texture;
        _meshCollidier.sharedMesh = combinedWorldData.collisionData.GetMesh();
    }

    public World.CombinedWorldData GenerateWorld(int size, int seed, int octaveQuantity, float persistance, float lacunarity, float scale, float heightScale, int collisionLod, AnimationCurve fallofCurve)
    {
        System.Random rng = new System.Random(seed);

        Vector2[] octaves = new Vector2[octaveQuantity];
        for (int i = 0; i < octaveQuantity; i++)
        {
            octaves[i] = new Vector2(rng.Next(-1000, 1000), rng.Next(-1000, 1000));
        }

        world = new World(size, octaves, persistance, lacunarity, scale, heightScale, fallofCurve);

        World.CombinedWorldData worldData = world.GenerateWorld(collisionLod,  true);

        return worldData;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
