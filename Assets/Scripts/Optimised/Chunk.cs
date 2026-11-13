using System;
using System.Drawing;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk
{
    // Take Input Position
    // Generate Mesh for Chunk (Size + 1 Verticies)
    // Generate Perlin Noise for Tile (Terrain, Precipitation???)
    // Modify Terrain By Terrain Noise

    // Generate Tile Map from terrain and precipitation???

    public Vector2Int _chunkPosition;

    public World _world;

    public float[,] _terrainData;

    public GameObject _gameObject;

    public Chunk(Vector2Int chunkPosition, World world)
    {
        _chunkPosition = chunkPosition;
        _world = world;
    }

    public async Awaitable LoadChunk()
    {
        Awaitable<float[,]> terrainDataGen = GenerateTerrainData();
        Awaitable<Mesh> meshGen = GenerateMesh();

        _terrainData = await terrainDataGen;
        Mesh visualMesh = await meshGen;



    }

    public async Awaitable<Mesh> ApplyTerrainData()
    {
        Mesh mesh = new Mesh();


        Vector3[] verticies = mesh.vertices;


        for (int i = 0; i < verticies.Length; i++)
        {
            int x = (int)Mathf.Floor(i / _world._chunkSize);
            int y = (i % _world._chunkSize);
            verticies[i] = new Vector3(verticies[i].x, verticies[i].y + _terrainData[x, y], verticies[i].z);
        }



        return mesh;
    }

    public async Awaitable<Mesh> GenerateMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[_world._chunkSize * _world._chunkSize];
        Vector3[] normals = new Vector3[_world._chunkSize * _world._chunkSize];
        Vector2[] uvs = new Vector2[_world._chunkSize * _world._chunkSize];
        // Number of squares * 3 Vertexs * 2 tris
        int[] triangles = new int[(_world._chunkSize - 1) * (_world._chunkSize - 1) * 3 * 2];

        uint[] triangleIndex = new uint[_world._chunkSize * _world._chunkSize];
        uint tempIndex = 0;
        for (int i = 0; i < _world._chunkSize * _world._chunkSize; i++)
        {
            if (((i + 1) % _world._chunkSize != 0) && (i / _world._chunkSize < _world._chunkSize - 1))
            {
                triangleIndex[i] = tempIndex;
                tempIndex++;
            }
        }

        ComputeBuffer triangleIndexBuffer = new ComputeBuffer(_world._chunkSize * _world._chunkSize, sizeof(int));
        ComputeBuffer verticesBuffer = new ComputeBuffer(_world._chunkSize * _world._chunkSize, sizeof(float) * 3);
        ComputeBuffer normalsBuffer = new ComputeBuffer(_world._chunkSize * _world._chunkSize, sizeof(float) * 3);
        ComputeBuffer uvsBuffer = new ComputeBuffer(_world._chunkSize * _world._chunkSize, sizeof(float) * 2);
        ComputeBuffer trianglesBuffer = new ComputeBuffer((_world._chunkSize - 1) * (_world._chunkSize - 1) * 3 * 2, sizeof(uint));

        triangleIndexBuffer.SetData(triangleIndex);

        //inputBuffer.SetData(inputData);
        int kernelIndex = _world._meshGenerator.FindKernel("CSMain");


        _world._meshGenerator.SetBuffer(kernelIndex, "_triangleIndex", triangleIndexBuffer);
        _world._meshGenerator.SetBuffer(kernelIndex, "_verticies", verticesBuffer);
        _world._meshGenerator.SetBuffer(kernelIndex, "_normals", normalsBuffer);
        _world._meshGenerator.SetBuffer(kernelIndex, "_uvs", uvsBuffer);
        _world._meshGenerator.SetBuffer(kernelIndex, "_triangles", trianglesBuffer);
        _world._meshGenerator.SetInt("size", _world._chunkSize);
        _world._meshGenerator.SetInt("limit", _world._chunkSize * _world._chunkSize);

        int threadGroups = Mathf.CeilToInt((float)_world._chunkSize * _world._chunkSize / 64f);
        //print(threadGroups);
        _world._meshGenerator.Dispatch(kernelIndex, threadGroups, 1, 1);

        verticesBuffer.GetData(vertices);
        normalsBuffer.GetData(normals);
        uvsBuffer.GetData(uvs);
        trianglesBuffer.GetData(triangles);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        verticesBuffer.Release();
        normalsBuffer.Release();
        uvsBuffer.Release();
        trianglesBuffer.Release();

        return mesh;
    }

    public async Awaitable<float[,]> GenerateTerrainData()
    {
        float[,] terrainData = new float[_world._chunkSize, _world._chunkSize];



        for (int x = 0; x < _world._chunkSize; x++)
        {
            for (int y = 0; y < _world._chunkSize; y++)
            {
                float amplitude = 1, frequency = 1;
                float value = 0;

                foreach (Vector2 octave in _world._octavesOffset)
                {
                    float sampleX = ((x) + (_chunkPosition.x * _world._chunkSize)) / _world._heightMultiplier * frequency + octave.x;
                    float sampleY = ((y) + (_chunkPosition.y * _world._chunkSize)) / _world._heightMultiplier * frequency + octave.y;

                    float noise = Mathf.PerlinNoise(sampleX, sampleY);

                    value += noise * amplitude;
                    amplitude *= _world._persistance;
                    frequency *= _world._lacunarity;

                    //if (value < minValue) minValue = value;
                    //if (value > maxValue) maxValue = value;
                }

                value = Mathf.InverseLerp(_world._minValue, _world._maxValue, value);
                terrainData[x, y] = value;
            }
        }

        return terrainData;
    }



}
