using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class NoiseGenerator
{
    // Generate Noise for Terrain
    [BurstCompile]
    public struct GenerateTerrainNoise : IJobParallelFor
    {
        NativeArray<float> _terrainValues;

        public half _persistance;
        public half _lacunarity;

        [ReadOnly] public NativeArray<Vector2> _octaves;

        public Vector2Int _startPosition;
        public int _size;
        public float _scale;

        public float _minValue;
        public float _maxValue;

        public void Execute(int index)
        {
            float amplitude = 1, frequency = 1;
            float value = 0;

            int x = index % _size;
            int y = index / _size;

            float sampleX = ((x - _size / 2f) + _startPosition.x) / _scale * frequency;
            float sampleY = ((y - _size / 2f) + _startPosition.y) / _scale * frequency;

            foreach (float2 octave in _octaves)
            {
                //float sampleX = ((x - _size / 2) + _offset.x) / _scale * frequency + octave.x;
                //float sampleY = ((y - _size / 2) + _offset.y) / _scale * frequency + octave.y;


                float2 samplePosition = new(sampleX + octave.x, sampleY + octave.y);

                //float noise = Mathf.PerlinNoise(sampleX + octave.x, sampleY + octave.y) * 2 - 1;
                float outputNoise = noise.cnoise(samplePosition) * 2 - 1;

                value += outputNoise * amplitude;
                amplitude *= _persistance;
                frequency *= _lacunarity;

                //if (value < minValue) minValue = value;
                //if (value > maxValue) maxValue = value;
            }

            value = Mathf.InverseLerp(_minValue, _maxValue, value);
            _terrainValues[index] = value;
        }
    }
}
