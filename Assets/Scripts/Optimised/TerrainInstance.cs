using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.DebugUI.Table;


public class WorldInstance
{
    public struct WorldData
    {
        public Mesh mesh;
        public Mesh collider;
        public Texture2D texture;
    }

    public async Awaitable<WorldData> GenerateWorld(int seed, int size, float scale, float worldScale, float heightScale, float2 offset, float persistance, float lacunarity, int octaves, ComputeShader computeShader)
    {
        // Start Mesh Creation
        //Mesh mesh = new Mesh();
        WorldData worldData = new WorldData();
        worldData.mesh = new Mesh();


        #region GPU Mesh Generation
        Vector3[] vertices = new Vector3[size * size];
        Vector3[] normals = new Vector3[size * size];
        Vector2[] uvs = new Vector2[size * size];
        // Number of squares * 3 Vertexs * 2 tris
        int[] triangles = new int[(size - 1) * (size - 1) * 3 * 2];

        uint[] triangleIndex = new uint[size * size];
        uint tempIndex = 0;
        for (int i = 0; i < size * size; i++)
        {
            if (((i + 1) % size != 0) && (i / size < size - 1))
            {
                triangleIndex[i] = tempIndex;
                tempIndex++;
            }
        }

        ComputeBuffer triangleIndexBuffer = new ComputeBuffer(size * size, sizeof(int));
        ComputeBuffer verticesBuffer = new ComputeBuffer(size * size, sizeof(float) * 3);
        ComputeBuffer normalsBuffer = new ComputeBuffer(size * size, sizeof(float) * 3);
        ComputeBuffer uvsBuffer = new ComputeBuffer(size * size, sizeof(float) * 2);
        ComputeBuffer trianglesBuffer = new ComputeBuffer((size - 1) * (size - 1) * 3 * 2, sizeof(uint));

        triangleIndexBuffer.SetData(triangleIndex);

        int kernelIndex = computeShader.FindKernel("CSMain");


        computeShader.SetBuffer(kernelIndex, "_triangleIndex", triangleIndexBuffer);
        computeShader.SetBuffer(kernelIndex, "_verticies", verticesBuffer);
        computeShader.SetBuffer(kernelIndex, "_normals", normalsBuffer);
        computeShader.SetBuffer(kernelIndex, "_uvs", uvsBuffer);
        computeShader.SetBuffer(kernelIndex, "_triangles", trianglesBuffer);
        computeShader.SetInt("size", size);
        computeShader.SetFloat("scale", worldScale);
        computeShader.SetInt("limit", size * size);
        #endregion

        // Calculate Thread Groups required
        int threadGroups = Mathf.CeilToInt((float)size / 8f);

        // Dispate to gpu
        computeShader.Dispatch(kernelIndex, threadGroups, threadGroups, 1);


        // Get min max values based on number of octaves
        #region Job Struct Setup
        GetNoiseMinMax(persistance, lacunarity, octaves, out float minValue, out float maxValue);

        // Create new random 
        System.Random _rng = new System.Random(seed);

        NativeArray<float> matrix = new NativeArray<float>(size * size, Allocator.TempJob);
        NativeArray<float2> octavesOffsets = new NativeArray<float2>(octaves, Allocator.TempJob);

        // Generate octave offsets
        for (int i = 0; i < octaves; i++)
        {
            octavesOffsets[i] = new float2(_rng.Next(-1000, 1000), _rng.Next(-1000, 1000));
        }
        #endregion

        #region Create PerlinNoise Job
        // Create job struct for chunk loader
        ChunkLoader chunkLoader = new ChunkLoader()
        {
            _minValue = minValue,
            _maxValue = maxValue,

            _lacunarity = lacunarity,
            _persistance = persistance,

            _offset = offset,

            _matrix = matrix,
            _octaves = octavesOffsets,

            _size = size,
            _scale = scale
        };

        // Start Batch Creation of perlin noise
        //int chunkGroups = Mathf.CeilToInt((float)size * size / 64f);
        JobHandle chunkHandle = chunkLoader.Schedule(size * size, 64);



        #endregion


        #region Get Data
        // Wait for 
        chunkHandle.Complete();

        float[] floatMatrix = new float[size * size];
        matrix.CopyTo(floatMatrix);

        Awaitable<Texture2D> matrixTexture = GetTexture(floatMatrix, size);

        verticesBuffer.GetData(vertices);
        normalsBuffer.GetData(normals);
        uvsBuffer.GetData(uvs);
        trianglesBuffer.GetData(triangles);

        // Use "matrix" for job output

        #endregion
        NativeArray<Vector3> verticiesVector3 = new NativeArray<Vector3>(vertices, Allocator.TempJob);

        NativeArray <float3> verticiesFloat3 = verticiesVector3.Reinterpret<float3>();

        // Update Verticies from height from height map
        ChunkHeightModifier heightModifier = new ChunkHeightModifier()
        {
            _matrix = matrix,
            _verticies = verticiesFloat3,

            _scale = heightScale
        };

        JobHandle heightModifiderHandle = heightModifier.Schedule(size * size, 64);

        heightModifiderHandle.Complete();
        verticiesFloat3.Reinterpret<Vector3>().CopyTo(vertices);

        // Use "vertices" for job output

        worldData.mesh.vertices = vertices;
        worldData.mesh.triangles = triangles;
        worldData.mesh.normals = normals;
        worldData.mesh.uv = uvs;

        // Wait For Texture
        worldData.texture = await matrixTexture;

        // Cleanup
        #region Cleanup
        verticesBuffer.Release();
        normalsBuffer.Release();
        uvsBuffer.Release();
        trianglesBuffer.Release();
        triangleIndexBuffer.Release();


        matrix.Dispose();
        octavesOffsets.Dispose();
        verticiesVector3.Dispose();
        #endregion


        return worldData;
    }



    // Get Noise for math.cnoise (-1, 1) instead of (0, 1)
    public void GetNoiseMinMax(float persistance, float lacunarity, int octaves, out float min, out float max)
    {
        min = 0;
        max = 0;
        float amplitude = 1f, frequency = 1f;


        for (int i = 0; i < octaves; i++)
        {
            // Min should be -1 for cnoise and 0 for Mathf.PerlinNoise
            // Changed to 0-2 for cnoise
            min = 0f * 2f - 1f;
            max = 1f * 2f - 1f;

            min += min * amplitude;
            max += max * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }
    }

    public async Awaitable<Texture2D> GetTexture(float[,] matrix)
    {
        //return TextureGenerator.TextureFromMatrix(matrix, point: false);
        return TextureGenerator.TextureFromMatrix(matrix, Color.black, Color.white, point: false);
    }

    public async Awaitable<Texture2D> GetTexture(float[] array, int size)
    {
        float[,] matrix = new float[size, size];
        for (int i = 0; i < array.Length; i++)
        {
            matrix[i / size, i % size] = array[i];
        }

        return await GetTexture(matrix);
    }

}
