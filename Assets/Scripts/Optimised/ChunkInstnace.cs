using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

/*public class Chunk
{
    private float[,] _chunkMatrix;
    // Chunk position
    private Vector2Int _position;

    public Chunk(Vector2Int position)
    {
        _position = position;
    }

    public async void GenerateChunk()
    {
        
    }

}*/

[BurstCompile]
public struct ChunkLoader : IJobParallelFor
{
    // Output Matrix
    public NativeArray<float> _matrix;

    // Input Parameters
    public int _size;
    public float _scale;

    public float _persistance;
    public float _lacunarity;

    [ReadOnly] public NativeArray<float2> _octaves;
    public float2 _offset;

    public float _minValue;
    public float _maxValue;

    public void Execute(int index)
    {
        float amplitude = 1, frequency = 1;
        float value = 0;

        int x = index % _size;
        int y = index / _size;

        // Do operation once outside loop, without octave's so that it doesn't waste operations


        foreach (float2 octave in _octaves)
        {
            //float sampleX = ((x - _size / 2) + _offset.x) / _scale * frequency + octave.x;
            //float sampleY = ((y - _size / 2) + _offset.y) / _scale * frequency + octave.y;

            float sampleX = ((x - _size / 2f) + _offset.x) / _scale * frequency + octave.x;
            float sampleY = ((y - _size / 2f) + _offset.y) / _scale * frequency + octave.y;

            float2 samplePosition = new(sampleX, sampleY);

            //float noise = Mathf.PerlinNoise(sampleX + octave.x, sampleY + octave.y) * 2 - 1;
            float outputNoise = noise.cnoise(samplePosition) * 2 - 1;

            value += outputNoise * amplitude;
            amplitude *= _persistance;
            frequency *= _lacunarity;

            //if (value < minValue) minValue = value;
            //if (value > maxValue) maxValue = value;
        }

        value = Mathf.InverseLerp(_minValue, _maxValue, value);
        _matrix[index] = value;
    }
}


[BurstCompile]
public struct ChunkHeightModifier : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> _matrix;
    public NativeArray<float3> _verticies;
    public float _scale;

    public void Execute(int index)
    {
        float3 newValue = _verticies[index];
        newValue.y = _matrix[index] * _scale;
        _verticies[index] = newValue;
    }
}